using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using MapzenGo.Models;
using HoloToolkit.Unity.InputModule;
using System;

public class BoardInteraction : Singleton<BoardInteraction>{

    //The scaling difference per zoom level.
    public float scaleFactor = 1.5f;
    public int minZoomLevel = 17;
    public int maxZoomLevel = 19;

    public float dragSensitivity = 0.1f;
    public float rotationSensitivity = 0.5f;
    public float tiltSensitivity = 3f;
    public float scaleSpeed = 0.0005f;
    public float minimumScale = 0.2f; // TODO: find correct values
    public float maximumScale = 2.5f; // TODO: find correct values
    public float maxTileRange = 4;
    public float minTileRange = 1;

    private bool tableIsTilted = false;

    public bool tableIsRotating = false;

    public Vector3 originalTablePosition;
    public Vector3 originalTableScale;
    public Quaternion originalTableRotation;

    [SerializeField]
    private GameObject terrain;

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
    }

    public void Tap(Vector2d tappedPosition)
    {
        if (ObjectInteraction.Instance.objectInFocus != null)
        {
            ObjectInteraction.Instance.Place();
        }    

        if (UIManager.Instance.currentMode == "ZoomInBtn")
        {
            Debug.Log("ZoomIn to " + tappedPosition);
            Zoom(1, tappedPosition);
        }
        if (UIManager.Instance.currentMode == "ZoomOutBtn")
        {
            Debug.Log("ZoomOut to " + tappedPosition);
            Zoom(0, tappedPosition);
        }
        if (UIManager.Instance.currentMode == "CenterBtn")
        {
            Debug.Log("Center to " + tappedPosition);
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
        AppState.Instance.ResetMap(view);
    }

    public void Zoom(int zoomDirection, Vector2d tappedPosition)
    {
        if (tableIsTilted)
        {
            ResetTableTilt();
        }

        var view = AppState.Instance.Config.ActiveView;
        Vector2 latLonv2 = Extensions.ToVector2(tappedPosition);

        // ZoomDirection 0 = out 1 = in
        if (zoomDirection == 0 && view.Zoom > minZoomLevel)
        {
            Debug.Log("Current Zoom level: " + view.Zoom + " | New Zoom level =  " + (view.Zoom - 1));   
            view.Zoom -= 1;

            view.SetView(latLonv2.x, latLonv2.y, view.Zoom, view.Range);
            //UIManager.Instance.SetTextValues();
            RescaleBoardObjects(zoomDirection);
            var boardColSize = AppState.Instance.Board.GetComponent<BoxCollider>().size;
            AppState.Instance.ResetMap(view);
            AppState.Instance.Board.GetComponent<BoxCollider>().size = new Vector3(boardColSize.x, boardColSize.y / 2, boardColSize.z);
        }

        if (zoomDirection == 1 && view.Zoom < maxZoomLevel)
        {
            Debug.Log("Current Zoom level: " + view.Zoom + " | New Zoom level =  " + (view.Zoom + 1));
            view.Zoom += 1;

            view.SetView(latLonv2.x, latLonv2.y, view.Zoom, view.Range);
           // UIManager.Instance.SetTextValues();
            RescaleBoardObjects(zoomDirection);
            var boardColSize = AppState.Instance.Board.GetComponent<BoxCollider>().size;
            AppState.Instance.ResetMap(view);
            AppState.Instance.Board.GetComponent<BoxCollider>().size = new Vector3(boardColSize.x, boardColSize.y * 2, boardColSize.z);
        }
    }
    
    private void RescaleBoardObjects(int zoomDirection)
    {
        foreach (var spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
        {
            // Zoom out
            if (zoomDirection == 1)
            {
                spawnedObject.obj.transform.localScale = spawnedObject.obj.transform.localScale * scaleFactor;
            }
            // Zoom in
            if (zoomDirection == 0)
            {
                spawnedObject.obj.transform.localScale = spawnedObject.obj.transform.localScale / scaleFactor;
            }
        }
    }

    public void SwitchMapOverlay(int view, string name, int zoomLevel)
    {
        AppState.Instance.Config.ActiveView = AppState.Instance.Config.Views[view].Clone();
        AppState.Instance.Config.ActiveView.Zoom = 17;
        AppState.Instance.ResetMap();
        SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
        UIManager.Instance.CurrentOverlayText.text = AppState.Instance.Config.ActiveView.Name.ToString();
    }

    public void ResetTable()
    {
        terrain.transform.localScale = originalTableScale;
        terrain.transform.rotation = originalTableRotation;
        tableIsTilted = false;
    }

    public void ResetTableTilt()
    {
        terrain.transform.rotation = originalTableRotation;
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
        //UIInteraction.Instance.HandlerPanel.SetActive(false);
        UIInteraction.Instance.MapPanel.SetActive(false);
        UIInteraction.Instance.InventoryPanel.SetActive(false);
        UIInteraction.Instance.InventoryTextMesh.text = "Open \nInventory";
    }

    public void StopManipulatingTable()
    {
        InputManager.Instance.OverrideFocusedObject = null;
        tableIsRotating = false;       
    }

    public void UpdateTableRotation(NavigationEventData eventData)
    {
        var rotationFactor = eventData.CumulativeDelta.x * rotationSensitivity;
        terrain.transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
        UIManager.Instance.SetOriginalRotation();
    }

    public void UpdateTableTilt(ManipulationEventData eventData)
    {
        var tableOrientation = UIManager.Instance.currentUIPosition;
        tableIsTilted = true;
        var tiltFactorX = eventData.CumulativeDelta.x * tiltSensitivity;
        var tiltFactorY = eventData.CumulativeDelta.y * tiltSensitivity;
        //var tiltFactorZ = eventData.CumulativeDelta.z * tiltSensitivity;
        

        switch (tableOrientation)
        {
            case "Front":
                terrain.transform.Rotate(new Vector3(tiltFactorY, 0, -1 * tiltFactorX));
                break;
            case "Left":
                terrain.transform.Rotate(new Vector3(-1 * tiltFactorX, 0, -1 * tiltFactorY));
                break;

            case "Right":
                terrain.transform.Rotate(new Vector3(tiltFactorX, 0, tiltFactorY));
                break;

            case "Back":
                terrain.transform.Rotate(new Vector3( -1* tiltFactorY, 0,tiltFactorX));
                break;

            default:
                break;
        }

        // Save new rotation
        UIManager.Instance.SetOriginalRotation();
    }

    public void UpdateTableSize(NavigationEventData eventData)
    {        
        // Size up.
        if (eventData.CumulativeDelta.x >= 0)
        {
            if (terrain.transform.localScale.x < maximumScale && terrain.transform.localScale.y < maximumScale && terrain.transform.localScale.z < maximumScale)
            {
                terrain.transform.localScale += new Vector3((terrain.transform.localScale.x * scaleSpeed), (terrain.transform.localScale.y * scaleSpeed), (terrain.transform.localScale.z * scaleSpeed));
            }
        }

        // Size down.
        if (eventData.CumulativeDelta.x <= 0)
        {
            Debug.Log("Try Scaling down");
            if (terrain.transform.localScale.x > minimumScale && terrain.transform.localScale.y > minimumScale && terrain.transform.localScale.z > minimumScale)
            {
                Debug.Log("Scaling down");
                terrain.transform.localScale -= new Vector3((terrain.transform.localScale.x * scaleSpeed), (terrain.transform.localScale.y * scaleSpeed), (terrain.transform.localScale.z * scaleSpeed));
            }

        }
    }

    public void IncreaseTiles()
    {
        var av = AppState.Instance.Config.ActiveView;
        var currentRange = av.Range;
        var newRange = av.Range + 1;

        if (newRange <= maxTileRange)
        {
            AppState.Instance.Config.ActiveView.TileSize = 150;
            AppState.Instance.Config.ActiveView.Range = newRange;
            UIInteraction.Instance.SetTileRangeButtons(AppState.Instance.Config.ActiveView.Range);
            AppState.Instance.ResetMap();
            SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
        }
    }

    public void DecreaseTiles()
    {
        var av = AppState.Instance.Config.ActiveView;
        var currentRange = av.Range;
        var newRange = av.Range - 1;
        if (newRange >= minTileRange)
        { 
            if (newRange == 1)
            {
                AppState.Instance.Config.ActiveView.TileSize = 125;
            }
            AppState.Instance.Config.ActiveView.Range = newRange;
            UIInteraction.Instance.SetTileRangeButtons(newRange);
            AppState.Instance.ResetMap();
            SessionManager.Instance.UpdateView(AppState.Instance.Config.ActiveView);
        }
    }

    public void Go(string direction/*, int stepSize = 1*/)
    {
       // InitSessions();
        Debug.Log(string.Format("Go {0}...", direction));
        var view = AppState.Instance.Config.ActiveView;
        var metersPerTile = view.Resolution/* * stepSize*/;
        Debug.Log(string.Format("Moving {0} meters {1}...", metersPerTile, direction));
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
