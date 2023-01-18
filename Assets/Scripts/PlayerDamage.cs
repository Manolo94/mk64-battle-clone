using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartControl))]
[RequireComponent(typeof(BouncyKart))]
public class PlayerDamage : MonoBehaviour
{
    private int numOfLives = 3;

    public int NumOfLives { get => numOfLives; set
        {
            numOfLives = value;

            for (int i = 0; i < LiveVisuals.Length; i++)
                if (i < numOfLives) LiveVisuals[i].SetActive(true);
                else LiveVisuals[i].SetActive(false);
        } }

    public float DeactivationTimeFromShellHit = 3.0f;
    public Vector3 AngularVelocityFromShellHit = Vector3.zero;
    public GameObject[] LiveVisuals;

    // DEBUG
    public bool TestPlayerShotByShell = false;
    public Vector3 TestPlayerShotVelocity = Vector3.zero;

    private KartControl kart;
    private Rigidbody kartBody;
    private BouncyKart kartBouncy;

    private void Start()
    {
        kart = GetComponent<KartControl>();
        kartBody = GetComponent<Rigidbody>();
        kartBouncy = GetComponent<BouncyKart>();
    }

    private void Update()
    {
        if (TestPlayerShotByShell)
        {
            PlayerShotByShell(TestPlayerShotVelocity, transform.position + Random.insideUnitSphere);
        }
        TestPlayerShotByShell = false;
    }

    public void PlayerShotByShell(Vector3 hitVelocity, Vector3 hitPosition)
    {
        NumOfLives--;

        kart.DisableStabilization();
        kart.enabled = false;
        kartBouncy.enabled = false;

        kartBody.velocity += hitVelocity;
        kartBody.angularVelocity = AngularVelocityFromShellHit;
        kartBody.maxAngularVelocity = 1000f;
        kartBody.AddForceAtPosition(hitVelocity*100, hitPosition);

        StartCoroutine(EnableKartAfter(DeactivationTimeFromShellHit));
    }

    IEnumerator EnableKartAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        var newRotation = transform.rotation;
        newRotation.x = 0; newRotation.z = 0;
        transform.rotation = newRotation;
        transform.position = transform.position + Vector3.up;
        kart.enabled = true;
        kart.EnableStabilization();
        kartBouncy.enabled = true;
    }
}
