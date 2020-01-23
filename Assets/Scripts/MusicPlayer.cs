using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// script used to randomly swith current scene's bgm from 3 audios.
public class MusicPlayer : MonoBehaviour {
    public AudioClip[] clips;
    private AudioSource audiosource; 

	// Use this for initialization
	void Start () {
        audiosource = FindObjectOfType<AudioSource>();
        audiosource.loop = false;
	}
    private AudioClip GetRandomClip()
    {
        return clips[Random.Range(0,clips.Length)];
    }
	
	// Update is called once per frame
	void Update () {
		if (!audiosource.isPlaying)
        {
            audiosource.clip = GetRandomClip();
            audiosource.Play();
        }
	}
}
