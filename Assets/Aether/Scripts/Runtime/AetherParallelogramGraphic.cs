using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight black-and-gold menu plate. Its rendered silhouette and raycast shape
/// share the same parallelogram geometry, so transparent corner areas do not steal taps.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public sealed class AetherParallelogramGraphic : MaskableGraphic
{
    [SerializeField, Range(0.02f, 0.22f)] private float skewRatio = 0.12f;
    [SerializeField, Min(0f)] private float outlineThickness = 1.5f;
    [SerializeField] private Color fillTint = new Color(0.86f, 0.67f, 0.29f, 0.12f);
    [SerializeField] private Color outlineTint = new Color(0.86f, 0.67f, 0.29f, 0.55f);

    public void Configure(Color fill, Color outline, float skew, float thickness)
    {
        fillTint = fill;
        outlineTint = outline;
        skewRatio = Mathf.Clamp(skew, 0.02f, 0.22f);
        outlineThickness = Mathf.Max(0f, thickness);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f) return;

        float skew = Mathf.Min(rect.width * skewRatio, rect.width * 0.24f);
        Vector2 bottomLeft = new Vector2(rect.xMin + skew, rect.yMin);
        Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);
        Vector2 topRight = new Vector2(rect.xMax - skew, rect.yMax);
        Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);

        AddQuad(vh, bottomLeft, bottomRight, topRight, topLeft, fillTint);
        float thickness = Mathf.Min(outlineThickness, Mathf.Min(rect.width, rect.height) * 0.08f);
        if (thickness <= 0f || outlineTint.a <= 0f) return;

        AddInsetEdge(vh, bottomLeft, bottomRight, thickness, outlineTint);
        AddInsetEdge(vh, bottomRight, topRight, thickness, outlineTint);
        AddInsetEdge(vh, topRight, topLeft, thickness, outlineTint);
        AddInsetEdge(vh, topLeft, bottomLeft, thickness, outlineTint);
    }

    public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
    {
        if (!base.Raycast(screenPoint, eventCamera)) return false;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPoint, eventCamera, out Vector2 localPoint))
            return false;

        Rect rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f || !rect.Contains(localPoint))
            return false;

        float t = Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height);
        float skew = Mathf.Min(rect.width * skewRatio, rect.width * 0.24f);
        float left = rect.xMin + skew * (1f - t);
        float right = rect.xMax - skew * t;
        return localPoint.x >= left && localPoint.x <= right;
    }

    private static void AddInsetEdge(
        VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color tint)
    {
        Vector2 direction = end - start;
        Vector2 inward = new Vector2(-direction.y, direction.x).normalized * thickness;
        AddQuad(vh, start, end, end + inward, start + inward, tint);
    }

    private static void AddQuad(
        VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
    {
        vh.AddUIVertexQuad(new[]
        {
            MakeVertex(a, tint),
            MakeVertex(b, tint),
            MakeVertex(c, tint),
            MakeVertex(d, tint)
        });
    }

    private static UIVertex MakeVertex(Vector2 position, Color tint)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = tint;
        return vertex;
    }
}
