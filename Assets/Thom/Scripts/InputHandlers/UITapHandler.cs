using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class UITapHandler : MonoBehaviour, IInputClickHandler {

    private AudioClip clickFeedback;
    private AudioSource audioSource;

    private void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[1];
        clickFeedback = Resources.Load<AudioClip>("Audio/lowclick");
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {       
        UIManager.Instance.Tap(gameObject);
        audioSource.PlayOneShot(clickFeedback, 1f);
    }
}
