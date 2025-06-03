using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class VisibilityTestedObject : MonoBehaviour
{
    [NonSerialized] public bool isVisible = false;
    [NonSerialized] public uint numViewers = 0;
    
    [Tooltip("Whether to trigger events only on first appearance")]
    public bool firstAppearanceOnly;
    [NonSerialized] public bool hasBeenSeen;
    
    public UnityEvent OnAppear;
    public UnityEvent OnDisappear;
    
    private void LateUpdate()
    {
        if (isVisible)
        {
            if (numViewers == 0)
            {
                isVisible = false;
                hasBeenSeen = true;
                
                if (!(firstAppearanceOnly && hasBeenSeen))
                    OnDisappear.Invoke();
            }
        }
        else if (numViewers > 0)
        {
            isVisible = true;

            if (!(firstAppearanceOnly && hasBeenSeen))
                OnAppear.Invoke();
        }

        numViewers = 0;
    }
}
