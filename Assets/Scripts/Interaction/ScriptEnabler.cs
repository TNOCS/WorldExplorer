using UnityEngine;

public class ScriptEnabler : MonoBehaviour
{
    /// <summary>
    /// Switches between the two scripts: one for handlign navigation input and one for manipulation input, based on the current mode.,
    /// This is done as the HoloLens is unable to detect both at the same time.
    /// 
    /// Currently unused. Enable if you want to enable scaling
    /// </summary>

    void Update()
    {
        var currentMode = UIManager.Instance.currentMode;
        var objectList = InventoryObjectInteraction.Instance.spawnedObjectsList;

        if (currentMode == "RotateBtn")
        {
            foreach (SpawnedObject spawnedObject in objectList)
            {
                //  spawnedObject.obj.GetComponent<ObjectNavigationHandler>().enabled = false;
                //  spawnedObject.obj.GetComponent<ObjectManipulationHandler>().enabled = true;
            }

        }
        if (currentMode == "ScaleBtn")
        {
            foreach (SpawnedObject spawnedObject in objectList)
            {
                if (spawnedObject.obj.GetComponent<ObjectNavigationHandler>() != null)
                {
                    spawnedObject.obj.GetComponent<ObjectNavigationHandler>().enabled = true;
                }
                if (spawnedObject.obj.GetComponent<ObjectManipulationHandler>() != null)
                {
                    spawnedObject.obj.GetComponent<ObjectManipulationHandler>().enabled = false;
                }
            }
        }
    }
}
