using UnityEngine;

public static class RectTransformExtensions
{
    // Сдвинуть pivot без визуального смещения элемента
    public static void SetPivotWithoutMoving(this RectTransform rt, Vector2 newPivot)
    {
        Vector2 deltaPivot = newPivot - rt.pivot;
        Vector3 deltaPos = new Vector3(deltaPivot.x * rt.rect.size.x, deltaPivot.y * rt.rect.size.y);
        rt.pivot = newPivot;
        rt.localPosition += deltaPos;
    }

    // Поставить pivot в центр (0.5, 0.5)
    public static void SetPivotToCenter(this RectTransform rt)
        => rt.SetPivotWithoutMoving(new Vector2(0.5f, 0.5f));

    // Поставить pivot в левый нижний угол
    public static void SetPivotToBottomLeft(this RectTransform rt)
        => rt.SetPivotWithoutMoving(Vector2.zero);

    // Поставить pivot в правый верхний угол
    public static void SetPivotToTopRight(this RectTransform rt)
        => rt.SetPivotWithoutMoving(Vector2.one);

    // Применить pivot ко всем дочерним элементам рекурсивно
    public static void SetPivotToAllChildren(this RectTransform rt, Vector2 pivot, bool recursive = true)
    {
        foreach (RectTransform child in rt)
        {
            child.SetPivotWithoutMoving(pivot);
            if (recursive && child.childCount > 0)
                child.SetPivotToAllChildren(pivot, recursive);
        }
    }
}