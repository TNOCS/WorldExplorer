using Assets.Scripts;
using HoloToolkit.Unity.InputModule;
using MapzenGo.Helpers;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Plugins;
using UnityEngine;
using Assets.Scripts.Classes;

public class ObjectInteraction : Singleton<ObjectInteraction> {

    // Object being manipulated.
    public GameObject objectInFocus;
   
    [SerializeField]
    private bool moving = false;

    // Rotation and scaling
    public GameObject popUpHolder;
    public float RotationSensitivity = 5.0f;
    private float rotationFactor;
    private float minimumScale = 0.001f;
    private float maximumScale = 2f;
    private float scaleSpeed = 0.01f;
    private Vector3 drawPoint1, drawPoint2;
    private GameObject lrObject;

    private void Awake()
    {
        popUpHolder = GameObject.Find("PopUpHolder");
        popUpHolder.SetActive(false);
    }

    private void Start()
    {
        // Demo Ship
        var ship = GameObject.Find("Ship");
        var latShip = 52.9591529941637;
        var lonShip = 4.78603529997701;
        var latLonv2 = new Vector2d(latShip, lonShip);
        var metersShip = GM.LatLonToMeters(latLonv2);


        SpawnedObject spawnedObject = new SpawnedObject(ship, ship.transform.TransformDirection(ship.transform.position), latShip, lonShip, metersShip);
        Debug.Log("ADding" + spawnedObject.obj + " to list");
        InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
        ship.tag = "spawnobject";
    }

    private void Update()
    {
        if (moving)
        {
            objectInFocus.transform.position = new Vector3(CursorManagerCustom.Instance.Cursor.transform.position.x, CursorManagerCustom.Instance.Cursor.transform.position.y, CursorManagerCustom.Instance.Cursor.transform.position.z);
        }    
    }

    public void Tap(GameObject go)
    {
        switch (UIManager.Instance.currentMode)
        {
            case "MoveBtn":                
                StartMoving(go);
                break;
            case "DeleteBtn":
                Delete(go);
                break;
            case "CopyBtn":
                Copy(go);
                break;
            default:
                break;
        }
    }

    public void Place()
    {
        if (objectInFocus.tag == "spawnobject" || objectInFocus.tag == "newspawnobject")
        {
            GameObject tile;
            RaycastHit hit;
            int layerMask =  (1 << 7);
            layerMask |= Physics.IgnoreRaycastLayer;
            layerMask = ~layerMask;
            
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, layerMask))
            {
                tile = hit.collider.gameObject;
                Debug.Log(tile);
                if (tile.name.StartsWith("tilelayer"))
                {   

                    var cursorLatLon = SessionManager.Instance.me.CursorLocationToVector2d();
                    var cursorMeter = GM.LatLonToMeters(cursorLatLon);

                    moving = false;

                    // If object already exists, only update position.
                    if (objectInFocus.tag == "spawnobject")
                    {
                        foreach (SpawnedObject so in InventoryObjectInteraction.Instance.spawnedObjectsList)
                        {
                            if (so.obj == objectInFocus)
                            {
                                so.lat = cursorLatLon.x;
                                so.lon = cursorLatLon.y;
                            }
                        }
                    }

                    // If it's a new object, add it to the list.
                    if (objectInFocus.tag == "newspawnobject")
                    {
                        SpawnedObject spawnedObject = new SpawnedObject(objectInFocus, objectInFocus.transform.TransformDirection(objectInFocus.transform.position), /*objectScale,*/ cursorLatLon.x, cursorLatLon.y, cursorMeter);
                        InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
                        objectInFocus.tag = "spawnobject";
                    }

                    objectInFocus.layer = 0;
                    objectInFocus = null;
                    UIManager.Instance.currentMode = "MoveBtn";
                    CursorManagerCustom.Instance.SetCursorIcon(UIManager.Instance.currentMode);
                }
                else
                {
                    Debug.Log("Collider is no tile");
                }
            }            
        }        
    }

    public void StartMoving(GameObject go)
    {
        UIManager.Instance.currentMode = "Placing";        
        objectInFocus = go;
        objectInFocus.layer = 2;
        moving = true;
        go.layer = 2;
    }

    public void StartNavigatingOrManipulatingObject(GameObject go)
    {
        InputManager.Instance.OverrideFocusedObject = go;
        popUpHolder.SetActive(true);
        popUpHolder.transform.position = go.transform.position;
        drawPoint1 = popUpHolder.transform.position;
    }

    // Rotating
    public void UpdateManipulatingObject(GameObject go, ManipulationEventData eventData)
    {
        if (UIManager.Instance.currentMode == "RotateBtn")
        {
            drawPoint2 = new Vector3(go.transform.position.x + eventData.CumulativeDelta.x, drawPoint1.y, drawPoint1.z + eventData.CumulativeDelta.z);
            go.transform.LookAt(drawPoint2);
        }
    }

    // Scaling
    public void UpdateNavigatingObject(GameObject go, NavigationEventData eventData)
    {        
        if (UIManager.Instance.currentMode == "ScaleBtn")
        {
            // Scales object and draws a line on a rails as visual feedback.
            if (eventData.CumulativeDelta.x >= 0)
            {
                if (go.transform.localScale.x < maximumScale && go.transform.localScale.y < maximumScale && go.transform.localScale.z < maximumScale)
                {
                    go.transform.localScale += new Vector3((go.transform.localScale.x * scaleSpeed), (go.transform.localScale.y * scaleSpeed), (go.transform.localScale.z * scaleSpeed));
                }
            }

            if (eventData.CumulativeDelta.x <= 0)
            {
                if (go.transform.localScale.x > minimumScale && go.transform.localScale.y > minimumScale && go.transform.localScale.z > minimumScale)
                {
                    go.transform.localScale -= new Vector3((go.transform.localScale.x * scaleSpeed), (go.transform.localScale.y * scaleSpeed), (go.transform.localScale.z * scaleSpeed));
                }
                    
            }
        }
    }

    public void StopNavigatingOrManipulatingObject()
    {
        InputManager.Instance.OverrideFocusedObject = null;
        popUpHolder.SetActive(false);
//        Destroy(lrObject);
    }

    private void Delete(GameObject go)
    {
        foreach (SpawnedObject spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
        {
            if (spawnedObject.obj == go)
            {
                InventoryObjectInteraction.Instance.spawnedObjectsList.Remove(spawnedObject);
            }
        }

        Destroy(go);            
    }

    private void Copy(GameObject go)
    {
        GameObject copyObject;
        copyObject = Instantiate(go, go.transform.position, go.transform.rotation) as GameObject;
        copyObject.transform.parent = go.transform.parent;
        copyObject.transform.localScale = go.transform.localScale;
        StartMoving(copyObject);
        copyObject = null;
    }
    /*
    public void DrawLine(Vector3 drawPoint1, Vector3 drawPoint2)
    {
        lrObject = new GameObject();
        LineRenderer lr = lrObject.AddComponent<LineRenderer>();
        var color1 = Color.white;
        var color2 = new Color(1, 1, 1, 0);
        lr.material = new Material(Shader.Find("Custom/AlwaysOnTop"));
        lr.startColor = color1;
        lr.endColor = color2;

        lr.startWidth = 0.015f;
        lr.endWidth = 0.015f;
        lr.SetPosition(0, drawPoint1);
        lr.SetPosition(1, drawPoint2);
    }*/
}
