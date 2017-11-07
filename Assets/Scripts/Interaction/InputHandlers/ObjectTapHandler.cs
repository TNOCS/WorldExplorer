using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class ObjectTapHandler : MonoBehaviour, IInputClickHandler {
    
    private AudioClip clickFeedback;
    private AudioSource audioSource;

    void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[1];
        clickFeedback = Resources.Load<AudioClip>("Audio/highclick");
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        ObjectInteraction.Instance.Tap(gameObject);
        audioSource.PlayOneShot(clickFeedback, 0.1f);
    }

}
