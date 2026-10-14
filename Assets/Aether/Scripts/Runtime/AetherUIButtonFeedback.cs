using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Quiet tactile response for a UI button: a tiny press-in scale, soft pointer focus,
/// and independent pointer tracking. It never changes the button label's white color.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(Button))]
public sealed class AetherUIButtonFeedback : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    private readonly HashSet<int> pressedPointers = new HashSet<int>();
    private RectTransform rect;
    private Button button;
    public static bool MotionEnabled { get; set; } = true;
    private AetherUIReveal reveal;
    private Vector3 baseScale;
    private bool pointerInside;
    private bool selected;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        reveal = GetComponent<AetherUIReveal>();
        baseScale = rect != null ? rect.localScale : Vector3.one;
    }

    private void Update()
    {
        if (reveal == null) reveal = GetComponent<AetherUIReveal>();
        if (rect == null || (reveal != null && !reveal.IsComplete)) return;
        if (!MotionEnabled)
        {
            rect.localScale = baseScale;
            return;
        }
        float scale = pressedPointers.Count > 0 ? 0.975f : ((pointerInside || selected) ? 1.012f : 1f);
        float blend = 1f - Mathf.Exp(-16f * Time.unscaledDeltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, baseScale * scale, blend);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable) pressedPointers.Add(eventData.pointerId);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressedPointers.Remove(eventData.pointerId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable) pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    private void OnDisable()
    {
        pressedPointers.Clear();
        pointerInside = false;
        selected = false;
        if (rect != null) rect.localScale = baseScale;
    }
}
