using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour {

    [SerializeField] float m_lower = 0, m_wait = 2;

    Vector3 m_origPos;
    bool m_isMoving = false;
    Rigidbody m_body;

	void Start () {
        m_origPos = transform.position;
        m_body = GetComponent<Rigidbody>();
	}
	
	void Update () {
        if (!m_isMoving && transform.position.y < m_lower) {
            m_isMoving = true;
            StartCoroutine(Move());
        }
	}

    IEnumerator Move() {
        yield return new WaitForSeconds(m_wait);
        m_body.velocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.position = m_origPos;
        m_isMoving = false;
    }
}
