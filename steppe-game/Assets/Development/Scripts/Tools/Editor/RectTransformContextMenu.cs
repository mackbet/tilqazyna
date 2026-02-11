using UnityEngine;
using UnityEditor;

public static class RectTransformContextMenu
{
    [MenuItem("CONTEXT/RectTransform/Set Anchors → To Current Position")]
    static void SetAnchorsToCurrentPosition(MenuCommand cmd)
    {
        var rt = (RectTransform)cmd.context;
        var parent = rt.parent as RectTransform;
        if (parent == null) return;

        Undo.RecordObject(rt, "Set Anchors To Current Position");

        Vector2 parentSize = parent.rect.size;
        Vector2 size = rt.rect.size;

        Vector2 pivotWorld = new Vector2(
            rt.anchorMin.x * parentSize.x + rt.offsetMin.x + size.x * rt.pivot.x,
            rt.anchorMin.y * parentSize.y + rt.offsetMin.y + size.y * rt.pivot.y
        );

        Vector2 anchor = pivotWorld / parentSize;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;

        rt.offsetMin = -new Vector2(size.x * rt.pivot.x,        size.y * rt.pivot.y);
        rt.offsetMax =  new Vector2(size.x * (1f - rt.pivot.x), size.y * (1f - rt.pivot.y));

        EditorUtility.SetDirty(rt);
    }

    [MenuItem("CONTEXT/RectTransform/Set Anchors → To Object Edges")]
    static void SetAnchorsToObjectEdges(MenuCommand cmd)
    {
        var rt = (RectTransform)cmd.context;
        var parent = rt.parent as RectTransform;
        if (parent == null) return;

        Undo.RecordObject(rt, "Set Anchors To Object Edges");

        Vector2 parentSize = parent.rect.size;
        Vector2 size = rt.rect.size;

        // Левый нижний угол элемента в координатах родителя
        Vector2 corner = new Vector2(
            rt.anchorMin.x * parentSize.x + rt.offsetMin.x,
            rt.anchorMin.y * parentSize.y + rt.offsetMin.y
        );

        rt.anchorMin = corner / parentSize;
        rt.anchorMax = (corner + size) / parentSize;

        // Offsets обнуляются — размер теперь задаётся якорями
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        EditorUtility.SetDirty(rt);
    }
}