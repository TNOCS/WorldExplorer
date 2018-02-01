using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class InventoryObjectInteraction : SingletonCustom<InventoryObjectInteraction>
{
    /// <summary>
    /// Handles the interaction with inventory objects, including spawning them.
    /// </summary>
    
    private GameObject newlySpawned;
    public GameObject copyObject;

    [SerializeField]
    public List<SpawnedObject> spawnedObjectsList = new List<SpawnedObject>();

    public void Start()
    {
        newlySpawned = GameObject.Find("NewlySpawned");
    }

    public void Tap(GameObject go)
    {
        Spawn(go.name);
    }

    // Factors for scaling upon zoom.
    public Dictionary<int, float> spawnScaleFactors = new Dictionary<int, float>
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
            goName = "Pin";
        }
        var zoom = AppState.Instance.Config.ActiveView.Zoom;
        copyObject = Instantiate(Resources.Load("Prefabs/Inventory/" + goName)) as GameObject;
        copyObject.transform.localScale = copyObject.transform.localScale * spawnScaleFactors[zoom] * BoardInteraction.Instance.terrain.transform.localScale.x;
        copyObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        var col = copyObject.GetComponent<BoxCollider>();
        col.isTrigger = true;

        // An unique objectname is needed for MQTT references.
        copyObject.name = copyObject.name + "-" + (Time.deltaTime * 1000).ToString();

        // Ignore Raycast layer.
        copyObject.layer = 2;

        InventoryObjectTapHandler handler = copyObject.GetComponent<InventoryObjectTapHandler>();
        Destroy(handler);

        copyObject.gameObject.transform.parent = newlySpawned.transform;
        copyObject.gameObject.tag = "newspawnobject";
        ObjectInteraction.Instance.StartMoving(copyObject);
    }
}
