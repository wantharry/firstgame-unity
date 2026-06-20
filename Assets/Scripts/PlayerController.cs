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

    private Rigidbody rb;
    private CharacterAnimator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                      // stay upright; don't roll
        anim = GetComponentInChildren<CharacterAnimator>();
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

        Vector3 force = new Vector3(h, 0f, v).normalized * speed;
        rb.AddForce(force);

        // Hand the current planar movement to the animator so the character
        // can face its direction and swing its limbs.
        if (anim != null)
            anim.SetMovement(new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z));
    }
}
