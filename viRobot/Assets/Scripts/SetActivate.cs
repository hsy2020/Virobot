using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActivate : MonoBehaviour {

    public GameObject myobject;
    public bool activateme;
	
	// Update is called once per frame
	void Update () {
        if (activateme == true)
        {
            myobject.SetActive(false);
        }
        else {
            myobject.SetActive(true);
        }
	}
}
