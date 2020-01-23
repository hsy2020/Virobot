using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChangeColor : MonoBehaviour {

    private MeshRenderer m_meshrenderer;
    // Use this for initialization
    void Start () {
        m_meshrenderer = gameObject.GetComponent<MeshRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "BlockCube")
        {
            m_meshrenderer.material.color = Color.red;
        }
    }
    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.name == "BlockCube")
        {
            m_meshrenderer.material.color = Color.white;
        }
    }
}
