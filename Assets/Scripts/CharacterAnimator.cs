using UnityEngine;

// Procedurally walks/runs a character built from primitives.
// The motion uses separate limb pivots, foot rolls, body bob/lean, and
// arm counter-swing so the character feels more like a small platform hero.
public class CharacterAnimator : MonoBehaviour
{
    public Transform leftLeg, rightLeg, leftArm, rightArm, body;
    public Transform leftFoot, rightFoot, leftHand, rightHand;

    [Tooltip("Limb swing speed relative to movement speed.")]
    public float swingRate = 4.2f;
    [Tooltip("Max swing angle of the limbs, in degrees.")]
    public float swingAngle = 38f;
    [Tooltip("How fast the character turns to face its direction.")]
    public float turnSpeed = 12f;
    [Tooltip("How much the body leans into movement, in degrees.")]
    public float leanAngle = 9f;

    private Vector3 moveDir;
    private float cycle;
    private float bodyBaseY;
    private Quaternion bodyBaseRotation;
    private Vector3 leftLegBasePos, rightLegBasePos, leftArmBasePos, rightArmBasePos;

    void Start()
    {
        if (body != null)
        {
            bodyBaseY = body.localPosition.y;
            bodyBaseRotation = body.localRotation;
        }

        if (leftLeg != null) leftLegBasePos = leftLeg.localPosition;
        if (rightLeg != null) rightLegBasePos = rightLeg.localPosition;
        if (leftArm != null) leftArmBasePos = leftArm.localPosition;
        if (rightArm != null) rightArmBasePos = rightArm.localPosition;
    }

    // Called every physics step by the controller with planar velocity.
    public void SetMovement(Vector3 planarVelocity)
    {
        moveDir = planarVelocity;
    }

    void Update()
    {
        float speed = moveDir.magnitude;
        float runAmount = Mathf.InverseLerp(0.05f, 5.5f, speed);

        // Face the direction of travel.
        if (speed > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
        }

        // Advance the run cycle proportional to speed. The second harmonic
        // gives each step a planted/compression moment instead of a pendulum.
        cycle += Mathf.Lerp(1.2f, speed, runAmount) * swingRate * Time.deltaTime;
        float stride = Mathf.Sin(cycle);
        float lift = Mathf.Max(0f, Mathf.Sin(cycle + Mathf.PI * 0.5f));
        float oppositeLift = Mathf.Max(0f, Mathf.Sin(cycle - Mathf.PI * 0.5f));
        float footRoll = Mathf.Sin(cycle + Mathf.PI * 0.15f);
        float oppositeFootRoll = Mathf.Sin(cycle + Mathf.PI + Mathf.PI * 0.15f);

        SetLimb(leftLeg, leftLegBasePos, stride * swingAngle * runAmount, -lift * 0.05f);
        SetLimb(rightLeg, rightLegBasePos, -stride * swingAngle * runAmount, -oppositeLift * 0.05f);
        SetLimb(leftArm, leftArmBasePos, -stride * swingAngle * 0.75f * runAmount, 0f);
        SetLimb(rightArm, rightArmBasePos, stride * swingAngle * 0.75f * runAmount, 0f);

        SetFoot(leftFoot, -stride * 12f * runAmount + footRoll * 16f * runAmount);
        SetFoot(rightFoot, stride * 12f * runAmount + oppositeFootRoll * 16f * runAmount);
        SetHand(leftHand, stride * 10f * runAmount);
        SetHand(rightHand, -stride * 10f * runAmount);

        if (body != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(cycle)) * 0.07f * runAmount;
            var p = body.localPosition;
            p.y = bodyBaseY + bob;
            body.localPosition = p;

            float lean = Mathf.Lerp(0f, leanAngle, runAmount);
            float sway = Mathf.Sin(cycle) * 3f * runAmount;
            body.localRotation = bodyBaseRotation * Quaternion.Euler(lean, 0f, sway);
        }
    }

    void SetLimb(Transform limb, Vector3 basePos, float xAngle, float yOffset)
    {
        if (limb == null) return;

        limb.localRotation = Quaternion.Euler(xAngle, 0f, 0f);
        limb.localPosition = basePos + new Vector3(0f, yOffset, 0f);
    }

    void SetFoot(Transform foot, float angle)
    {
        if (foot != null) foot.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    void SetHand(Transform hand, float angle)
    {
        if (hand != null) hand.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}
