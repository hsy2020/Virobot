using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionWarning : MonoBehaviour {

    public static GameObject txt;
 

	// Use this for initialization
	void Start () {
        txt = GameObject.Find("CollisionWarning");
        txt.SetActive(false);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
