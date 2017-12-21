using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ObjectManipulationHandler : MonoBehaviour, IManipulationHandler {

    private AudioClip clickFeedback;
    private AudioSource audioSource;

    void Start()
    { 
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[1];
        clickFeedback = Resources.Load<AudioClip>("Audio/highclick");
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
    //   audioSource.PlayOneShot(clickFeedback, 0.1f);
    //   ObjectInteraction.Instance.StartNavigatingOrManipulatingObject(gameObject);
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        //ObjectInteraction.Instance.UpdateManipulatingObject(gameObject, eventData);
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
       // ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
       // ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }
}
