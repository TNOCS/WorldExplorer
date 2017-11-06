using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class ObjectManipulationHandler : MonoBehaviour, IManipulationHandler {

    void Start()
    {
        // To be able to enable / disable handler in inspector.
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        ObjectInteraction.Instance.StartNavigatingOrManipulatingObject(gameObject);
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        ObjectInteraction.Instance.UpdateManipulatingObject(gameObject, eventData);
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        ObjectInteraction.Instance.StopNavigatingOrManipulatingObject();
    }
}
