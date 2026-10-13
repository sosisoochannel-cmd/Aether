using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedural animated title-screen atmosphere: radial depth, a restrained rift,
/// layered silhouettes, and slow drifting motes. It is intentionally lightweight
/// and contains no gameplay or navigation logic.
/// </summary>
[DisallowMultipleComponent]
public sealed class AetherMenuAtmosphere : MaskableGraphic
{
    private const int ArcSegments = 64;
    private const int MoteCount = 30;
    private float nextRefreshTime;
    private float phase;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
        color = Color.white;
        phase = 0f;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + 0.04f;
        phase += 0.04f;
        if (phase > 1000f) phase = 0f;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();
        if (r.width < 8f || r.height < 8f) return;

        Color upper = new Color(0.008f, 0.012f, 0.022f, 1f);
        Color lower = new Color(0.018f, 0.026f, 0.044f, 1f);
        AddQuad(vh,
            new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
            new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax),
            lower, lower, upper, upper);

        Vector2 portal = new Vector2(r.xMin + r.width * 0.735f, r.yMin + r.height * 0.54f);
        float pulse = 1f + Mathf.Sin(phase * 0.42f) * 0.006f;
        AddRadialGlow(vh, portal, r.width * 0.34f, r.height * 0.66f,
            new Color(0.11f, 0.16f, 0.24f, 0.22f), 52);
        AddRadialGlow(vh, portal + new Vector2(-r.width * 0.015f, r.height * 0.025f),
            r.width * 0.19f, r.height * 0.40f,
            new Color(0.34f, 0.25f, 0.12f, 0.075f), 40);

        // The rift is implied through broken arcs instead of a literal frame or card.
        AddEllipseArc(vh, portal, r.width * 0.266f * pulse, r.height * 0.475f * pulse,
            212f, 318f, 2.2f, new Color(0.90f, 0.72f, 0.39f, 0.40f));
        AddEllipseArc(vh, portal, r.width * 0.266f * pulse, r.height * 0.475f * pulse,
            22f, 116f, 1.5f, new Color(1f, 1f, 1f, 0.23f));
        AddEllipseArc(vh, portal, r.width * 0.222f, r.height * 0.394f,
            38f, 159f, 1.1f, new Color(0.78f, 0.83f, 0.90f, 0.15f));
        AddEllipseArc(vh, portal, r.width * 0.303f, r.height * 0.535f,
            128f, 194f, 1.0f, new Color(0.90f, 0.72f, 0.39f, 0.13f));

        DrawRiftCore(vh, portal, r.height);
        DrawMountainLayer(vh, r,
            new float[] { 0.24f, 0.31f, 0.26f, 0.39f, 0.29f, 0.33f, 0.23f, 0.37f, 0.28f, 0.34f, 0.25f, 0.31f, 0.23f },
            new Color(0.040f, 0.056f, 0.078f, 0.96f), new Color(0.026f, 0.037f, 0.057f, 1f));
        DrawMountainLayer(vh, r,
            new float[] { 0.12f, 0.18f, 0.14f, 0.23f, 0.16f, 0.20f, 0.13f, 0.24f, 0.15f, 0.19f, 0.12f, 0.17f, 0.11f },
            new Color(0.026f, 0.034f, 0.048f, 1f), new Color(0.014f, 0.021f, 0.034f, 1f));
        DrawMountainLayer(vh, r,
            new float[] { 0.075f, 0.11f, 0.085f, 0.13f, 0.095f, 0.12f, 0.08f, 0.14f, 0.09f, 0.12f, 0.075f, 0.10f, 0.07f },
            new Color(0.012f, 0.017f, 0.027f, 1f), new Color(0.006f, 0.010f, 0.018f, 1f));

        // A quiet, fine line anchors the menu side without boxing it in.
        float lineY = r.yMin + r.height * 0.806f;
        AddLine(vh, new Vector2(r.xMin + r.width * 0.072f, lineY),
            new Vector2(r.xMin + r.width * 0.335f, lineY), 1.15f,
            new Color(1f, 1f, 1f, 0.16f));
        AddLine(vh, new Vector2(r.xMin + r.width * 0.072f, lineY),
            new Vector2(r.xMin + r.width * 0.105f, lineY), 1.8f,
            new Color(0.90f, 0.72f, 0.39f, 0.58f));

        DrawMotes(vh, r);
        // Lower-edge depth keeps the foreground grounded on short and ultrawide screens.
        Color fade = new Color(0.004f, 0.007f, 0.013f, 0.48f);
        AddQuad(vh,
            new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
            new Vector2(r.xMax, r.yMin + r.height * 0.10f),
            new Vector2(r.xMin, r.yMin + r.height * 0.10f),
            fade, fade, new Color(fade.r, fade.g, fade.b, 0.02f),
            new Color(fade.r, fade.g, fade.b, 0.02f));
    }

    private void DrawRiftCore(VertexHelper vh, Vector2 center, float height)
    {
        int segments = 24;
        Vector2 previous = new Vector2(center.x, center.y - height * 0.30f);
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float y = center.y - height * 0.30f + height * 0.60f * t;
            float x = center.x + Mathf.Sin(t * Mathf.PI * 2.1f + phase * 0.11f) * height * 0.009f;
            Vector2 current = new Vector2(x, y);
            float visibility = Mathf.Sin(t * Mathf.PI);
            Color c = new Color(0.84f, 0.88f, 0.94f, 0.12f + visibility * 0.22f);
            AddLine(vh, previous, current, 1.25f, c);
            previous = current;
        }
        AddRadialGlow(vh, center + new Vector2(0f, height * 0.03f),
            height * 0.022f, height * 0.035f,
            new Color(0.91f, 0.74f, 0.43f, 0.21f), 16);
    }

    private void DrawMotes(VertexHelper vh, Rect r)
    {
        for (int i = 0; i < MoteCount; i++)
        {
            float seedX = Mathf.Repeat(i * 0.6180339f + 0.17f, 1f);
            float seedY = Mathf.Repeat(i * 0.7548776f + 0.08f, 1f);
            float xRange = 0.42f + seedX * 0.56f;
            float x = r.xMin + r.width * Mathf.Repeat(xRange + Mathf.Sin(phase * 0.20f + i * 1.7f) * 0.006f, 1f);
            float y = r.yMin + r.height * Mathf.Repeat(seedY * 0.87f + 0.06f + phase * (0.0011f + (i % 4) * 0.00035f), 0.94f);
            float size = 1.0f + (i % 4) * 0.65f;
            float flicker = 0.55f + 0.45f * Mathf.Sin(phase * (0.7f + (i % 3) * 0.17f) + i * 2.3f);
            float alpha = (0.045f + (i % 5 == 0 ? 0.14f : 0.045f)) * flicker;
            Color c = i % 5 == 0
                ? new Color(0.92f, 0.75f, 0.44f, alpha)
                : new Color(0.82f, 0.88f, 0.97f, alpha);
            Vector2 center = new Vector2(x, y);
            AddQuad(vh,
                center + new Vector2(0f, size * 1.8f),
                center + new Vector2(size, 0f),
                center + new Vector2(0f, -size * 1.8f),
                center + new Vector2(-size, 0f),
                Color.clear, c, Color.clear, c);
        }
    }

    private static void DrawMountainLayer(VertexHelper vh, Rect r, float[] heights, Color ridge, Color baseColor)
    {
        for (int i = 0; i < heights.Length - 1; i++)
        {
            float x0 = r.xMin + r.width * (i / (float)(heights.Length - 1));
            float x1 = r.xMin + r.width * ((i + 1) / (float)(heights.Length - 1));
            float y0 = r.yMin + r.height * heights[i];
            float y1 = r.yMin + r.height * heights[i + 1];
            AddQuad(vh,
                new Vector2(x0, r.yMin), new Vector2(x1, r.yMin),
                new Vector2(x1, y1), new Vector2(x0, y0),
                baseColor, baseColor, ridge, ridge);
        }
    }

    private static void AddRadialGlow(VertexHelper vh, Vector2 center, float radiusX, float radiusY, Color centerColor, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * Mathf.PI * 2f / segments;
            float a1 = (i + 1) * Mathf.PI * 2f / segments;
            Vector2 p0 = center + new Vector2(Mathf.Cos(a0) * radiusX, Mathf.Sin(a0) * radiusY);
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1) * radiusX, Mathf.Sin(a1) * radiusY);
            AddTriangle(vh, center, p0, p1, centerColor, Color.clear, Color.clear);
        }
    }

    private static void AddEllipseArc(VertexHelper vh, Vector2 center, float radiusX, float radiusY,
        float startDegrees, float endDegrees, float thickness, Color tint)
    {
        int segments = Mathf.Max(8, Mathf.RoundToInt(ArcSegments * Mathf.Abs(endDegrees - startDegrees) / 360f));
        float half = thickness * 0.5f;
        for (int i = 0; i < segments; i++)
        {
            float t0 = i / (float)segments;
            float t1 = (i + 1) / (float)segments;
            float a0 = Mathf.Lerp(startDegrees, endDegrees, t0) * Mathf.Deg2Rad;
            float a1 = Mathf.Lerp(startDegrees, endDegrees, t1) * Mathf.Deg2Rad;
            Vector2 outer0 = center + new Vector2(Mathf.Cos(a0) * (radiusX + half), Mathf.Sin(a0) * (radiusY + half));
            Vector2 outer1 = center + new Vector2(Mathf.Cos(a1) * (radiusX + half), Mathf.Sin(a1) * (radiusY + half));
            Vector2 inner1 = center + new Vector2(Mathf.Cos(a1) * (radiusX - half), Mathf.Sin(a1) * (radiusY - half));
            Vector2 inner0 = center + new Vector2(Mathf.Cos(a0) * (radiusX - half), Mathf.Sin(a0) * (radiusY - half));
            AddQuad(vh, outer0, outer1, inner1, inner0, tint, tint, tint, tint);
        }
    }

    private static void AddLine(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color tint)
    {
        Vector2 direction = end - start;
        if (direction.sqrMagnitude < 0.0001f) return;
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized * (thickness * 0.5f);
        AddQuad(vh, start + normal, end + normal, end - normal, start - normal, tint, tint, tint, tint);
    }

    private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc)
    {
        int index = vh.currentVertCount;
        vh.AddVert(a, ca, Vector2.zero);
        vh.AddVert(b, cb, Vector2.zero);
        vh.AddVert(c, cc, Vector2.zero);
        vh.AddTriangle(index, index + 1, index + 2);
    }

    private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d,
        Color ca, Color cb, Color cc, Color cd)
    {
        int index = vh.currentVertCount;
        vh.AddVert(a, ca, Vector2.zero);
        vh.AddVert(b, cb, Vector2.zero);
        vh.AddVert(c, cc, Vector2.zero);
        vh.AddVert(d, cd, Vector2.zero);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}
