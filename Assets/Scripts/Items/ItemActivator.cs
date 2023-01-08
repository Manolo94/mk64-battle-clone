using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartControl))]
public class ItemActivator : MonoBehaviour
{
    public Transform ForwardShoot;
    public Transform BackwardShoot;

    private KartControl kartControl;

    private bool prevActivate = false;

    // TODO: Implement item system
    public GameObject greenShellPrefab;

    // Start is called before the first frame update
    void Start()
    {
        kartControl = GetComponent<KartControl>();
    }

    // Update is called once per frame
    void Update()
    {
        bool activateItem = Input.GetButton(kartControl.InputName + " activate");

        if(activateItem && !prevActivate)
        {
            // TODO: Implement item system
            if(greenShellPrefab != null && ForwardShoot != null)
            {
                GameObject nItem = Instantiate(greenShellPrefab);
                nItem.transform.position = ForwardShoot.position;
                nItem.transform.rotation = ForwardShoot.rotation;
            }
        }

        prevActivate = activateItem;
    }
}
