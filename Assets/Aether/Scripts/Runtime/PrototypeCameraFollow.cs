using UnityEngine;

/// <summary>Stable follow camera with horizontal look-ahead and bounded vertical framing.</summary>
public sealed class PrototypeCameraFollow : MonoBehaviour
{
    public Transform Target { get; set; }

    [SerializeField, Min(0.01f)] private float horizontalSmoothTime = 0.16f;
    [SerializeField, Min(0.01f)] private float verticalSmoothTime = 0.22f;
    [SerializeField, Min(0f)] private float horizontalLookAhead = 1.35f;
    [SerializeField] private float minimumY = 0.2f;
    [SerializeField] private float maximumY = 3.6f;

    private Vector3 velocity;
    private float lastTargetX;
    private float facing = 1f;

    private void LateUpdate()
    {
        if (Target == null) return;

        float deltaX = Target.position.x - lastTargetX;
        if (Mathf.Abs(deltaX) > 0.015f) facing = Mathf.Sign(deltaX);
        lastTargetX = Target.position.x;

        Vector3 current = transform.position;
        float desiredX = Target.position.x + facing * horizontalLookAhead;
        float desiredY = Mathf.Clamp(Target.position.y + 1.75f, minimumY, maximumY);

        current.x = Mathf.SmoothDamp(current.x, desiredX, ref velocity.x, horizontalSmoothTime);
        current.y = Mathf.SmoothDamp(current.y, desiredY, ref velocity.y, verticalSmoothTime);
        current.z = -10f;
        transform.position = current;
    }
}
