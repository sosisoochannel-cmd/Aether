using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small unscaled entrance animation for menu text and controls. Invisible button plates
/// stay untappable, then become active shortly after their fade/slide becomes visible.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(Graphic))]
public sealed class AetherUIReveal : MonoBehaviour
{
    private RectTransform rect;
    private Graphic graphic;
    private bool isButtonGraphic;
    private Color finalColor;
    private Vector2 finalPosition;
    private Vector3 finalScale;
    private float delay;
    private float duration = 0.34f;
    private Vector2 offset = new Vector2(0f, -16f);
    private bool configured;
    public bool IsComplete { get; private set; }
    public static bool MotionEnabled { get; set; } = true;

    public void Begin(float delaySeconds, Vector2 slideOffset, float durationSeconds = 0.34f)
    {
        if (configured) return;
        rect = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();
        isButtonGraphic = GetComponent<Button>() != null;
        if (rect == null || graphic == null) return;

        delay = Mathf.Max(0f, delaySeconds);
        offset = slideOffset;
        duration = Mathf.Max(0.08f, durationSeconds);
        finalColor = graphic.color;
        finalPosition = rect.anchoredPosition;
        finalScale = rect.localScale;
        configured = true;
        IsComplete = false;
        graphic.raycastTarget = false;

        if (!MotionEnabled)
        {
            rect.anchoredPosition = finalPosition;
            rect.localScale = finalScale;
            graphic.color = finalColor;
            graphic.raycastTarget = isButtonGraphic;
            IsComplete = true;
            return;
        }

        Color hidden = finalColor;
        hidden.a = 0f;
        graphic.color = hidden;
        rect.anchoredPosition = finalPosition + offset;
        rect.localScale = finalScale * 0.985f;
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        float elapsed = 0f;
        while (elapsed < duration && this != null && rect != null && graphic != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            rect.anchoredPosition = Vector2.Lerp(finalPosition + offset, finalPosition, eased);
            rect.localScale = Vector3.Lerp(finalScale * 0.985f, finalScale, eased);
            Color c = finalColor;
            c.a = finalColor.a * eased;
            graphic.color = c;
            // Do not expose an invisible hit target; enable the button once the plate is visibly entering.
            if (isButtonGraphic && t >= 0.18f) graphic.raycastTarget = true;
            yield return null;
        }

        if (rect != null)
        {
            rect.anchoredPosition = finalPosition;
            rect.localScale = finalScale;
        }
        if (graphic != null)
        {
            graphic.color = finalColor;
            graphic.raycastTarget = isButtonGraphic;
        }
        IsComplete = true;
    }
}
