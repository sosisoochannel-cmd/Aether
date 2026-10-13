using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Isolated movement controller for the first playable test room.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PrototypePlayerMotor : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 6.2f;
    [SerializeField, Min(0.1f)] private float jumpImpulse = 11.5f;
    private Rigidbody2D body;
    private bool grounded;
    private float horizontal;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        horizontal = (TouchHoldButton.LeftHeld ? -1f : 0f) + (TouchHoldButton.RightHeld ? 1f : 0f);
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) horizontal += 1f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) TouchHoldButton.ConsumeJump();
        }
#endif
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        if (TouchHoldButton.ConsumeJump() && grounded)
        {
            body.velocity = new Vector2(body.velocity.x, 0f);
            body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
            grounded = false;
        }
    }

    private void FixedUpdate() => body.velocity = new Vector2(horizontal * moveSpeed, body.velocity.y);
    private void OnCollisionEnter2D(Collision2D collision) => CheckGround(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckGround(collision);
    private void OnCollisionExit2D(Collision2D collision) => grounded = false;

    private void CheckGround(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
            if (collision.GetContact(i).normal.y > 0.55f) { grounded = true; return; }
    }
}
