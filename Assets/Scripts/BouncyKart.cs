using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyKart : MonoBehaviour {

    public Transform[] BouncePoints;

    public float suspensionStiffness = 1000.0f;
    public float suspensionDistance = 1.0f;
    public float damping = 0.2f;

    public Rigidbody KartBody;

    private float[] prevCompressionRate;
    private bool[] touching;

    private bool onGround;

	// Use this for initialization
	void Start () {
        // Setup prevCompressionRates
        prevCompressionRate = new float[BouncePoints.Length];
        touching = new bool[BouncePoints.Length];
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        // Create the bounce, do a raycast checking 
        //  for the floor for every BouncePoint
        if (BouncePoints != null && KartBody != null)
        {
            onGround = false;

            for (int i = 0; i < BouncePoints.Length; i++)
            {
                RaycastHit ray;

                //if(Physics.Raycast(BouncePoints[i].position, BouncePoints[i].TransformDirection(Vector3.forward), out ray, suspensionDistance))
                if (Physics.Raycast(BouncePoints[i].position, Vector3.down, out ray, suspensionDistance))
                {
                    // Get the compression rate, the higher the rate, the more compressed
                    float compressionRate = 1.0f - ray.distance / suspensionDistance;

                    //Debug.Log("Hit " + i + " compression rate: " + compressionRate);
                    //Debug.DrawRay(BouncePoints[i].position, BouncePoints[i].TransformDirection(Vector3.forward) * ray.distance, Color.green);
                    Debug.DrawRay(BouncePoints[i].position, Vector3.down * ray.distance, Color.green);

                    // Calculate force
                    Vector3 springForce = BouncePoints[i].TransformDirection(Vector3.back) * compressionRate * suspensionStiffness;
                    // Add a damping force based on how fast the compressionRate is changing
                    Vector3 dampingForce = BouncePoints[i].TransformDirection(Vector3.forward) * (prevCompressionRate[i] - compressionRate) * damping * suspensionStiffness;
                    Vector3 resultingForce = springForce + dampingForce;

                    // Apply the corresponding force based on the compression rate
                    //  force's direction is opposite to the bounce point's forward direction
                    KartBody.AddForceAtPosition(resultingForce, BouncePoints[i].position);

                    Debug.DrawRay(BouncePoints[i].position, resultingForce/2, Color.blue);

                    // Save previousCompressionRate
                    prevCompressionRate[i] = compressionRate;

                    touching[i] = true;
                    onGround = true;
                }
                else
                {
                    Debug.DrawRay(BouncePoints[i].position, BouncePoints[i].TransformDirection(Vector3.forward) * suspensionDistance, Color.red);
                    touching[i] = false;
                }
            }

            // Reset prevCompressionRate when not on the ground,
            //  to work around an error where the kart becomes tilted
            if(!onGround)
                prevCompressionRate = new float[BouncePoints.Length];
        }
	}

    public bool GetOnGround()
    {
        return onGround;
    }
}
