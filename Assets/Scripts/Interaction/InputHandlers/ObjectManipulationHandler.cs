using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ObjectManipulationHandler : MonoBehaviour
{
    private AudioClip clickFeedback;
    private AudioSource audioSource;

    void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[1];
        clickFeedback = Resources.Load<AudioClip>("Audio/highclick");
    }
}
