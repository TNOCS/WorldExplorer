using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class UITapHandler : MonoBehaviour, IInputClickHandler
{
    /// <summary>
    /// Handles the users taps on the UI.
    /// </summary>
    
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
