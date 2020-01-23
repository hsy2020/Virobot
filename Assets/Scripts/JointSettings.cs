using BioIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointSettings : MonoBehaviour {

    [SerializeField] float m_acceleration = 50, m_velocity = 50;

	void Start () {
        var joints = GetComponentsInChildren<KinematicJoint>();
        foreach(var joint in joints) {
            joint.XMotion.SetMaximumAcceleration(m_acceleration);
            joint.YMotion.SetMaximumAcceleration(m_acceleration);
            joint.ZMotion.SetMaximumAcceleration(m_acceleration);
            joint.XMotion.SetMaximumVelocity(m_velocity);
            joint.YMotion.SetMaximumVelocity(m_velocity);
            joint.ZMotion.SetMaximumVelocity(m_velocity);
        }
	}
	
}
