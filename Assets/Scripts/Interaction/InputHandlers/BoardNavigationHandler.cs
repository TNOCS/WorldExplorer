using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class BoardNavigationHandler : MonoBehaviour, INavigationHandler, IInputHandler
{
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
        BoardInteraction.Instance.StopManipulatingTable(gameObject);
        gameObject.GetComponent<Renderer>().material = BoardInteraction.Instance.boundingBoxInitial;
    }

    public void OnNavigationCompleted(NavigationEventData eventData)
    {
        BoardInteraction.Instance.StopManipulatingTable(gameObject);
        gameObject.GetComponent<Renderer>().material = BoardInteraction.Instance.boundingBoxInitial;
    }

    public void OnNavigationStarted(NavigationEventData eventData)
    {
        audioSource.PlayOneShot(clickFeedback, 0.1f);
        BoardInteraction.Instance.StartManipulatingTable(gameObject, gameObject.name);
    }

    public void OnInputDown(InputEventData eventData)
    {
        gameObject.GetComponent<Renderer>().material = BoardInteraction.Instance.boundingBoxSelected;
    }

    public void OnNavigationUpdated(NavigationEventData eventData)
    {
        if (gameObject.name == "RotateInteractable")
        {
            BoardInteraction.Instance.UpdateTableRotation(eventData);
        }
        if (gameObject.name == "ResizeInteractableLeft")
        {
            BoardInteraction.Instance.UpdateTableSize(eventData, -1);
        }

        if (gameObject.name == "ResizeInteractableRight" || gameObject.name == "ResizeInteractable")
        {
            BoardInteraction.Instance.UpdateTableSize(eventData, 1);
        }
    }

    public void OnInputUp(InputEventData eventData)
    {
        gameObject.GetComponent<Renderer>().material = BoardInteraction.Instance.boundingBoxInitial;
    }
}
