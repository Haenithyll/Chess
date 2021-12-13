using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    private string _TileName = string.Empty;

    public GameObject PrefabCell;
    public Sprite CellAlternate;
    public int Width = 8,Height = 8;
    public float CellSize = 1.28f;
    public Transform BoardStart;

    public Action<BoardCell> OnBoardCellClicked;

    BoardCell[,] _grid;

    private void Awake()
    {
        _grid = new BoardCell[Width, Height];
    }

    public void Initialize()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++, _TileName = string.Empty)
            {
                _TileName = (char)(x+65) + (y+1).ToString();

                Vector3 position = GetWorldPosition(x, y);
                GameObject cellGo = GameObject.Instantiate(PrefabCell, position, Quaternion.identity);
                cellGo.transform.name = _TileName;
                BoardCell cell = cellGo.GetComponent<BoardCell>();
                cell.Position = new Vector2Int(x, y);
                _grid[x, y] = cell;
                cell.OnClick += OnCellClicked;

                if ((x + y) % 2 == 1)
                    cellGo.GetComponent<SpriteRenderer>().sprite = CellAlternate;

                cellGo.transform.SetParent(transform);
            }
        }
    }

    void OnCellClicked( BoardCell cell )
    {
        OnBoardCellClicked?.Invoke(cell);
    }


    public BoardCell GetCell(int xGrid, int yGrid)
    {
        return _grid[xGrid, yGrid];
    }

    public BoardCell GetCell(Vector2Int gridPosition)
    {
        return GetCell(gridPosition.x, gridPosition.y);
    }

    public Vector3 GetWorldPosition(int xGrid , int yGrid)
    {
        return new Vector3(xGrid * CellSize + BoardStart.transform.position.x,
                           yGrid * CellSize + BoardStart.transform.position.y, 0);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return GetWorldPosition(gridPosition.x, gridPosition.y);
    }


}
