using UnityEngine;
using UnityEngine.InputSystem;

// Moves the player character around using physics forces, and feeds its
// velocity to the CharacterAnimator so the figure runs and faces its
// direction. Attached to the player root (which has a Rigidbody).
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("How hard the character is pushed. Higher = faster.")]
    public float speed = 14f;
    [Tooltip("Upward impulse applied when pressing Space while grounded.")]
    public float jumpForce = 7.5f;
    [Tooltip("How long after leaving ground the player can still jump.")]
    public float coyoteTime = 0.12f;
    [Tooltip("How long a Space press is remembered until physics consumes it.")]
    public float jumpBufferTime = 0.18f;

    private Rigidbody rb;
    private CharacterAnimator anim;
    private float jumpBufferCounter;
    private float groundedCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                      // stay upright; don't roll
        anim = GetComponentInChildren<CharacterAnimator>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;

        if (groundedCounter > 0f)
            groundedCounter -= Time.deltaTime;
    }

    // FixedUpdate is the correct place to apply physics forces.
    void FixedUpdate()
    {
        float h = 0f; // left / right
        float v = 0f; // forward / back

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;
        }

        Vector3 input = new Vector3(h, 0f, v).normalized;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 force = (right * input.x + forward * input.z).normalized * speed;
        rb.AddForce(force);

        if (jumpBufferCounter > 0f && groundedCounter > 0f)
        {
            jumpBufferCounter = 0f;
            groundedCounter = 0f;

            Vector3 velocity = rb.linearVelocity;
            if (velocity.y < 0f)
                velocity.y = 0f;
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Hand the current planar movement to the animator so the character
        // can swing its limbs while the first-person camera owns facing.
        if (anim != null)
            anim.SetMovement(new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z));
    }

    void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.45f)
            {
                groundedCounter = coyoteTime;
                return;
            }
        }
    }
}
