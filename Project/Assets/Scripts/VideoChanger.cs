using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoChanger : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    
    public VideoClip[] clips;

    public int clipNo = 0;

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    public void ChangeVideo()
    {
        clipNo++;

        if (clipNo > clips.Length - 1)
        {
            clipNo = 0;
        }
        
        videoPlayer.clip = clips[clipNo];
    }
}
