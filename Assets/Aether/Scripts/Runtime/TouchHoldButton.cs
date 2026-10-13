using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Pointer state for a large, accessible mobile action button.</summary>
public sealed class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Action { Left, Right, Jump }
    [SerializeField] private Action action;
    public static bool LeftHeld { get; private set; }
    public static bool RightHeld { get; private set; }
    public static bool JumpRequested { get; private set; }

    public static void ResetState()
    {
        LeftHeld = false;
        RightHeld = false;
        JumpRequested = false;
    }

    public void Configure(Action value) => action = value;
    public void OnPointerDown(PointerEventData eventData)
    {
        Set(true);
        if (action == Action.Jump) JumpRequested = true;
    }
    public void OnPointerUp(PointerEventData eventData) => Set(false);
    public void OnPointerExit(PointerEventData eventData) => Set(false);
    private void OnDisable() => Set(false);

    private void Set(bool pressed)
    {
        switch (action)
        {
            case Action.Left: LeftHeld = pressed; break;
            case Action.Right: RightHeld = pressed; break;
            case Action.Jump: break;
        }
    }

    public static bool ConsumeJump()
    {
        bool requested = JumpRequested;
        JumpRequested = false;
        return requested;
    }
}
