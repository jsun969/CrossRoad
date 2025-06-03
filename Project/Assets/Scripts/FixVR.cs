using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
public class FixVR : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(StartVR());
    }
    
    private IEnumerator StartVR()
    {
        yield return 0;
        if( XRGeneralSettings.Instance.Manager.activeLoader != null ){
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }
}

