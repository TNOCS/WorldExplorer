using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using HoloToolkit.Unity.InputModule;

public class BoardInteraction : SingletonCustom<BoardInteraction>
{
    /// <summary>
    /// Handles any interaction with the table/board, including zooming, panning, adjusting the table, toggling terrain heights and opening/switching map locations.
    /// </summary>    

    // The scaling difference per zoom level.
    public float scaleFactor = 2f;
    public int minZoomLevel = 16;
    public int maxZoomLevel = 19;

    public float dragSensitivity = 0.1f;
    public float rotationSensitivity = 0.5f;
    public float tiltSensitivity = 3f;
    public float scaleSpeed = 0.0050f;
    public float minimumScale = 0.2f;
    public float maximumScale = 2.5f;
    public float maxTileRange = 4;
    public float minTileRange = 1;

    private bool tableIsTilted = false;
    public bool tableIsRotating = false;

    public Vector3 originalTablePosition;
    public Vector3 originalTableScale;
    public Quaternion originalTableRotation;

    public GameObject terrain;
    public bool terrainHeights = true;
    public bool terrainLeveled = false;

    public Material boundingBoxInitial;
    public Material boundingBoxSelected;

    public Shader twoSidedShader;

    void Start()
    {
        Invoke("SetObjects", 3);
    }

    private void SetObjects()
    {
        terrain = GameObject.Find("terrain");
        originalTablePosition = terrain.transform.position;
        originalTableRotation = terrain.transform.rotation;
        originalTableScale = terrain.transform.localScale;

        boundingBoxInitial = Resources.Load("Textures/BoundingBoxInitial", typeof(Material)) as Material;
        boundingBoxSelected = Resources.Load("Textures/BoundingBoxSelected", typeof(Material)) as Material;
    }

    public void Tap(Vector2d tappedPosition)
    {
        if (ObjectInteraction.Instance.objectInFocus != null)
        {
            ObjectInteraction.Instance.Place();
        }
        if (UIManager.Instance.currentMode == "ZoomInBtn")
        {
            Zoom(1, tappedPosition);
        }
        if (UIManager.Instance.currentMode == "ZoomOutBtn")
        {
            Zoom(0, tappedPosition);
        }
        if (UIManager.Instance.currentMode == "CenterBtn")
        {
            CenterMap(tappedPosition);
        }
    }

    public void CenterMap(Vector2d tappedPosition)
    {
        if (tableIsTilted)
        {
            ResetTableTilt();
        }

        var view = AppState.Instance.Config.ActiveView;
        var latLonv2 = Extensions.ToVector2(tappedPosition);
        view.SetView(latLonv2.x, latLonv2.y, view.Zoom, view.Range);

        SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
    }

    public void Zoom(int zoomDirection, Vector2d tappedPosition)
    {
        if (tableIsTilted)
        {
            ResetTableTilt();
        }

        var view = AppState.Instance.Config.ActiveView;
        Vector2 latLonv2 = MapzenGo.Helpers.Extensions.ToVector2(tappedPosition);

        // ZoomDirection 0 = out 1 = in
        if (zoomDirection == 0 && view.Zoom > minZoomLevel)
        {
            view.Zoom -= 1;
            view.SetView(latLonv2.x, latLonv2.y, view.Zoom, view.Range);

            AppState.Instance.ResetMap(view);
            SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView, "out");
        }

