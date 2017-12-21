using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ObjectNavigationHandler : MonoBehaviour, INavigationHandler {

    private AudioClip clickFeedback;
    private AudioSource audioSource;

    void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[1];
        clickFeedback = Resources.Load<AudioClip>("Audio/highclick");
    }

    public void OnNavigationCanceled(NavigationEventData eventData)
    {
        ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }

    public void OnNavigationCompleted(NavigationEventData eventData)
    {
        ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }

    public void OnNavigationStarted(NavigationEventData eventData)
    {
        Debug.Log("startnav");
        audioSource.PlayOneShot(clickFeedback, 0.1f);
        ObjectInteraction.Instance.StartNavigatingOrManipulatingObject(gameObject);
    }

    public void OnNavigationUpdated(NavigationEventData eventData)
    {
        Debug.Log("updatenav");
        ObjectInteraction.Instance.UpdateNavigatingObject(gameObject, eventData);
    }
}
