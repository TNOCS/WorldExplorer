using Assets.Scripts;
using HoloToolkit.Unity.InputModule;
using MapzenGo.Helpers;
using Assets.Scripts.Plugins;
using UnityEngine;

public class ObjectInteraction : SingletonCustom<ObjectInteraction>
{
    /// <summary>
    /// Handles any interaction with an interactable object, including moving, rotating, deleting and showing info.
    /// </summary>
    
    // The object being manipulated.
    public GameObject objectInFocus;

    // If an object is moving.
    [SerializeField]
    private bool moving = false;

    // Rotation and scaling
    public float RotationSensitivity = 5.0f;
    private float rotationFactor;
    private float minimumScale = 0.001f;
    private float maximumScale = 2f;
    private float scaleSpeed = 0.02f;
    private Vector3 drawPoint1, drawPoint2;
    private float rotationSensitivity = 1.5f;

    // Info label
    public GameObject infoLabel;
    public GameObject tooltipObject = null;

    private void Update()
    {
        if (moving && objectInFocus != null)
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
            case "InfoBtn":
                ShowInfo(go);
                break;
            case "ZoomInBtn":
                BoardInteraction.Instance.Zoom(1, SessionManager.Instance.me.CursorLocationToVector2d());
                break;
            case "ZoomOutBtn":
                BoardInteraction.Instance.Zoom(0, SessionManager.Instance.me.CursorLocationToVector2d());
                break;
            case "CenterBtn":
                BoardInteraction.Instance.CenterMap(SessionManager.Instance.me.CursorLocationToVector2d());
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
            int layerMask = (1 << 7);
            layerMask |= Physics.IgnoreRaycastLayer;
            layerMask = ~layerMask;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, layerMask))
            {
                tile = hit.collider.gameObject;
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
                               // HKL SessionManager.Instance.UpdateExistingObject(so);
                            }
                        }
                    }

                    // If it's a new object, add it to the list.
                    if (objectInFocus.tag == "newspawnobject")
                    {
                        SpawnedObject spawnedObject = new SpawnedObject(objectInFocus, objectInFocus.transform.TransformDirection(objectInFocus.transform.position), /*objectScale,*/ cursorLatLon.x, cursorLatLon.y, objectInFocus.transform.localScale, objectInFocus.transform.rotation);
                        InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
                        objectInFocus.tag = "spawnobject";

                        // Let other users know there is either a new or editted object.
                        // HKLSessionManager.Instance.UpdateNewObject(spawnedObject);
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
        if (!go.name.Contains("Buildings"))
        {
            UIManager.Instance.currentMode = "Placing";
            objectInFocus = go;
            objectInFocus.layer = 2;
            moving = true;
            go.layer = 2;
        }
    }

    public void StartNavigatingOrManipulatingObject(GameObject go)
    {
        InputManager.Instance.OverrideFocusedObject = go;
    }

    // Rotating
    public void UpdateNavigatingObject(GameObject go, NavigationEventData eventData)
    {
        if (UIManager.Instance.currentMode == "RotateBtn")
        {
            var rotationFactor = eventData.NormalizedOffset.x * rotationSensitivity;
            go.transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
            UpdateObjectToOtherUsers(go);
        }
    }

    // Scaling
    public void _UpdateNavigatingObject(GameObject go, NavigationEventData eventData)
    {
        if (UIManager.Instance.currentMode == "ScaleBtn" && go.GetComponent<PrefabObjectData>().scaleable)
        {
            if (eventData.NormalizedOffset.x >= 0)
            {
                if (go.transform.localScale.x < maximumScale && go.transform.localScale.y < maximumScale && go.transform.localScale.z < maximumScale)
                {
                    go.transform.localScale += new Vector3((go.transform.localScale.x * scaleSpeed), (go.transform.localScale.y * scaleSpeed), (go.transform.localScale.z * scaleSpeed));
                }
            }

            if (eventData.NormalizedOffset.x <= 0)
            {
                if (go.transform.localScale.x > minimumScale && go.transform.localScale.y > minimumScale && go.transform.localScale.z > minimumScale)
                {
                    go.transform.localScale -= new Vector3((go.transform.localScale.x * scaleSpeed), (go.transform.localScale.y * scaleSpeed), (go.transform.localScale.z * scaleSpeed));
                }
            }

            UpdateObjectToOtherUsers(go);
        }
    }

    public void StopNavigatingOrManipulatingObject()
    {
        InputManager.Instance.OverrideFocusedObject = null;
    }

    public void Delete(GameObject go)
    {
        foreach (SpawnedObject spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
        {
            if (spawnedObject.obj == go)
            {
                InventoryObjectInteraction.Instance.spawnedObjectsList.Remove(spawnedObject);
                // HKL SessionManager.Instance.UpdateDeletedObject(spawnedObject);
                Destroy(go);
            }
        }
    }

    private void Copy(GameObject go)
    {
        GameObject copyObject;
        copyObject = Instantiate(go, go.transform.position, go.transform.rotation) as GameObject;

        // Unique objectname is needed for MQTT references.
        var name = copyObject.name;
        int index = name.IndexOf("-");
        if (index > 0)
            name = name.Substring(0, index);

        copyObject.name = name + "-" + (Time.deltaTime * 1000).ToString();

        copyObject.transform.parent = go.transform.parent;
        copyObject.transform.localScale = go.transform.localScale;
        copyObject.tag = "newspawnobject";
        StartMoving(copyObject);
        copyObject = null;
    }

    public void ShowInfo(GameObject go)
    {
        if (infoLabel == null)
        {
            infoLabel = GameObject.Find("InfoLabel");
        }

        if (tooltipObject != null)
        {
            if (go == tooltipObject)
            {
                infoLabel.SetActive(false);
                tooltipObject = null;
            }
            else
            {
                SetLabel(go);
            }
        }
        else
        {
            SetLabel(go);
        }

        UIInteraction.Instance.MapPanel.SetActive(false);
    }

    public void SetLabel(GameObject go)
    {
        infoLabel.SetActive(true);
        tooltipObject = go;

        // Gets the purpose of a building, based on material name (commercial, industrial, etc).
        var buildingPurpose = go.GetComponent<MeshRenderer>().material.name;
        int index = buildingPurpose.IndexOf(" ");
        if (index > 0)
        {
            buildingPurpose = buildingPurpose.Substring(0, index);
        }

        var xCoordinates = SessionManager.Instance.me.CursorLocationToVector2d().x.ToString();
        xCoordinates = (xCoordinates).Remove(xCoordinates.Length - 7);
        var yCoordinates = SessionManager.Instance.me.CursorLocationToVector2d().y.ToString();
        yCoordinates = (yCoordinates).Remove(yCoordinates.Length - 7);

        infoLabel.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<TextMesh>().text = go.name;
        infoLabel.transform.GetChild(0).gameObject.transform.GetChild(2).GetComponent<TextMesh>().text =
            "The " + go.name + " are located in the '" + AppState.Instance.Config.ActiveView.Name.ToString() + "' area. \nThe coordinates are (" + xCoordinates + ", " + yCoordinates + "). \nThe purpose of this building is " + buildingPurpose + ". \n\nTap the building again to close this message.";
        infoLabel.transform.position = new Vector3(go.transform.position.x, go.transform.position.y + 0.4f, go.transform.position.z);
        infoLabel.transform.LookAt(Camera.main.transform);
    }

    public void CloseLabel()
    {
        if (infoLabel == null)
        {
            infoLabel = GameObject.Find("InfoLabel");
        }

        if (infoLabel.activeInHierarchy)
        {
            infoLabel.SetActive(false);
        }
    }

    public void UpdateObjectToOtherUsers(GameObject go)
    {
        // Find SpawnedObject instance of GameObject and uses it as a reference to update the data to other users.
        foreach (SpawnedObject spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
        {
            if (spawnedObject.obj == go)
            {
               // HKL SessionManager.Instance.UpdateExistingObject(spawnedObject);
            }
        }
    }
}
