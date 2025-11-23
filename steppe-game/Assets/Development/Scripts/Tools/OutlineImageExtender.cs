using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OutlineImageExtender : BaseMeshEffect
{
    [SerializeField] private float _padding = 10f;
    
    public float Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            graphic.SetVerticesDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        var verts = new System.Collections.Generic.List<UIVertex>();
        vh.GetUIVertexStream(verts);

        if (verts.Count == 0) return;

        // Находим границы UV и позиций
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        float minU = float.MaxValue, minV = float.MaxValue;
        float maxU = float.MinValue, maxV = float.MinValue;

        foreach (var v in verts)
        {
            minX = Mathf.Min(minX, v.position.x);
            maxX = Mathf.Max(maxX, v.position.x);
            minY = Mathf.Min(minY, v.position.y);
            maxY = Mathf.Max(maxY, v.position.y);
            minU = Mathf.Min(minU, v.uv0.x);
            maxU = Mathf.Max(maxU, v.uv0.x);
            minV = Mathf.Min(minV, v.uv0.y);
            maxV = Mathf.Max(maxV, v.uv0.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float uRange = maxU - minU;
        float vRange = maxV - minV;

        // Расширяем UV за пределы [0,1]
        float uPadding = (uRange / width) * _padding;
        float vPadding = (vRange / height) * _padding;

        vh.Clear();

        var color = graphic.color;
        
        // Создаём расширенный квад
        var v0 = new UIVertex { position = new Vector3(minX - _padding, minY - _padding), color = color, uv0 = new Vector2(minU - uPadding, minV - vPadding) };
        var v1 = new UIVertex { position = new Vector3(minX - _padding, maxY + _padding), color = color, uv0 = new Vector2(minU - uPadding, maxV + vPadding) };
        var v2 = new UIVertex { position = new Vector3(maxX + _padding, maxY + _padding), color = color, uv0 = new Vector2(maxU + uPadding, maxV + vPadding) };
        var v3 = new UIVertex { position = new Vector3(maxX + _padding, minY - _padding), color = color, uv0 = new Vector2(maxU + uPadding, minV - vPadding) };

        vh.AddVert(v0);
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }
}