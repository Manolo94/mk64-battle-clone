using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KartControl : MonoBehaviour
{

    public Rigidbody KartBody;
    public Transform AccelerationTransform;
    public Transform ForwardWheels;
    public Transform CenterOfMass;

    public string InputName = "keyboard";

    public float AccelerationRate = 1.0f;
    public float WheelBrakeCoefficient = 0.9f;
    public float WheelDragCoefficient = 0.5f;
    public float RotatingRate = 2.0f;
    public float KartRotatingExtraRate = 2.0f;
    public float JumpingKartRotatingExtraRate = 4.0f;
    public float MaxKartRotationExtra = 100f;
    public float KartRotatingExtraDrag = 0.5f;
    public float BumpCoefficient = 0.3f;
    public float BumpDecayRate = 0.3f;
    public float DriftDecayRate = 0.3f;
    public float DriftRegenRate = 0.5f;
    /// <summary>
    /// Wheel Speed at which the kart starts actually moving
    /// </summary>
    public float WheelSpeedSleepThreshold = 1.0f;
    public float JumpImpulse = 0.5f;
    [Range(0.01f,1f)]
    public float WheelSpeedReductionFromBumps = 0.2f;

    private bool onGround = false;
    private bool prevJump = false;
    private Vector3 jumpingDirection = Vector3.zero;
    private bool prevOnGround = false;
    /// <summary>
    /// Direction to which the drift should be applied, in local space
    /// </summary>
    private Vector3 driftDirection = Vector3.zero;
    private float driftAmount = 0f;

    private float initialLinearDrag = 0.0f;
    private float wheelSpeed = 0.0f;
    private float kartExtraRotation = 0.0f;

    Vector3 CollisionPosition;
    Vector3 CollisionNormal;
    private Vector3 bumpVelocity = Vector3.zero;

    int counter = 0;

    public Camera GetKartCamera()
    {
        return GetComponentInChildren<Camera>();
    }

    // Use this for initialization
    void Start()
    {
        KartBody.centerOfMass = KartBody.transform.InverseTransformPoint(CenterOfMass.position);
        initialLinearDrag = KartBody.drag;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // pass the input to the car!
        float h = Input.GetAxis(InputName + " steer");
        float v = Input.GetAxis(InputName + " forward");

        //Debug.Log(v + ", " + h);

        float accelButton = Input.GetButton(InputName + " accel") ? 1 : 0;
        float brakeButton = Input.GetButton(InputName + " brake") ? 1 : 0;
        bool jump = Input.GetButton(InputName + " jump_drift");

        // Check if the kart is on the ground
        BouncyKart bouncyKart = GetComponent<BouncyKart>();
        if (bouncyKart != null) onGround = bouncyKart.GetOnGround();

        // apply the acceleration when accelButton is pressed only on ground
        if (onGround)
        {
            // Apply acceleration
            if (accelButton > 0) wheelSpeed += AccelerationRate * Time.deltaTime;

            // Brake button logic
            bool braking = false;
            if (brakeButton > 0)
            {
                // If down and brake button are pressed, reverse (down is positive)
                if (v > 0) wheelSpeed -= AccelerationRate * Time.deltaTime;
                else braking = true;
            }

            // Apply coefficient when brake is applied, drag coefficient otherwise
            float totalWheelFriction = braking ? WheelBrakeCoefficient : WheelDragCoefficient;

            // Apply wheel drag
            wheelSpeed -= wheelSpeed * totalWheelFriction * Time.deltaTime;
        }

        // Only apply velocity when it's over the wheel speed threshold, else, set it to 0;
        Vector3 newVelocity = Vector3.zero;
        if (Mathf.Abs(wheelSpeed) >= WheelSpeedSleepThreshold)
        {
            var dir = AccelerationTransform.forward;
            // drift mode
            if (jump)
            {
                dir = (driftDirection * driftAmount + AccelerationTransform.forward).normalized;
                driftDirection -= driftDirection * DriftDecayRate * Time.deltaTime;
                driftAmount -= driftAmount * DriftDecayRate * Time.deltaTime;
            }

            newVelocity = wheelSpeed * dir;
        }

        newVelocity.y = KartBody.velocity.y;
        KartBody.velocity = newVelocity;

        // Apply rotation, only rotate while moving or in the air
        if (Mathf.Abs(wheelSpeed) >= WheelSpeedSleepThreshold || !onGround)
        {
            Vector3 newRot = KartBody.rotation.eulerAngles;
            var oldRot = newRot;

            if (h > 0) newRot += Vector3.up * RotatingRate * Time.deltaTime;
            if (h < 0) newRot -= Vector3.up * RotatingRate * Time.deltaTime;

            // Increase rotating rate while drifting
            var kartRotating = KartRotatingExtraRate;
            if (jump) kartRotating += KartRotatingExtraRate * driftAmount;
            if (!onGround)
            {
                if (h > 0 && kartExtraRotation < 0) kartExtraRotation = 0;
                if (h < 0 && kartExtraRotation > 0) kartExtraRotation = 0;
                kartRotating = JumpingKartRotatingExtraRate;
            }

            if (driftAmount <= 0)
            {
                // Rotate AccelerationTransform independently
                var newRotAccel = AccelerationTransform.localRotation.eulerAngles;

                if (h > 0 && kartExtraRotation <= MaxKartRotationExtra)
                {
                    kartExtraRotation += kartRotating * Time.deltaTime;
                    // Make the switch from left to middle super quick, but not when drifting
                    if (kartExtraRotation < 0) kartExtraRotation -= kartExtraRotation * KartRotatingExtraDrag * Time.deltaTime;
                }
                else if (h < 0 && kartExtraRotation >= -MaxKartRotationExtra)
                {
                    kartExtraRotation -= kartRotating * Time.deltaTime;
                    // Make the switch from right to middle super quick, but not when drifting
                    if (kartExtraRotation > 0) kartExtraRotation -= kartExtraRotation * KartRotatingExtraDrag * Time.deltaTime;
                }
                else kartExtraRotation -= kartExtraRotation * KartRotatingExtraDrag * Time.deltaTime;

                newRotAccel.y = kartExtraRotation;

                AccelerationTransform.localRotation = Quaternion.Euler(newRotAccel);
            }
            // Drift mode - press the opposite to maintain drift amount, overdoing it can overdrift
            // TODO: FIX DRIFTING, it's a bit buggy
            else
            {
                // Rotate AccelerationTransform independently
                var newRotAccel = AccelerationTransform.localRotation.eulerAngles;

                if (kartExtraRotation > 1e-2)
                {
                    if(h > 0.2) kartExtraRotation += kartRotating * Time.deltaTime;
                    if (h < -0.2)
                    {
                        driftAmount += DriftRegenRate * Time.deltaTime;
                        // null out camera rotation
                        newRot = oldRot;
                    }
                }
                else if (kartExtraRotation < -1e-2)
                {
                    if(h < -0.2) kartExtraRotation -= kartRotating * Time.deltaTime;
                    if (h > 0.2)
                    {
                        driftAmount += DriftRegenRate * Time.deltaTime;
                        // null out camera rotation
                        newRot = oldRot;
                    }
                }
                else kartExtraRotation -= kartExtraRotation * KartRotatingExtraDrag * Time.deltaTime;

                newRotAccel.y = kartExtraRotation;

                AccelerationTransform.localRotation = Quaternion.Euler(newRotAccel);

                if (driftAmount > 1.5) driftAmount = 0;
            }

            KartBody.rotation = Quaternion.Euler(newRot);
        }

        // Bump Velocity, velocity added as a result of bumping into something
        KartBody.velocity += bumpVelocity;
        bumpVelocity -= BumpDecayRate * bumpVelocity * Time.deltaTime; // Bump velocity disappears after the bump

        // jump
        if (!prevJump && jump && onGround)
        {
            KartBody.AddForce(Vector3.up * JumpImpulse, ForceMode.VelocityChange);

            // Setup the values in case of a drift
            jumpingDirection = AccelerationTransform.forward;
            driftDirection = Vector3.zero;
            driftAmount = 1;
        }

        // Reset drift amount when jump is released
        if (!jump) driftAmount = 0;

        //Debug.Log("DRIFT " + driftAmount);

        // lock drift direction when hitting the ground
        if (jump && !prevOnGround && onGround)
        {
            // only the portion of the jumpingDirection vector perpendicular to forward direction
            //  using absolute value because the resulting vector should always point in the same direction as jumpingDirection
            driftDirection = Mathf.Abs(Vector3.Dot(AccelerationTransform.right, jumpingDirection)) * jumpingDirection;
        }

        if (CollisionPosition != null && CollisionNormal != null)
            Debug.DrawRay(CollisionPosition, bumpVelocity, Color.yellow);

        prevJump = jump;
        prevOnGround = onGround;

        // DEBUG
        Debug.DrawRay(KartBody.position, KartBody.velocity);
        Debug.DrawRay(KartBody.position, bumpVelocity, Color.cyan);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // reflect velocity
        // r = d - 2(d.n)n  => r - new reflected vector, d - original vector, n - normal of the surface
        Vector3 d = KartBody.velocity;
        Vector3 n = collision.contacts[0].normal;

        n.y = 0; d.y = 0; // Remove vertical component, to avoid flying in the air when going up ramps

        // Negate the dot product, since the resulting velocity should negate the incoming one
        Vector3 r = n * BumpCoefficient * -Vector3.Dot(d, n);

        bumpVelocity = r;
        //Debug.Log(bumpVelocity.magnitude);

        Debug.DrawRay(collision.contacts[0].point, bumpVelocity, Color.black, 10.0f);

        CollisionPosition = collision.contacts[0].point;
        CollisionNormal = collision.contacts[0].normal;

        counter++;
    }

    public void DisableStabilization()
    {
        KartBody.ResetCenterOfMass();
    }

    public void EnableStabilization()
    {
        KartBody.centerOfMass = KartBody.transform.InverseTransformPoint(CenterOfMass.position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0)
            return;

        wheelSpeed = (1-WheelSpeedReductionFromBumps) * wheelSpeed;
    }
}

