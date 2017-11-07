using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class ObjectNavigationHandler : MonoBehaviour, INavigationHandler {

    void Start()
    {
        // To be able to enable / disable handler in inspector.
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
        ObjectInteraction.Instance.StartNavigatingOrManipulatingObject(gameObject);
    }

    public void OnNavigationUpdated(NavigationEventData eventData)
    {
        ObjectInteraction.Instance.UpdateNavigatingObject(gameObject, eventData);
    }
}
