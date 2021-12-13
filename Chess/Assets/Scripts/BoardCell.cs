using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardCell : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int Position;
    public Piece Piece;
    public Action<BoardCell> OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(this);
    }
}
