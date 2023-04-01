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
            backShell.GetComponent<GreenShell>().enabled = false;
            Debug.Log(backShell.name);
        }

        // Make the item react to activates
        activatorInventory.OnActivateReleased.AddListener(ActivateReleased);
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

        activatorInventory.OnActivatePressed.RemoveListener(ActivatePressed);
        activatorInventory.OnActivateReleased.RemoveListener(ActivateReleased);

        Destroy(gameObject);
    }

    public override ItemManager.ItemType GetItemType()
    {
        return ItemManager.ItemType.SINGLE_GREEN_SHELL;
    }
}
