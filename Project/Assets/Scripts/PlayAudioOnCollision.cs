using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayAudioOnCollision : MonoBehaviour
{
    private AudioSource _source;

    public float scaleValue = 1;
    // Start is called before the first frame update
    void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        float num = Random.Range(scaleValue - .1f, scaleValue + .1f);
        _source.pitch = num;
        _source.Play();
    }
}
