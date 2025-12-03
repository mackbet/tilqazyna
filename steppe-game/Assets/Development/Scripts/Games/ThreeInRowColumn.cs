using System.Collections.Generic;
using UnityEngine;

public class ThreeInRowColumn : MonoBehaviour
{
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private float itemSpacing = 100f;

    private List<ThreeInRowItem> items = new List<ThreeInRowItem>();
    private int columnIndex;

    public int ColumnIndex => columnIndex;
    public int ItemCount => items.Count;

    public void Initialize(int index)
    {
        columnIndex = index;
    }

    public void AddItem(ThreeInRowItem item, int rowIndex)
    {
        if (rowIndex < items.Count)
        {
            items[rowIndex] = item;
        }
        else
        {
            items.Add(item);
        }

        item.transform.SetParent(itemsContainer != null ? itemsContainer : transform, false);
        UpdateItemPosition(item, rowIndex);
    }

    public ThreeInRowItem GetItem(int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < items.Count)
        {
            return items[rowIndex];
        }
        return null;
    }

    public void SetItem(int rowIndex, ThreeInRowItem item)
    {
        if (rowIndex >= 0 && rowIndex < items.Count)
        {
            items[rowIndex] = item;
            if (item != null)
            {
                item.transform.SetParent(itemsContainer != null ? itemsContainer : transform, false);
                UpdateItemPosition(item, rowIndex);
            }
        }
    }

    public void RemoveItem(int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < items.Count)
        {
            items[rowIndex] = null;
        }
    }

    public void UpdateItemPosition(ThreeInRowItem item, int rowIndex)
    {
        if (item == null) return;

        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Позиция по вертикали (0 внизу, rows-1 вверху)
            float yPos = rowIndex * itemSpacing;
            rectTransform.anchoredPosition = new Vector2(0, yPos);
        }
    }

    public void AnimateItemMove(ThreeInRowItem item, int targetRow, float duration = 0.3f)
    {
        if (item == null) return;

        float yPos = targetRow * itemSpacing;
        item.AnimateMove(new Vector2(0, yPos), duration);
    }

    public List<ThreeInRowItem> GetAllItems()
    {
        return new List<ThreeInRowItem>(items);
    }

    public void Clear()
    {
        items.Clear();
    }
}
