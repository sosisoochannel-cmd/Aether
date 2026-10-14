using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Dedicated jump input. Its Image bounds are the exact touch target.</summary>
[RequireComponent(typeof(RectTransform), typeof(Image))]
public sealed class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Action { Jump }
    [SerializeField] private Action action;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(1f, 0.78f, 0.38f, 0.82f);
    private readonly HashSet<int> activePointers = new HashSet<int>();
    private Image targetImage;
    public static bool JumpHeld { get; private set; }
    public static bool JumpRequested { get; private set; }
    private static bool keyboardJumpHeld;
    private static readonly HashSet<TouchHoldButton> Instances = new HashSet<TouchHoldButton>();

    private void Awake() { targetImage = GetComponent<Image>(); targetImage.raycastTarget = true; targetImage.color = idleColor; }
    private void OnEnable() => Instances.Add(this);
    private void OnDisable() { activePointers.Clear(); Instances.Remove(this); RefreshState(); if (targetImage != null) targetImage.color = idleColor; }
    public void Configure(Action value) { action = value; if (targetImage != null) targetImage.color = idleColor; }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !activePointers.Add(eventData.pointerId)) return;
        if (action == Action.Jump) JumpRequested = true;
        RefreshState();
    }
    public void OnPointerUp(PointerEventData eventData) { if (activePointers.Remove(eventData.pointerId)) RefreshState(); }
    private void RefreshState()
    {
        bool held = activePointers.Count > 0;
        if (action == Action.Jump) JumpHeld = held || keyboardJumpHeld;
        if (targetImage != null) targetImage.color = held ? pressedColor : idleColor;
    }
    public static void RequestJump() => JumpRequested = true;
    public static void SetKeyboardJumpHeld(bool held)
    {
        keyboardJumpHeld = held;
        foreach (var instance in Instances) if (instance.action == Action.Jump) instance.RefreshState();
        if (Instances.Count == 0) JumpHeld = keyboardJumpHeld;
    }
    public static bool ConsumeJumpRequest() { bool requested = JumpRequested; JumpRequested = false; return requested; }
    public static void ResetAll()
    {
        foreach (var instance in Instances) { instance.activePointers.Clear(); if (instance.targetImage != null) instance.targetImage.color = instance.idleColor; }
        JumpHeld = false; JumpRequested = false; keyboardJumpHeld = false;
    }
}