using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemUI : MonoBehaviour
{
    public TMP_Text itemText;
    public Inventory kartInventory;

    void Start()
    {
        kartInventory.OnItemAdded.AddListener(UpdateInventory);
        kartInventory.OnItemLost.AddListener(ClearInventory);
    }

    void UpdateInventory(Inventory inventory)
    {
        itemText.text = inventory.item.GetItemType().ToString();
    }

    void ClearInventory(Inventory inventory)
    {
        itemText.text = "";
    }
}
