using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public Item Item { get; private set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }


    public bool IsEmpty => Item == null;

    public void Setup(int cellX, int cellY)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }


    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        if (item == null)//
        {
            Debug.LogError("Item is null! Cannot assign to cell.");
            return;
        }//
        Item = item;
        Item.SetCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }

    //internal void AnimateItemForHint()
    //{
    //    if (Item == null) return;
    //    Item.AnimateForHint();
    //}

    //internal void StopHintAnimation()
    //{
    //    if (Item == null) return; // Nếu Item null, không làm gì cả
    //    Item.StopAnimateForHint();
    //}

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
    /////////////////////////////
    private void OnMouseDown()//
    {
        // Kiểm tra nếu có item trong ô
        if (Item == null) return;

        // Kiểm tra nếu item là NormalItem
        if (Item is NormalItem)
        {
            HandleCellTapped();
        }
    }
    private void HandleCellTapped()
    {
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnItemTapped(this);
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
    }
    ///////////////////////////
}
