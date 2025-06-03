using System;
using UnityEngine;
using UnityEngine.Events;

public class SelfDespawner : MonoBehaviour
{
    public UnityEvent OnDespawn;

    private bool quitting = false;
    
    private void OnDestroy()
    {
        if (quitting)
            return;
        
        OnDespawn.Invoke();
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    public void DoDespawn()
    {
        Destroy(gameObject);
    }
}