        if (zoomDirection == 1 && view.Zoom < maxZoomLevel)
        {
            view.Zoom += 1;
            view.SetView(latLonv2.x, latLonv2.y, view.Zoom, view.Range);

            AppState.Instance.ResetMap(view);
            SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView, "in");
        }
    }

    public void ToggleTerrainHeights()
    {
        if (AppState.Instance.Config.ActiveView.TerrainHeightsAvailable == true)
        {
            terrainHeights = !terrainHeights;
            AppState.Instance.ResetMap();

            if (terrainHeights)
            {
                GameObject.Find("ToggleTerrainBtn").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("terrainicon");
                var UI = GameObject.Find("UIMain");
                UI.transform.position = new Vector3(UI.transform.position.x, UI.transform.position.y - 0.05f, UI.transform.position.z);
                terrainLeveled = true;
            }
            else
            {
                GameObject.Find("ToggleTerrainBtn").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("terrainiconflat");
                var UI = GameObject.Find("UIMain");
                UI.transform.position = new Vector3(UI.transform.position.x, UI.transform.position.y + 0.05f, UI.transform.position.z);
                terrainLeveled = false;

            }
        }
    }

    public void SwitchMapOverlay(int view, string name, int zoomLevel)
    {
        AppState.Instance.Config.ActiveView = AppState.Instance.Config.Views[view].Clone();

        // Compound has no .pngs at level 19.
        if (AppState.Instance.Config.ActiveView.Name == "Compound")
        {
            maxZoomLevel = 18;
        }
        else
        {
            maxZoomLevel = 19;
        }

        terrainHeights = false;
       
        AppState.Instance.ResetMap();

        if (terrainLeveled)
        {
            var UI = GameObject.Find("UIMain");
            UI.transform.position = new Vector3(UI.transform.position.x, UI.transform.position.y + 0.05f, UI.transform.position.z);
            terrainLeveled = false;
        }

        if (GameObject.Find("ToggleTerrainBtn") != null)
        {
            GameObject.Find("ToggleTerrainBtn").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("terrainiconflat");
        }

        UIManager.Instance.CurrentOverlayText.text = AppState.Instance.Config.ActiveView.Name.ToString();
        SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
    }

    public void ResetTable()
    {
        terrain.transform.localScale = originalTableScale;
        terrain.transform.rotation = originalTableRotation;
        tableIsTilted = false;

        SessionManager.Instance.UpdateTable();
    }

    public void ResetTableTilt()
    {
        terrain.transform.rotation = originalTableRotation;
        SessionManager.Instance.UpdateTable();
    }

    public void StartManipulatingTable(GameObject go, string manipulationMode)
    {
        if (terrain == null)
        {
            terrain = GameObject.Find("terrain");
        }

        if (manipulationMode == "RotateInteractable")
        {
            tableIsRotating = true;
        }

        InputManager.Instance.OverrideFocusedObject = go;
        UIInteraction.Instance.MapPanel.SetActive(false);
        UIInteraction.Instance.InventoryPanel.SetActive(false);
        UIInteraction.Instance.InventoryTextMesh.text = "Open \nInventory";
    }

    public void StopManipulatingTable(GameObject go)
    {
        go.GetComponent<Renderer>().material = boundingBoxInitial;
        InputManager.Instance.OverrideFocusedObject = null;
        tableIsRotating = false;
        SessionManager.Instance.UpdateTable();
    }

    public void UpdateTableRotation(NavigationEventData eventData)
    {
        var rotationFactor = eventData.CumulativeDelta.x * rotationSensitivity;
        terrain.transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
        SessionManager.Instance.UpdateTable();
    }

    //public void UpdateTableTilt(ManipulationEventData eventData)
    //{
    //    var tableOrientation = UIManager.Instance.currentUIPosition;
    //    tableIsTilted = true;
    //    var tiltFactorX = eventData.CumulativeDelta.x * tiltSensitivity;
    //    var tiltFactorY = eventData.CumulativeDelta.y * tiltSensitivity;
    //
    //    switch (tableOrientation)
    //    {
    //        case "Front":
    //            terrain.transform.Rotate(new Vector3(tiltFactorY, 0, -1 * tiltFactorX));
    //            break;
    //        case "Left":
    //            terrain.transform.Rotate(new Vector3(-1 * tiltFactorX, 0, -1 * tiltFactorY));
    //            break;
    //        case "Right":
    //            terrain.transform.Rotate(new Vector3(tiltFactorX, 0, tiltFactorY));
    //            break;
    //        case "Back":
    //            terrain.transform.Rotate(new Vector3(-1 * tiltFactorY, 0, tiltFactorX));
    //            break;
    //        default:
    //            break;
    //    }
    //
    //     // Save new rotation
    //     UIManager.Instance.SetOriginalRotation();
    //     SessionManager.Instance.UpdateTable();
    // }

    public void UpdateTableSize(NavigationEventData eventData, int direction)
    {
        // Size up.
        if (eventData.CumulativeDelta.x >= 0)
        {
            if (terrain.transform.localScale.x < maximumScale && terrain.transform.localScale.y < maximumScale && terrain.transform.localScale.z < maximumScale)
            {
                terrain.transform.localScale += new Vector3((terrain.transform.localScale.x * scaleSpeed * direction), (terrain.transform.localScale.y * scaleSpeed * direction), (terrain.transform.localScale.z * scaleSpeed * direction));
            }
        }

        // Size down.
        if (eventData.CumulativeDelta.x <= 0)
        {
            if (terrain.transform.localScale.x > minimumScale && terrain.transform.localScale.y > minimumScale && terrain.transform.localScale.z > minimumScale)
            {
                terrain.transform.localScale -= new Vector3((terrain.transform.localScale.x * scaleSpeed * direction), (terrain.transform.localScale.y * scaleSpeed * direction), (terrain.transform.localScale.z * scaleSpeed * direction));
            }
        }

        SessionManager.Instance.UpdateTable();
    }

    //  public void IncreaseTiles()
    //  {
    //      var av = AppState.Instance.Config.ActiveView;
    //      var currentRange = av.Range;
    //      var newRange = av.Range + 1;
    //
    //      if (newRange <= maxTileRange)
    //      {
    //          AppState.Instance.Config.ActiveView.TileSize = 150;
    //          AppState.Instance.Config.ActiveView.Range = newRange;
    //          //UIInteraction.Instance.SetTileRangeButtons(AppState.Instance.Config.ActiveView.Range);
    //          //AppState.Instance.ResetMap();
    //          SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
    //      }
    //  }
    //
    //  public void DecreaseTiles()
    //  {
    //      var av = AppState.Instance.Config.ActiveView;
    //      var currentRange = av.Range;
    //      var newRange = av.Range - 1;
    //      if (newRange >= minTileRange)
    //      {
    //          if (newRange == 1)
    //          {
    //              AppState.Instance.Config.ActiveView.TileSize = 125;
    //          }
    //          AppState.Instance.Config.ActiveView.Range = newRange;
    //          //UIInteraction.Instance.SetTileRangeButtons(newRange);
    //          //AppState.Instance.ResetMap();
    //          SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
    //      }
    //  }

    public void Go(string direction)
    {
        var view = AppState.Instance.Config.ActiveView;
        var metersPerTile = view.Resolution;
        var merc = GM.LatLonToMeters(new Vector2d(view.Lon, view.Lat));
        Vector2d delta;

        switch (direction.ToLowerInvariant())
        {
            case "north":
                delta = new Vector2d(metersPerTile, 0);
                break;
            case "south":
                delta = new Vector2d(-metersPerTile, 0);
                break;
            case "east":
                delta = new Vector2d(0, metersPerTile);
                break;
            case "west":
                delta = new Vector2d(0, -metersPerTile);
                break;
            case "northeast":
                delta = new Vector2d(metersPerTile, metersPerTile);
                break;
            case "southeast":
                delta = new Vector2d(-metersPerTile, metersPerTile);
                break;
            case "northwest":
                delta = new Vector2d(metersPerTile, -metersPerTile);
                break;
            case "southwest":
                delta = new Vector2d(-metersPerTile, -metersPerTile);
                break;
            default:
                delta = new Vector2d();
                break;
        }
        merc += delta;
        var ll = GM.MetersToLatLon(merc);
        view.Lat = (float)ll.x;
        view.Lon = (float)ll.y;
        AppState.Instance.ResetMap(view);
        SessionManager.Instance.UpdateView(view);
    }
}
