using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptEnabler : MonoBehaviour {
    
    void Update()
    {
        var currentMode = UIManager.Instance.currentMode;
        var objectList = InventoryObjectInteraction.Instance.spawnedObjectsList;

        if (currentMode == "RotateBtn")
        {
            foreach (SpawnedObject spawnedObject in objectList)
            {
                spawnedObject.obj.GetComponent<ObjectNavigationHandler>().enabled = false;
                spawnedObject.obj.GetComponent<ObjectManipulationHandler>().enabled = true;
            }
            
        }
        if (currentMode == "ScaleBtn")
        {
            foreach (SpawnedObject spawnedObject in objectList)
            {
                spawnedObject.obj.GetComponent<ObjectNavigationHandler>().enabled = true;
                spawnedObject.obj.GetComponent<ObjectManipulationHandler>().enabled = false;
            }
        }
    }
}
