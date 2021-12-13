using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PieceData 
{
    public enum Type
    {
        Pawn,
        King,
        Queen,
        Bishop,
        Rook,
        Knight
    }

    public enum Color
    {
        White,
        Black
    }

    public Type PieceType;
    public Color PieceColor;
    public Vector2Int Position;

    [Space]
    [Space]
    public bool HasMovedAlready;
    public bool JumpOverPieces;
    public bool TakesWithRegularMoves;

    [Space]
    [Space]
    public List<Vector2Int> RegularMoves;
    public int RegularMovesRange;
    [Space]
    public List<Vector2Int> SpecialMoves;
    public int SpecialMovesRange;
    [Space]
    public List<Vector2Int> FirstMoves;
    public int FirstMovesRange;

    [Space]
    [Space]
    public List<BoardCell> TilesAttacked = new List<BoardCell>();
    public List<BoardCell> TilesPlayable = new List<BoardCell>();
}

[Serializable]
public class PiecePrefab
{
    public GameObject Prefab;
    public PieceData.Type Type;
    public PieceData.Color Color;
}
