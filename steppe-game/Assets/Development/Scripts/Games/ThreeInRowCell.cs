using UnityEngine;

public class ThreeInRowCell : MonoBehaviour
{
    private ThreeInRowItem item;
    private Vector2Int gridPosition;
    private RectTransform rectTransform;

    public ThreeInRowItem Item => item;
    public Vector2Int GridPosition => gridPosition;
    public bool IsEmpty => item == null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(int col, int row)
    {
        gridPosition = new Vector2Int(col, row);
    }

    public void SetItem(ThreeInRowItem newItem)
    {
        item = newItem;

        if (item != null)
        {
            item.transform.SetParent(transform, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
        }
    }

    public void ClearItem()
    {
        item = null;
    }

    public Vector3 GetWorldPosition()
    {
        return rectTransform.position;
    }
}
