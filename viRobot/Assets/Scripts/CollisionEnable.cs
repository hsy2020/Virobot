using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEnable : MonoBehaviour {

    private GameObject drawingtest;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Equals("collisiondetect")) 

        {
            CollisionWarning.txt.SetActive(true);

        }
    }
    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name.Equals("collisiondetect"))
        {
            CollisionWarning.txt.SetActive(false);

        }
    }
}
