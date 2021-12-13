using PlayerIOClient;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{

    private Connection pioconnection;
    private List<Message> msgList = new List<Message>(); //  Messsage queue implementation
    private bool joinedroom = false;
    public static MainGame Instance;

    public List<PieceData> StartPieces;
    public List<GameObject> Pieces = new List<GameObject>();
    public PiecePrefab[] PrefabPieces;
    public ChessBoard ChessBoard;
    public Color SelectedTileColor;
    public Color PlayableTileColor;
    private Color _LightSquare, _DarkSquare;
    private MovesAssignment MovesAssignment = new MovesAssignment();
    public Transform PieceFolder;
    public PieceData.Color PlayerColor;

    private bool _isMoving = new bool();

    enum GameStep
    {
        None,
        PieceSelected
    }

    GameStep _step;
    Piece _currentPiece;
    BoardCell _currentCell;


    private void Awake()
    {
        Instance = this;
        PlayerColor = PieceData.Color.White;
    }

    private string infomsg = "";

    void Start()
    {
        Application.runInBackground = true;

        ChessBoard.Initialize();
        _DarkSquare = ChessBoard.GetCell(0, 0).gameObject.GetComponent<Renderer>().material.color;
        _LightSquare = ChessBoard.GetCell(0, 1).gameObject.GetComponent<Renderer>().material.color;
        ChessBoard.OnBoardCellClicked += OnCellClicked;

        foreach (var pieceData in StartPieces)
        {
            GameObject pieceGo = GameObject.Instantiate(GetPrefab(pieceData),
                ChessBoard.GetWorldPosition(pieceData.Position), Quaternion.identity);
            pieceGo.gameObject.transform.SetParent(PieceFolder);
            Piece piece = pieceGo.GetComponent<Piece>();
            piece.Data.Position = pieceData.Position;
            BoardCell cell = ChessBoard.GetCell(piece.Data.Position);
            cell.Piece = piece;
            Pieces.Add(pieceGo);
        }

        // Create a random userid 
        System.Random random = new System.Random();
        string userid = "Guest" + random.Next(0, 10000);

        Debug.Log("Starting");

        PlayerIO.Authenticate(
            "chessproject-i2vst411ek6midmlmaonza",            //Your game id
            "public",                               //Your connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", userid },
            },
            null,                                   //PlayerInsight segments
            delegate (Client client)
            {
                Debug.Log("Successfully connected to Player.IO");
                infomsg = "Successfully connected to Player.IO";

                Debug.Log("Create ServerEndpoint");
                // Comment out the line below to use the live servers instead of your development server
                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                Debug.Log("CreateJoinRoom");
                //Create or join the room 
                client.Multiplayer.CreateJoinRoom(
                    "UnityDemoRoom",                    //Room id. If set to null a random roomid is used
                    "UnityMushrooms",                   //The room type started on the server
                    true,                               //Should the room be visible in the lobby?
                    null,
                    null,
                    delegate (Connection connection)
                    {
                        Debug.Log("Joined Room.");
                        infomsg = "Joined Room.";
                        // We successfully joined a room so set up the message handler
                        pioconnection = connection;
                        pioconnection.OnMessage += handlemessage;
                        joinedroom = true;
                    },
                    delegate (PlayerIOError error)
                    {
                        Debug.Log("Error Joining Room: " + error.ToString());
                        infomsg = error.ToString();
                    }
                );
            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error connecting: " + error.ToString());
                infomsg = error.ToString();
            }
        );

        MovesAssignment.GetAttackedTiles(Pieces, PlayerColor, ChessBoard);
        MovesAssignment.GetMoveTiles(Pieces, PlayerColor, ChessBoard);

    }

    void handlemessage(object sender, Message m)
    {
        msgList.Add(m);
    }

    void FixedUpdate()
    {
        // process message queue
        foreach (Message m in msgList)
        {
            switch (m.Type)
            {
                case "PlayerJoined":

                    break;
                case "Move":
                    Vector2Int StartTile = new Vector2Int();
                    Vector2Int ArrivalTile = new Vector2Int();
                    StartTile.x = m.GetInt(0);
                    StartTile.y = m.GetInt(1);
                    ArrivalTile.x = m.GetInt(2);
                    ArrivalTile.y = m.GetInt(3);
                    MovePiece(ChessBoard.GetCell(StartTile), ChessBoard.GetCell(ArrivalTile));
                    break;



            }
        }

        // clear message queue after it's been processed
        msgList.Clear();
    }

    void OnCellClicked(BoardCell cell)
    {
        if (_step == GameStep.None)
        {
            SelectPiece(cell);
        }
        else if (_step == GameStep.PieceSelected)
        {
            if (cell.Piece != null && cell.Piece.Data.PieceColor == PlayerColor)
            {
                _currentCell.gameObject.GetComponent<Renderer>().material.color =
                                        (_currentCell.Position.x + _currentCell.Position.y) % 2 == 0 ? _DarkSquare : _LightSquare;
                RemovePotentialTiles(_currentPiece);
                if (!cell.Piece == _currentPiece)
                    SelectPiece(cell);
                else
                    _step = GameStep.None;
            }
            else
            {
                if (_currentPiece.Data.TilesPlayable.Contains(ChessBoard.GetCell(cell.Position))) 
                {
                    _isMoving = true;
                    pioconnection.Send("Move", _currentPiece.Data.Position.x, _currentPiece.Data.Position.y, cell.Position.x, cell.Position.y);
                }
                else
                {
                    _step = GameStep.None;
                    CleanTile(_currentCell);
                    RemovePotentialTiles(_currentPiece);
                    Debug.Log("Move not Legal");
                }
            }
        }
    }

    private void SelectPiece(BoardCell cell)
    {
        if (cell.Piece != null && cell.Piece.Data.PieceColor == PlayerColor)
        {
            _currentPiece = cell.Piece;
            _currentCell = cell;
            _step = GameStep.PieceSelected;
            cell.gameObject.GetComponent<Renderer>().material.color = SelectedTileColor;
            foreach(BoardCell PlayableTile in _currentPiece.Data.TilesPlayable)
            {
                PlayableTile.gameObject.GetComponent<Renderer>().material.color = PlayableTileColor;
            }
        }
    }

    private void MovePiece(BoardCell startcell, BoardCell arrivalcell)
    {
        Piece pieceToMove = startcell.Piece;
        if (_isMoving)
        {
            _isMoving = false;
            RemovePotentialTiles(pieceToMove);
            CleanTile(startcell);
        }
        startcell.Piece = null;

        if (arrivalcell.Piece != null)
        {
            TakePiece(arrivalcell);
        }

        if (!pieceToMove.Data.HasMovedAlready) { pieceToMove.Data.HasMovedAlready = true; }
        pieceToMove.transform.position = arrivalcell.transform.position;
        arrivalcell.Piece = pieceToMove;
        arrivalcell.Piece.Data.Position = arrivalcell.Position;

        _currentPiece = null;
        _currentCell = null;
        _step = GameStep.None;
        MovesAssignment.GetAttackedTiles(Pieces, PlayerColor, ChessBoard);
        MovesAssignment.GetMoveTiles(Pieces, PlayerColor, ChessBoard);
    }

    private GameObject GetPrefab(PieceData data)
    {
        foreach (var prefabPiece in PrefabPieces)
        {
            if (data.PieceType == prefabPiece.Type && data.PieceColor == prefabPiece.Color)
                return prefabPiece.Prefab;
        }

        throw new System.Exception($"Can't find type {data.PieceType} and {data.PieceColor}");
    }

    private void TakePiece(BoardCell arrivalCell)
    {
        foreach(GameObject Piece in Pieces)
        {
            if(Piece.GetComponent<Piece>().Data.Position == arrivalCell.Position)
            {
                Pieces.Remove(Piece);
                Destroy(Piece);
                return;
            }
        }
    }

    private void RemovePotentialTiles(Piece piece)
    {
        foreach (BoardCell Tile in piece.Data.TilesPlayable)
        {
            CleanTile(Tile);
        }
    }

    private void CleanTile(BoardCell Tile)
    {
        Tile.gameObject.GetComponent<Renderer>().material.color = (Tile.Position.x + Tile.Position.y) % 2 == 0 ? _DarkSquare : _LightSquare;
    }

}
