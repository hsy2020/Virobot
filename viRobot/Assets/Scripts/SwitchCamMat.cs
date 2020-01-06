using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamMat : MonoBehaviour {
    public Material cam1;
    public Material cam2;
    public Material cam3;

    public void switchmat(int x) {
        
        if (x == 1)
        {
            GetComponent<Renderer>().material = cam1;
        }
        else if (x == 2)
        {
            GetComponent<Renderer>().material = cam2;
        }
        else {
            GetComponent<Renderer>().material = cam3;
        }


    }
}
