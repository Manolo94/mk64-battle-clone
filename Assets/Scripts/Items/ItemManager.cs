using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public enum ItemType
    {
        SINGLE_GREEN_SHELL, 
        MULTI_GREEN_SHELL, 
        RED_SHELL, 
        STAR, 
        SINGLE_BANANA, 
        MULTI_BANANA,
        FAKE_ITEM
    }

    public abstract class Item : MonoBehaviour
    {
        public abstract void ActivatePressed(Inventory activatorInventory);
        public abstract void ActivateReleased(Inventory activatorInventory, float forwardAxis);
    }

    public static Dictionary<ItemType, GameObject> ItemPrefabs;

    [Serializable]
    struct ItemPrefabMapping
    {
        public ItemType itemType;
        public GameObject prefab;
    }

    [SerializeField]
    private ItemPrefabMapping[] itemPrefabs;

    private void Start()
    {
        ItemPrefabs = new Dictionary<ItemType, GameObject>();

        foreach (var p in itemPrefabs)
            ItemPrefabs.Add(p.itemType, p.prefab);
    }
}
