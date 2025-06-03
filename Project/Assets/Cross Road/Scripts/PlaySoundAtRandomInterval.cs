using UnityEngine;

public class PlaySoundAtRandomInterval : MonoBehaviour
{
    public AudioSource audioSource;
    public float minInterval = 1f;
    public float maxInterval = 5f;

    private float timer;
    private float currentRandomInterval;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("No audio source");
            enabled = false;
            return;
        }

        if (maxInterval < minInterval)
        {
            Debug.LogWarning("Max Interval should be larger than Min. Setting Max to Min now.");
            maxInterval = minInterval;
        }

        SetNextRandomInterval();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            audioSource.Play();
            SetNextRandomInterval();
        } 
    }

    private void SetNextRandomInterval()
    {
        currentRandomInterval = Random.Range(minInterval, maxInterval);
        timer = currentRandomInterval;
    }
}
