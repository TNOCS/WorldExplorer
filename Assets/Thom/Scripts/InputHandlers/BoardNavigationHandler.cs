using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class BoardNavigationHandler : MonoBehaviour, INavigationHandler {

    public void OnNavigationCanceled(NavigationEventData eventData)
    {
        BoardInteraction.Instance.StopManipulatingTable();
    }

    public void OnNavigationCompleted(NavigationEventData eventData)
    {
        BoardInteraction.Instance.StopManipulatingTable();
    }

    public void OnNavigationStarted(NavigationEventData eventData)
    {
        BoardInteraction.Instance.StartManipulatingTable(gameObject, gameObject.name);       
    }

    public void OnNavigationUpdated(NavigationEventData eventData)
    {
        if (gameObject.name == "RotateInteractable")
        {
            BoardInteraction.Instance.UpdateTableRotation(eventData);
        }       
        if (gameObject.name == "ResizeInteractable")
        {
            BoardInteraction.Instance.UpdateTableSize(eventData);
        }
    }
}
