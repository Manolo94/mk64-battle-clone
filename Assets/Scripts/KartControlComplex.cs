using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartControlComplex : MonoBehaviour
{

    public Rigidbody KartBody;
    public Transform AccelerationTransform;
    public Transform ForwardWheels;
    public Transform CenterOfMass;

    public string InputName = "keyboard";

    public float AccelerationRate = 1.0f;
    public float BrakeRate = 0.3f;
    public float RotatingRate = 2.0f;
    public float MaxSteeringDegrees = 25.0f;
    public float DriftFriction = 0.5f;
    public float JumpImpulse = 0.5f;
    public float BumpVelocity = 1.0f;
    public float StopVelocity = 0.3f;
    public float SteerFriction = 0.5f;

    private bool onGround = false;
    private bool prevJump = false;

    Vector3 CollisionPosition;
    Vector3 CollisionNormal;

    int counter = 0;

    public Camera GetKartCamera()
    {
        return GetComponentInChildren<Camera>();
    }

    // Use this for initialization
    void Start()
    {
        //KartBody.centerOfMass = CenterOfMass.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // pass the input to the car!
        float h = Input.GetAxis(InputName + " steer");
        float v = Input.GetAxis(InputName + " forward");

        //Debug.Log(v);

        //Debug.Log(h);

        float accelButton = Input.GetButton(InputName + " accel") ? 1 : 0;
        float brakeButton = Input.GetButton(InputName + " brake") ? 1 : 0;
        bool jump = Input.GetButton(InputName + " jump_drift");

        // Check if the kart is on the ground
        BouncyKart bouncyKart = GetComponent<BouncyKart>();
        if (bouncyKart != null) onGround = bouncyKart.GetOnGround();

        // get acceleration, remove y component
        Vector3 accel = Vector3.zero;
        if (accelButton > 0) accel = AccelerationTransform.TransformDirection(Vector3.forward) * AccelerationRate * accelButton;
        // get brake, proportional to speed
        Vector3 brake = AccelerationTransform.TransformDirection(-Vector3.forward) * BrakeRate * Mathf.Abs(brakeButton) * KartBody.velocity.sqrMagnitude;

        // Reverse - pressing the brake and down ( axis < 0 )
        if (v < 0 && brakeButton > 0)
        {
            accel = AccelerationTransform.TransformDirection(-Vector3.forward) * AccelerationRate;
            brake = Vector3.zero;
        }

        // apply the acceleration when accelButton is pressed only on ground
        if (onGround)
        {
            KartBody.drag = 0.5f;

            KartBody.AddForceAtPosition(accel + brake, AccelerationTransform.position);

            // rotate "wheels" based on h
            Quaternion wheelRotation = Quaternion.AngleAxis(MaxSteeringDegrees * h, ForwardWheels.TransformDirection(Vector3.up));
            ForwardWheels.localRotation = wheelRotation;
            Debug.Log(wheelRotation + " " + h);

            // apply friction, front wheels
            Vector3 velocityProjectedOnForwardWheels = Vector3.Project(KartBody.velocity, ForwardWheels.TransformDirection(Vector3.right));
            KartBody.AddForceAtPosition(-velocityProjectedOnForwardWheels, ForwardWheels.position);
            // apply a lot of angular drag when player is not steering
            if (h == 0) KartBody.angularVelocity += (-SteerFriction * Vector3.up * KartBody.angularVelocity.y) * Time.deltaTime * 60;
            Debug.DrawRay(ForwardWheels.position, -velocityProjectedOnForwardWheels); Debug.DrawRay(KartBody.position, KartBody.velocity);

            // apply drift, rear wheels 
            //  get side ways component of velocity
            Vector3 sideWaysVelocity = Vector3.Project(KartBody.velocity, AccelerationTransform.TransformDirection(Vector3.right));
            KartBody.AddForce(-sideWaysVelocity * DriftFriction);

            // Stop the Kart completely if it reaches stop velocity
            if (accel.sqrMagnitude == 0 && KartBody.velocity.sqrMagnitude <= StopVelocity * StopVelocity)
                KartBody.velocity = new Vector3(0f, KartBody.velocity.y, 0f);
        }
        else
        {
            // remove linear drag while in air
            KartBody.drag = 0;
        }

        // jump
        if (!prevJump && jump && onGround) KartBody.AddForce(Vector3.up * JumpImpulse, ForceMode.VelocityChange);

        // rotate in air
        if (!onGround) KartBody.AddForceAtPosition(ForwardWheels.TransformDirection(Vector3.right) * h * RotatingRate * 10, ForwardWheels.position);

        if (CollisionPosition != null && CollisionNormal != null)
            Debug.DrawRay(CollisionPosition, CollisionNormal, Color.yellow);

        prevJump = jump;
    }

    private void OnCollisionEnter(Collision collision)
    {
        return;
        if (collision.contacts[0].normal.z > 0)
            return;

        // reflect velocity
        // r = d - 2(d.n)n  => r - new reflected vector, d - original vector, n - normal of the surface
        Vector3 d = KartBody.velocity;
        d.z = 0;
        Vector3 n = collision.contacts[0].normal;
        Vector3 r = d + Vector3.Dot(d, n) * n * BumpVelocity;

        Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());

        KartBody.AddForce(r, ForceMode.VelocityChange);

        Debug.DrawRay(KartBody.position, KartBody.velocity);

        CollisionPosition = collision.contacts[0].point;
        CollisionNormal = collision.contacts[0].normal;

        Debug.Log("Colliding " + counter);
        counter++;
    }
}