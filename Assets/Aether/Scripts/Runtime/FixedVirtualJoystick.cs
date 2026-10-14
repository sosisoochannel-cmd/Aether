using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Fixed-origin virtual stick: the base stays put; only the knob follows the finger.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class FixedVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField, Range(0.15f, 0.5f)] private float travelRadiusRatio = 0.34f;
    [SerializeField, Range(0.05f, 0.5f)] private float deadZone = 0.12f;
    private RectTransform baseRect;
    private RectTransform knobRect;
    private int activePointerId = int.MinValue;
    public static float HorizontalAxis { get; private set; }

    private void Awake() => baseRect = GetComponent<RectTransform>();
    public void Configure(RectTransform knob) { baseRect = GetComponent<RectTransform>(); knobRect = knob; ResetStick(); }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || activePointerId != int.MinValue) return;
        activePointerId = eventData.pointerId;
        UpdateStick(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isActiveAndEnabled && eventData.pointerId == activePointerId) UpdateStick(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        activePointerId = int.MinValue;
        ResetStick();
    }

    private void UpdateStick(PointerEventData eventData)
    {
        if (baseRect == null || knobRect == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
        float radius = Mathf.Min(baseRect.rect.width, baseRect.rect.height) * travelRadiusRatio;
        if (radius <= 1f) return;
        Vector2 normalized = Vector2.ClampMagnitude(local / radius, 1f);
        knobRect.anchoredPosition = normalized * radius;
        HorizontalAxis = Mathf.Abs(normalized.x) < deadZone ? 0f : Mathf.Sign(normalized.x) * Mathf.InverseLerp(deadZone, 1f, Mathf.Abs(normalized.x));
    }

    private void ResetStick() { HorizontalAxis = 0f; if (knobRect != null) knobRect.anchoredPosition = Vector2.zero; }
    public static void ResetAll()
    {
        HorizontalAxis = 0f;
        foreach (var stick in FindObjectsOfType<FixedVirtualJoystick>()) { stick.activePointerId = int.MinValue; stick.ResetStick(); }
    }
    private void OnDisable() { if (activePointerId != int.MinValue) { activePointerId = int.MinValue; ResetStick(); } }
    private void OnApplicationFocus(bool hasFocus) { if (!hasFocus) ResetAll(); }
    private void OnApplicationPause(bool paused) { if (paused) ResetAll(); }
}