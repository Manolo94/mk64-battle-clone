using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartControl))]
public class Inventory : MonoBehaviour
{
    public Transform ForwardShoot;
    public Transform BackwardShoot;

    private KartControl kartControl;

    private bool prevActivate = false;

    public ItemManager.Item item;

    // Debug
    public ItemManager.ItemType Debug_ItemToAdd;
    public bool Debug_AddItem;

    // Start is called before the first frame update
    void Start()
    {
        kartControl = GetComponent<KartControl>();
    }

    // Update is called once per frame
    void Update()
    {
        bool activateItem = Input.GetButton(kartControl.InputName + " activate");
        float forward = Input.GetAxis(kartControl.InputName + " forward");

        if(item != null)
        {
            if (activateItem && !prevActivate)
            {
                item.ActivatePressed(this);
            }

            if (!activateItem && prevActivate)
            {
                item.ActivateReleased(this, forward);

                Destroy(item.gameObject);
            }
        }

        prevActivate = activateItem;

        // TODO: Implement pick-up system
        if(Debug_AddItem) 
        {
            GameObject newItem = ItemManager.ItemPrefabs[Debug_ItemToAdd];
            if(newItem != null)
            {
                item = Instantiate(newItem.GetComponent<ItemManager.Item>(), transform);
                // Items will be deactivated (not shown) when sitting in inventory
                item.gameObject.SetActive(false);
            }
        }
        Debug_AddItem = false;
    }
}
