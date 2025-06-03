using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnter : MonoBehaviour
{
    public bool userCollisionOnly = true;

    public UnityEvent onEnterEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (userCollisionOnly && other.transform.parent)
        {
            //if (other.transform.parent.GetComponent<ActionBasedControllerManager>())
            //{
            //    onEnterEvent.Invoke();
            //}
        }
        else if(!userCollisionOnly)
        {
            onEnterEvent.Invoke(); 
        }
    }
}
