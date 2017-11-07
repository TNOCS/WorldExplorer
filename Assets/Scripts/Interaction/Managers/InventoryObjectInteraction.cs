using Assets.Scripts;
using MapzenGo.Helpers;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryObjectInteraction : Singleton<InventoryObjectInteraction> {

    private GameObject newlySpawned;
    public GameObject copyObject;

    [SerializeField]
    public List<SpawnedObject> spawnedObjectsList = new List<SpawnedObject>();

    private void Awake()
    {
        newlySpawned = GameObject.Find("NewlySpawned");
    }

    public void Tap(GameObject go)
    {
        Spawn(go.name);
    }

    // Factors for scaling upon zoom.
    private Dictionary<int, float> spawnScaleFactors = new Dictionary<int, float>
    {
        {15, 0.5f},
        {16, 0.5f},
        {17, 1f},
        {18, 2f},
        {19, 3f},
        {20, 4f}
    };

    public void Spawn(string goName)
    {
        UIManager.Instance.SetSpriteColor(GameObject.Find("MoveBtn"));
        copyObject = null;
        Destroy(ObjectInteraction.Instance.objectInFocus);

        if (goName == "MarkMapBtn")
        {
            copyObject = Instantiate(Resources.Load("Pin")) as GameObject;
            copyObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            copyObject.AddComponent<BoxCollider>();
            var col = copyObject.GetComponent<BoxCollider>();
            col.center = new Vector3(0, 50, 0);
            col.size = new Vector3(50, 100, 50);
            col.isTrigger = true;

            copyObject.AddComponent<ObjectTapHandler>();
            copyObject.AddComponent<ObjectNavigationHandler>();
            copyObject.AddComponent<ObjectManipulationHandler>();
            copyObject.AddComponent<ScriptEnabler>();
        }
        else
        {
            var zoom = AppState.Instance.Config.ActiveView.Zoom;
            //copyObject = Instantiate(go, go.transform.position, go.transform.rotation) as GameObject;
            copyObject = Instantiate(Resources.Load("Prefabs/" + goName)) as GameObject;
            //copyObject.transform.localScale = go.transform.localScale * spawnScaleFactors[zoom];
            copyObject.transform.localScale = copyObject.transform.localScale * spawnScaleFactors[zoom];
            copyObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            var col = copyObject.GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        // Ignore Raycast layer.
        copyObject.layer = 2;

        InventoryObjectTapHandler handler = copyObject.GetComponent<InventoryObjectTapHandler>();
        Destroy(handler);

        copyObject.gameObject.transform.parent = newlySpawned.transform;
        copyObject.gameObject.tag = "newspawnobject";
        ObjectInteraction.Instance.StartMoving(copyObject);
    }
}
