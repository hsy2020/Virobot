using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleRend : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
