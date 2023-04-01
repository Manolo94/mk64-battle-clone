using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KartControl))]
public class Inventory : MonoBehaviour
{
    public Transform ForwardShoot;
    public Transform BackwardShoot;
    public Transform CenterPosition;

    public UnityEvent<Inventory, float> OnActivatePressed;
    public UnityEvent<Inventory, float> OnActivateReleased;
    public UnityEvent<Inventory> OnItemAdded;
    public UnityEvent<Inventory> OnItemLost;

    private KartControl kartControl;

    private bool prevActivate = false;

    public ItemManager.Item item = null;
    public ItemManager.Item deployedItem = null;

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

        if (activateItem && !prevActivate)
        {
            if (OnActivatePressed != null) OnActivatePressed.Invoke(this, forward);
            if (item != null && deployedItem == null)
            {
                item.ActivatePressed(this, forward);
                deployedItem = item;
                item = null;

                if(OnItemLost != null) OnItemLost.Invoke(this);
            }
        }

        if (!activateItem && prevActivate)
        {
            if (OnActivateReleased != null) OnActivateReleased.Invoke(this, forward);
        }

        prevActivate = activateItem;

        // For debug
        if(Debug_AddItem) 
        {
            AddItem(Debug_ItemToAdd);
        }
        Debug_AddItem = false;
    }

    public void AddRandomItem()
    {
        var keys = ItemManager.ItemPrefabs.Keys;
        var itemIndex = Random.Range(0, keys.Count);

        AddItem(keys.ToList()[itemIndex]);
    }

    public void AddItem(ItemManager.ItemType itemToAdd)
    {
        GameObject newItem = ItemManager.ItemPrefabs[itemToAdd];
        if (newItem != null)
        {
            item = Instantiate(newItem.GetComponent<ItemManager.Item>(), transform);
            // Items will be deactivated (not shown) when sitting in inventory
            item.gameObject.SetActive(false);

            if (OnItemAdded != null) OnItemAdded.Invoke(this);
        }
    }
}
