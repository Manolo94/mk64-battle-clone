using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour {

    public Transform carlookAt;
    public Transform camTransform;
    public Transform camDesiredPosition;

    public Rigidbody carRigidBody;

    public float positionLagInSeconds = 0;

	// Use this for initialization
	void Start () {
        camTransform = transform;
	}

    public void Update()
    {

    }

    private void LateUpdate()
    {
        // lerp the camera position
        Vector3 newPosition = camDesiredPosition.position;
        newPosition.y = carRigidBody.position.y + 1.5f;
        camTransform.position = newPosition;

        // Get the desired distance
        float distance = Vector3.Distance(carRigidBody.position, camTransform.position);

        // make the camera look at the car
        camTransform.LookAt(carlookAt.position);

        // prevent the camera from clipping
        RaycastHit ray;
        Debug.DrawRay(carRigidBody.position, (camTransform.position - carRigidBody.position), Color.green);
        if (Physics.Raycast(carRigidBody.position, (camTransform.position - carRigidBody.position).normalized, out ray, distance))
        {
            camTransform.position = ray.point + ray.normal*0.1f;
        }
    }
}
