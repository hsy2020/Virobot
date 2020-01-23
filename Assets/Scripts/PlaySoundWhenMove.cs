using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundWhenMove : MonoBehaviour {
    AudioSource collisiondetectAudioSource;
    public AudioClip robotarm;
    public Rigidbody collisiondetect;

     void Start()
    {
        collisiondetectAudioSource = GetComponent<AudioSource>();
        collisiondetectAudioSource.enabled = true;
        collisiondetect = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (collisiondetect.velocity.magnitude >= 0.1 && collisiondetectAudioSource.isPlaying == true)
        {
            collisiondetectAudioSource.PlayOneShot(robotarm,0.7F);
        }
    }
}
