using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ObjectTapHandler : MonoBehaviour, IInputClickHandler
{
    /// <summary>
    /// Handles the users taps on objects.
    /// </summary>
    /// 
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
        audioSource.PlayOneShot(clickFeedback, 0.1f);
        ObjectInteraction.Instance.Tap(gameObject);
    }
}
