using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public float RespawnTime = 5f;
    public GameObject Visual;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var inventory = other.GetComponent<Inventory>();
            if (inventory.item == null)
            {
                inventory.AddRandomItem();

                // Hide the visual and disable collider until the pickup respawns
                if (Visual != null) Visual.SetActive(false);
                GetComponent<Collider>().enabled = false;

                StartCoroutine(RespawnAfter(RespawnTime));
            }
        }
    }

    private IEnumerator RespawnAfter(float timeToRespawn)
    {
        yield return new WaitForSeconds(timeToRespawn);

        // Show the visual and enable collider after the pickup respawn time
        if (Visual != null) Visual.SetActive(true);
        GetComponent<Collider>().enabled = true;
    }
}
