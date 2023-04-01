using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleGreenShell : ItemManager.Item
{
    public GameObject GreenShellProjectile;

    private GameObject backShell;

    public override void ActivatePressed(Inventory activatorInventory, float forwardAxis)
    {
        if (activatorInventory.BackwardShoot != null)
        {
            backShell = Instantiate(GreenShellProjectile, activatorInventory.BackwardShoot);
            backShell.transform.localPosition = Vector3.zero;
            backShell.SetActive(true);
            backShell.GetComponent<Rigidbody>().isKinematic = true;

            var greenShell = backShell.GetComponent<GreenShell>();
            greenShell.enabled = false;
            // If green shell was destroyed while in the back, destroy the item
            greenShell.OnProjectileDestroyed.AddListener((g) =>
            {
                activatorInventory.OnActivateReleased.RemoveListener(ActivateReleased);
                Destroy(gameObject);
            });
        }

        // Make the item react to activate release
        activatorInventory.OnActivateReleased.AddListener(ActivateReleased);

        // Stop the item from reacting to activate press
        activatorInventory.OnActivatePressed.RemoveListener(ActivatePressed);
    }

    public override void ActivateReleased(Inventory activatorInventory, float forwardAxis)
    {
        Debug.Log("SingleGreenShell: Released");

        if (backShell != null)
        {
            backShell.SetActive(false);
            Destroy(backShell);
        }

        Transform shoot = activatorInventory.ForwardShoot;
        if (forwardAxis > 0.3f) shoot = activatorInventory.BackwardShoot; // up is negative

        if (shoot != null)
        {
            GameObject gShell = Instantiate(GreenShellProjectile);
            gShell.SetActive(true);
            gShell.transform.position = shoot.position;
            gShell.transform.rotation = shoot.rotation;
        }

        activatorInventory.OnActivateReleased.RemoveListener(ActivateReleased);

        Destroy(gameObject);
    }

    public override ItemManager.ItemType GetItemType()
    {
        return ItemManager.ItemType.SINGLE_GREEN_SHELL;
    }
}
