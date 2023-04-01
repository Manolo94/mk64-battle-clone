using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGreenShell : ItemManager.Item
{
    public GameObject GreenShellProjectile;
    public Transform[] GreenShellPositions;

    private GameObject[] GreenShells;

    public override void ActivatePressed(Inventory activatorInventory, float forwardAxis)
    {
        GreenShells = new GameObject[GreenShellPositions.Length];

        // Show the shells
        transform.parent = activatorInventory.CenterPosition;
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);

        // Next activation shoots the shell
        activatorInventory.OnActivatePressed.AddListener(ThrowShell);

        for (int i = 0; i < GreenShellPositions.Length; i++)
        {
            if (GreenShellPositions[i] != null)
            {
                GreenShells[i] = Instantiate(GreenShellProjectile, GreenShellPositions[i]);
                GreenShells[i].transform.localPosition = Vector3.zero;
                GreenShells[i].SetActive(true);
                GreenShells[i].GetComponent<Rigidbody>().isKinematic = true;

                var greenShell = GreenShells[i].GetComponent<GreenShell>();
                greenShell.enabled = false;
                greenShell.OnProjectileDestroyed.AddListener(DestroyIfAllShellsAreUsed);
            }
        }
    }

    private void ThrowShell(Inventory activatorInventory, float forwardAxis)
    {
        Debug.Log("MultiGreenShell: Threw Shell");

        int positionInGreenShellArray = GetNextShellToUse();

        // Remove green shell from the array
        GreenShells[positionInGreenShellArray].SetActive(false);
        GreenShells[positionInGreenShellArray].GetComponent<GreenShell>()
            .OnProjectileDestroyed.RemoveListener(DestroyIfAllShellsAreUsed);
        Destroy(GreenShells[positionInGreenShellArray]);
        GreenShells[positionInGreenShellArray] = null;

        // Shoot the shell
        Transform shoot = activatorInventory.ForwardShoot;
        if (forwardAxis > 0.3f) shoot = activatorInventory.BackwardShoot; // up is negative

        if (shoot != null)
        {
            GameObject gShell = Instantiate(GreenShellProjectile);
            gShell.SetActive(true);
            gShell.transform.position = shoot.position;
            gShell.transform.rotation = shoot.rotation;
        }

        // If there's no shells left, stop throwing on activation
        if(GetNextShellToUse() == -1) activatorInventory.OnActivatePressed.RemoveListener(ThrowShell);

        DestroyIfAllShellsAreUsed(null);
    }

    private void DestroyIfAllShellsAreUsed(GameObject projectileToBeDestroyed)
    {
        // check if there's any shells left ignore projectileToBeDestroyed
        foreach (var s in GreenShells)
            if (s != null && s != projectileToBeDestroyed) return;

        Destroy(gameObject);
    }

    private int GetNextShellToUse()
    {
        for (int i = 0; i < GreenShells.Length; i++)
            if (GreenShells[i] != null) return i;
        return -1;
    }

    public override ItemManager.ItemType GetItemType()
    {
        return ItemManager.ItemType.MULTI_GREEN_SHELL;
    }

    public override void ActivateReleased(Inventory activatorInventory, float forwardAxis)
    {
        
    }
}
