using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Drag-and-resize settings for gameplay controls, persisted only after Save.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class GameplayControlHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform target;
    private string controlId;
    private Vector2 defaultAnchor;
    private Vector2 defaultSize;
    private float scaleFactor = 1f;
    private bool editing;
    private int activePointer = int.MinValue;
    private Outline selectionOutline;
    public string ControlId => controlId;

    public void Configure(string id, Vector2 initialAnchor, Vector2 initialSize, bool isEditing)
    {
        target = GetComponent<RectTransform>(); controlId = id; defaultAnchor = initialAnchor; defaultSize = initialSize; editing = isEditing;
        if (selectionOutline == null) { selectionOutline = GetComponent<Outline>(); if (selectionOutline == null) selectionOutline = gameObject.AddComponent<Outline>(); }
        selectionOutline.effectColor = new Color(0.94f, 0.70f, 0.27f, 0.95f);
        selectionOutline.effectDistance = new Vector2(3f, -3f);
        selectionOutline.enabled = false;
        ApplySavedLayout();
    }

    public static void ApplyLayout(RectTransform rect, string id, Vector2 defaultAnchor, Vector2 defaultSize)
    {
        if (rect == null) return;
        Vector2 anchor = ClampAnchor(new Vector2(PlayerPrefs.GetFloat(Key(id, "x"), defaultAnchor.x), PlayerPrefs.GetFloat(Key(id, "y"), defaultAnchor.y)));
        float scale = Mathf.Clamp(PlayerPrefs.GetFloat(Key(id, "scale"), 1f), 0.65f, 1.5f);
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = defaultSize; rect.localScale = Vector3.one * scale; rect.anchoredPosition = Vector2.zero;
    }
    private void ApplySavedLayout()
    {
        if (target == null) return;
        ApplyLayout(target, controlId, defaultAnchor, defaultSize);
        scaleFactor = Mathf.Clamp(PlayerPrefs.GetFloat(Key(controlId, "scale"), 1f), 0.65f, 1.5f);\n        target.localScale = Vector3.one * scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData) { if (!editing || activePointer != int.MinValue) return; activePointer = eventData.pointerId; MoveToPointer(eventData); }
    public void OnDrag(PointerEventData eventData) { if (editing && eventData.pointerId == activePointer) MoveToPointer(eventData); }
    public void OnEndDrag(PointerEventData eventData) { if (eventData.pointerId == activePointer) activePointer = int.MinValue; }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (target == null || !(target.parent is RectTransform parent)) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
        Rect bounds = parent.rect; if (bounds.width <= 1f || bounds.height <= 1f) return;
        Vector2 anchor = ClampAnchor(new Vector2((local.x - bounds.xMin) / bounds.width, (local.y - bounds.yMin) / bounds.height));
        target.anchorMin = anchor; target.anchorMax = anchor; target.anchoredPosition = Vector2.zero;
    }
    private static Vector2 ClampAnchor(Vector2 anchor) => new Vector2(Mathf.Clamp(anchor.x, 0.11f, 0.89f), Mathf.Clamp(anchor.y, 0.16f, 0.84f));
    public void ChangeScale(float delta) { if (!editing || target == null) return; scaleFactor = Mathf.Clamp(scaleFactor + delta, 0.65f, 1.5f); target.localScale = Vector3.one * scaleFactor; }

    public static void SetSelectedControl(string id)
    {
        foreach (var handle in FindObjectsOfType<GameplayControlHandle>())
            if (handle.selectionOutline != null) handle.selectionOutline.enabled = handle.editing && handle.controlId == id;
    }
    public static GameplayControlHandle Find(string id) { foreach (var handle in FindObjectsOfType<GameplayControlHandle>()) if (handle.controlId == id) return handle; return null; }
    public static void SaveAll()
    {
        foreach (var handle in FindObjectsOfType<GameplayControlHandle>())
        {
            if (handle.target == null) continue;
            Vector2 anchor = handle.target.anchorMin;
            PlayerPrefs.SetFloat(Key(handle.controlId, "x"), anchor.x); PlayerPrefs.SetFloat(Key(handle.controlId, "y"), anchor.y);
            PlayerPrefs.SetFloat(Key(handle.controlId, "scale"), handle.scaleFactor);
        }
        PlayerPrefs.Save();
    }
    public static void ResetAll()
    {
        foreach (string id in new[] { "joystick", "jump" }) { PlayerPrefs.DeleteKey(Key(id, "x")); PlayerPrefs.DeleteKey(Key(id, "y")); PlayerPrefs.DeleteKey(Key(id, "scale")); }
        PlayerPrefs.Save();
    }
    private static string Key(string id, string suffix) => "Aether.Controls." + id + "." + suffix;
    private void OnDisable() { activePointer = int.MinValue; }
}