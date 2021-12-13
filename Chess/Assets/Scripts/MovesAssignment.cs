using System.Collections.Generic;
using UnityEngine;

public class MovesAssignment
{
    private enum _type
    {
        Attack,
        Move
    }

    private bool _OutOfBoard = false;
    private bool _AllyPieceInTheWay = false;
    private bool _EnemyPieceInTheWay = false;

    private int _MoveRange = new int();

    private Vector2Int _CurrentMoveUnit = new Vector2Int();
    private Vector2Int _CurrentMove = new Vector2Int();
    private Vector2Int _PiecePosition = new Vector2Int();
    private Vector2Int _TargetedTilePosition = new Vector2Int();

    private List<Vector2Int> _MovesToTest = new List<Vector2Int>();

    private BoardCell _TargetedCell = new BoardCell();

    public void GetAttackedTiles(List<GameObject> Pieces, PieceData.Color PlayerColor, ChessBoard ChessBoard)
    {

        foreach (GameObject CurrentPiece in Pieces)
        {
            Piece CurrentPieceData = CurrentPiece.GetComponent<Piece>();

            CurrentPieceData.Data.TilesAttacked.Clear();
            _MovesToTest.Clear();
            _PiecePosition = CurrentPieceData.Data.Position;
            _MovesToTest.AddRange(CurrentPieceData.Data.TakesWithRegularMoves ? CurrentPieceData.Data.RegularMoves : CurrentPieceData.Data.SpecialMoves);
            _MoveRange = CurrentPieceData.Data.TakesWithRegularMoves ? CurrentPieceData.Data.RegularMovesRange : CurrentPieceData.Data.SpecialMovesRange;

            CheckTile(CurrentPieceData, PlayerColor, ChessBoard, _type.Attack);
        }
    }

    public void GetMoveTiles(List<GameObject> Pieces, PieceData.Color PlayerColor, ChessBoard ChessBoard)
    {
        foreach (GameObject CurrentPiece in Pieces)
        {
            Piece CurrentPieceData = CurrentPiece.GetComponent<Piece>();

            CurrentPieceData.Data.TilesPlayable.Clear();

            if (!CurrentPieceData.Data.HasMovedAlready)
            {
                _MovesToTest.Clear();
                _MovesToTest.AddRange(CurrentPieceData.Data.FirstMoves);
                _MoveRange = CurrentPieceData.Data.FirstMovesRange;
                _PiecePosition = CurrentPieceData.Data.Position;

                CheckTile(CurrentPieceData, PlayerColor, ChessBoard, _type.Move);
            }

            if (CurrentPieceData.Data.TakesWithRegularMoves)
                CurrentPieceData.Data.TilesPlayable.AddRange(CurrentPieceData.Data.TilesAttacked);
            else
            {
                _MovesToTest.Clear();
                _MovesToTest.AddRange(CurrentPieceData.Data.RegularMoves);
                _MoveRange = CurrentPieceData.Data.RegularMovesRange;
                _PiecePosition = CurrentPieceData.Data.Position;

                CheckTile(CurrentPieceData, PlayerColor, ChessBoard, _type.Move);
            }
        }
    }

    private void CheckTile(Piece CurrentPieceData, PieceData.Color PlayerColor, ChessBoard ChessBoard, _type MoveType)
    {
        for (int MoveIndex = 0; MoveIndex < _MovesToTest.Count; MoveIndex++, _OutOfBoard = false, _AllyPieceInTheWay = false, _EnemyPieceInTheWay = false)
        {
            _CurrentMoveUnit = _MovesToTest[MoveIndex];

            for (int Range = 1; Range <= _MoveRange && !_OutOfBoard && !_AllyPieceInTheWay && !_EnemyPieceInTheWay; Range++)
            {
                _CurrentMove = _CurrentMoveUnit * Range;
                _TargetedTilePosition = _PiecePosition + _CurrentMove;

                if (_TargetedTilePosition.x > 7 || _TargetedTilePosition.y > 7 || _TargetedTilePosition.x < 0 || _TargetedTilePosition.y < 0)
                {
                    _OutOfBoard = true;
                }
                else
                {
                    _TargetedCell = ChessBoard.GetCell(_TargetedTilePosition);

                    if (_TargetedCell.Piece != null && _TargetedCell.Piece.Data.PieceColor == PlayerColor)
                    {
                        _AllyPieceInTheWay = true;
                    }
                    else
                    {
                        if (_TargetedCell.Piece != null && _TargetedCell.Piece.Data.PieceColor != PlayerColor)
                            _EnemyPieceInTheWay = true;
                        if (MoveType == _type.Attack)
                            CurrentPieceData.Data.TilesAttacked.Add(_TargetedCell);
                        else
                            CurrentPieceData.Data.TilesPlayable.Add(_TargetedCell);
                    }
                }
            }
        }
    }
}

