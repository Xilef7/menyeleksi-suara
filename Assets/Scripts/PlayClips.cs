using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayClips : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips = null;
    [SerializeField] private float delay = 2.5f;

    private AudioSource audioSource;
    private int i;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        i = 0;
        InvokeRepeating("PlayCurrentClip", 0, delay);
    }

    void PlayCurrentClip()
    {
        audioSource.PlayOneShot(clips[i]);
        if (++i >= clips.Length) {
            CancelInvoke("PlayCurrentClip");
        }
    }
}
