using UnityEngine;

/// <summary>Soft follow camera for the first movement test.</summary>
public sealed class PrototypeCameraFollow : MonoBehaviour
{
    public Transform Target { get; set; }
    [SerializeField, Min(0.1f)] private float smoothTime = 0.18f;
    private Vector3 velocity;

    private void LateUpdate()
    {
        if (Target == null) return;
        Vector3 desired = new Vector3(Target.position.x + 1.5f, Mathf.Clamp(Target.position.y + 2.0f, 0.5f, 3.5f), -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
