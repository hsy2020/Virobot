using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObjectParent : MonoBehaviour {
    public GameObject ShapeIndexedFaceSet_003;
    public GameObject penball;
	// Use this for initialization
	void Start () {
        penball.transform.SetParent(ShapeIndexedFaceSet_003.transform);
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
