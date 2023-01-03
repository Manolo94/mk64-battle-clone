using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour {

    public float distance = 40;

    private float rotation = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float yposition = 20 + Mathf.Cos(rotation * 3.4f) * 5;
        this.transform.position = new Vector3(distance*Mathf.Cos(rotation),yposition,distance*Mathf.Sin(rotation));
        rotation += 0.2f*Time.deltaTime;

        this.transform.LookAt(new Vector3(0, 0, 0));
	}
}
