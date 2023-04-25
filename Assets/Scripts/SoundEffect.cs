using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    public AudioClip[] clips;

    public float volume = 1f;
    public float minPitch = 1f;
    public float maxPitch = 1f;

    public bool playOnAwake;
    // Start is called before the first frame update
    void Start()
    {
        if (playOnAwake)
        {
            PlayRandomSound();
        }
        
    }

    // Play random audioclip from list with random pitch
    public void PlayRandomSound()
    {
        GetComponent<AudioSource>().pitch = Random.Range(minPitch, maxPitch);
        GetComponent<AudioSource>().PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    // Play audioclip index from list with random pitch
    public void PlaySound(int index)
    {
        GetComponent<AudioSource>().pitch = Random.Range(minPitch, maxPitch);
        GetComponent<AudioSource>().PlayOneShot(clips[index]);
    }
}
