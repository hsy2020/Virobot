using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowObject : MonoBehaviour {

    public Transform ObjectTransform;

    private Vector3 _cameraOffset;

 

    [Range(0.01f,1.0f)]
    public float SmoothFactor = 0.5f;//able can smoothly follow the object

    public bool LookAtFront = false;

    // Use this for initialization
    void Start () {
        _cameraOffset = transform.position - ObjectTransform.position;

		
	}
	
	// Update is called after update methods
	void LateUpdate () {
        Vector3 newPos = ObjectTransform.position + _cameraOffset;

        transform.position = Vector3.Slerp(transform.position, newPos, SmoothFactor);

        if (LookAtFront)
            transform.LookAt(ObjectTransform);
	}
}
