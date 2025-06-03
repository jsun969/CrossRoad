using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSounds : MonoBehaviour
{
    private AudioSource source;

    public float minWaitTime = 1;
    public float maxWaitTime = 10;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            source.Play();
        }
    }
}
