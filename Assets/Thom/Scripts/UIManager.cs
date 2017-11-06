using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using Assets.Scripts;
using System;
using Assets.Scripts.Plugins;
using System.Linq;

public class UIManager : Singleton<UIManager> {

    // Set by taps on UI components.
    public string currentMode;

    private GameObject UI;
    private GameObject Board;
    private GameObject terrain;
    //private GameObject ZoomLevelMin, ZoomLevelPlus;
    //private TextMesh ZoomLevelText;
    public TextMesh CurrentOverlayText;

    // Zoom Min and Max handlers
    private GameObject ZoomInBtn;
    private GameObject ZoomOutBtn;
    private GameObject ZoomMaxReached;
    private GameObject ZoomMinReached;

    // Compass.
    private GameObject CompassUp, CompassLeft, CompassDown, CompassRight;
    private GameObject CompassUpStatic, CompassLeftStatic, CompassDownStatic, CompassRightStatic;

    // UI Positioning.
    public string currentUIPosition;
    private Transform frontSide, leftSide, rightSide, backSide;
    public Quaternion originalUIRotation;    

    private Dictionary<string, int> locationDict = new Dictionary<string, int>()
    {
        {"UitdamBtn", 0 },
        {"BeachBtn", 1 },
        {"StationBtn", 2 },
        {"LCFBtn", 3 },
        {"CityBtn", 4 },
        {"BunkerBtn", 5 },
        {"LhasaBtn", 6 },
        {"CavesBtn", 7 },
        {"VenloBtn", 8 },
        {"WaterBtn", 9 },
        {"MilitaryBtn", 10 },
        {"KazerneBtn", 11 }
    };

    void Awake()
    {        
        CurrentOverlayText = GameObject.Find("CurrentViewText").GetComponent<TextMesh>();
        CompassUpStatic = GameObject.Find("CompassUpStatic");
        CompassLeftStatic = GameObject.Find("CompassLeftStatic");
        CompassRightStatic = GameObject.Find("CompassRightStatic");
        CompassDownStatic = GameObject.Find("CompassDownStatic");

        ZoomMaxReached = GameObject.Find("ZoomOutMax");
        ZoomMinReached = GameObject.Find("ZoomInMax");
        ZoomMinReached.SetActive(false);
        ZoomMaxReached.SetActive(false);

        ZoomInBtn = GameObject.Find("ZoomInBtn");
        ZoomOutBtn = GameObject.Find("ZoomOutBtn");
    }


    void Update()
    {
        if (BoardInteraction.Instance.tableIsRotating == false)
        {
            CheckUIPosition();
        }

        SetZoomLevelMinMaxIcons();
    }

    private void SetZoomLevelMinMaxIcons()
    {
        if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.minZoomLevel)
        {
            ZoomMaxReached.SetActive(true);
            ZoomOutBtn.SetActive(false);
            
        }
        else
        {
            ZoomMaxReached.SetActive(false);
            ZoomOutBtn.SetActive(true);
        }

