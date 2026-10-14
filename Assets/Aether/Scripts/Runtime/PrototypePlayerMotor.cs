using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Responsive 2D platformer movement: acceleration, air steering, jump buffering,
/// coyote time, variable jump height, and stronger falling gravity.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PrototypePlayerMotor : MonoBehaviour
{
    [Header("Horizontal movement")]
    [SerializeField, Min(0.1f)] private float maxMoveSpeed = 6.2f;
    [SerializeField, Min(0.1f)] private float groundAcceleration = 46f;
    [SerializeField, Min(0.1f)] private float airAcceleration = 23f;
    [SerializeField, Min(0.1f)] private float groundDeceleration = 55f;

    [Header("Jump feel")]
    [SerializeField, Min(0.1f)] private float jumpVelocity = 11.5f;
    [SerializeField, Range(0f, 0.3f)] private float coyoteTime = 0.12f;
    [SerializeField, Range(0f, 0.3f)] private float jumpBufferTime = 0.13f;
    [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.48f;
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.65f;
    [SerializeField, Min(1f)] private float maximumFallSpeed = 18f;

    private Rigidbody2D body;
    private float horizontalInput;
    private float lastGroundedTime = float.NegativeInfinity;
    private float jumpBufferedUntil = float.NegativeInfinity;
    private float baseGravityScale;
    private bool grounded;
    private bool jumpCutApplied;
    private bool jumpWasHeld;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        baseGravityScale = body.gravityScale;
    }

    private void Update()
    {
        horizontalInput = FixedVirtualJoystick.HorizontalAxis;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) horizontalInput -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) horizontalInput += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) TouchHoldButton.RequestJump();
            TouchHoldButton.SetKeyboardJumpHeld(keyboard.spaceKey.isPressed);
        }
        else
        {
            TouchHoldButton.SetKeyboardJumpHeld(false);
        }
#endif

        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        if (TouchHoldButton.ConsumeJumpRequest())
            jumpBufferedUntil = Time.time + jumpBufferTime;

        if (grounded)
            lastGroundedTime = Time.time;

        bool canUseGroundGrace = Time.time - lastGroundedTime <= coyoteTime;
        if (Time.time <= jumpBufferedUntil && canUseGroundGrace)
        {
            PerformJump();
        }

        bool jumpHeld = TouchHoldButton.JumpHeld;
        if (jumpWasHeld && !jumpHeld && body.velocity.y > 0.05f && !jumpCutApplied)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * jumpCutMultiplier);
            jumpCutApplied = true;
        }
        jumpWasHeld = jumpHeld;
    }

    private void FixedUpdate()
    {
        bool movingIntoInput = Mathf.Abs(horizontalInput) > 0.01f;
        float acceleration = grounded
            ? (movingIntoInput ? groundAcceleration : groundDeceleration)
            : airAcceleration;
        float targetSpeed = horizontalInput * maxMoveSpeed;
        float nextX = Mathf.MoveTowards(body.velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        body.velocity = new Vector2(nextX, Mathf.Max(body.velocity.y, -maximumFallSpeed));

        if (body.velocity.y < -0.05f)
            body.gravityScale = baseGravityScale * fallGravityMultiplier;
        else
            body.gravityScale = baseGravityScale;
    }

    private void PerformJump()
    {
        jumpBufferedUntil = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
        grounded = false;
        jumpCutApplied = false;
        body.gravityScale = baseGravityScale;
        body.velocity = new Vector2(body.velocity.x, jumpVelocity);
        // Impulse-like launch uses explicit velocity for consistent feel across device frame rates.
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckGround(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckGround(collision);
    private void OnCollisionExit2D(Collision2D collision) => grounded = false;

    private void CheckGround(Collision2D collision)
    {
        // Ignore floor contacts while travelling upward, preventing a buffered jump from re-grounding.
        if (body.velocity.y > 0.1f) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.55f)
            {
                grounded = true;
                lastGroundedTime = Time.time;
                return;
            }
        }
    }

    private void OnDisable()
    {
        if (body != null) body.gravityScale = baseGravityScale;
    }
}