        if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.maxZoomLevel)
        {
            ZoomMinReached.SetActive(true);
            ZoomInBtn.SetActive(false);
        }
        else
        {
            ZoomMinReached.SetActive(false);
            ZoomInBtn.SetActive(true);
        }

    }
    public void InitUI()
    {
        UIInteraction.Instance.HandlerPanel.SetActive(false);
        UIInteraction.Instance.MapPanel.SetActive(false);
        UIInteraction.Instance.CloseInventory();

        CurrentOverlayText.text = AppState.Instance.Config.ActiveView.Name.ToString();
    }

    public void SetOriginalRotation()
    {
        if (UI == null)
        {
            UI = GameObject.Find("UIMain");
        }
        else
        {
            originalUIRotation = UI.transform.rotation;
        }
    }

    private void SetCompassStatic(string up, string left, string right, string down)
    {
        CompassUpStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(up);
        CompassLeftStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(left);
        CompassRightStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(right);
        CompassDownStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(down);
    }

    private void CheckUIPosition()
    {
        frontSide = GameObject.Find("FrontSide").transform;
        leftSide = GameObject.Find("LeftSide").transform;
        rightSide = GameObject.Find("RightSide").transform;
        backSide = GameObject.Find("BackSide").transform;

        var devicePosition = Camera.main.transform.position;
        var distanceToFrontSide = Vector3.Distance(devicePosition, frontSide.position);
        var distanceToLeftSide = Vector3.Distance(devicePosition, leftSide.position);
        var distanceToRightSide = Vector3.Distance(devicePosition, rightSide.position);
        var distanceToBackSide = Vector3.Distance(devicePosition, backSide.position);
        float[] distances = { distanceToFrontSide, distanceToLeftSide, distanceToRightSide, distanceToBackSide };
        var nearestSide = distances.Min();

        if (terrain == null)
        {
            terrain = GameObject.Find("terrain");
        }
        else
        {
            var terrainPosition = terrain.transform.position;
        }

        if (UI == null)
        {
            UI = GameObject.Find("UIMain");
            SetOriginalRotation();
        }
        else
        {
            if (nearestSide == distanceToFrontSide && currentUIPosition != "Front")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y, originalUIRotation.z);
                currentUIPosition = "Front";

                //SetCompass("N", "W", "E", "S");
                SetCompassStatic("CompassArrow", null, null, null);
            }
            if (nearestSide == distanceToLeftSide && currentUIPosition != "Left")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 90, originalUIRotation.z);
                currentUIPosition = "Left";
                //SetCompass("E", "N", "S", "W");
                SetCompassStatic(null, "CompassArrow", null, null);
            }
            if (nearestSide == distanceToBackSide && currentUIPosition != "Back")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 180, originalUIRotation.z);
                currentUIPosition = "Back";
                //SetCompass("S", "E", "W", "N");
                SetCompassStatic(null, null, null, "CompassArrow");
            }
            if (nearestSide == distanceToRightSide && currentUIPosition != "right")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 270, originalUIRotation.z);
                currentUIPosition = "Right";
                //SetCompass("W", "S", "N", "E");
                SetCompassStatic(null, null, "CompassArrow", null);
            }
        }
    }


    public void InitInventory()
    {
        //Get all Inventory objects from Resources/Inventory
        //Load them in a List
        //Display each listitem as GameObject
        //For each 9th object, create new page
    }

    public void Tap(GameObject go)
    {
        CallFunctions(go.name, go);
    }

    public void CallFunctions(string keyword, GameObject go = null)
    {
        switch (keyword)
        {
            // Every functionality that requires only a single tap
            case "BoardBtn":
            case "EditBtn":
            case "DrawBtn":
                UIInteraction.Instance.SwitchMode(keyword);
                break;
            case "SwitchMapBtn":
                UIInteraction.Instance.SetMapWindow();
                break;
            case "BoardHandlersBtn":
                UIInteraction.Instance.SetDragHandlerWindow();
                break;
            case "TileMinBtn":
                BoardInteraction.Instance.DecreaseTiles();
                break;
            case "TilePlusBtn":
                BoardInteraction.Instance.IncreaseTiles();
                break;
            case "MarkMapBtn":
                currentMode = keyword;
                InventoryObjectInteraction.Instance.Spawn("MarkMapBtn");
                break;
            case "InventoryTxt":
                UIInteraction.Instance.SetInventoryWindow();
                break;
            case "ResetHandler":
                BoardInteraction.Instance.ResetTable();
                break;
            case "UitdamBtn":
            case "BeachBtn":
            case "StationBtn":
            case "LCFBtn":
            case "CityBtn":
            case "BunkerBtn":
            case "LhasaBtn":
            case "CavesBtn":
            case "VenloBtn":
            case "WaterBtn":
            case "MilitaryBtn":
            case "KazerneBtn":
                BoardInteraction.Instance.SwitchMapOverlay(locationDict[keyword], keyword, AppState.Instance.Config.ActiveView.Zoom);
                break;
            case "NorthBtn":
            case "SouthBtn":
            case "EastBtn":
            case "WestBtn":
            case "NorthEastBtn":
            case "NorthWestBtn":
            case "SouthEastBtn":
            case "SouthWestBtn":
                var subString = keyword.Substring(0, keyword.Length - 3).ToLower();
                BoardInteraction.Instance.Go(subString);
                break;
            default:
                // For all functions that require more actions than just a single tap.
                currentMode = keyword;
                CursorManagerThom.Instance.SetCursorIcon(currentMode);
                break;
        }
    }
}
