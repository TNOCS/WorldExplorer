using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;
using System.Linq;

public class UIManager : SingletonCustom<UIManager>
{
    /// <summary>
    /// Handles the UI visualization, including the compass, sprites as well as the taps on the buttons.
    /// </summary>

    // Set by taps on UI components.
    public string currentMode;

    private GameObject UI;
    private GameObject Board;
    private GameObject terrain;
    public TextMesh CurrentOverlayText;

    // Zoom Min and Max handlers.
    private GameObject ZoomInBtn;
    private GameObject ZoomOutBtn;
    private GameObject ZoomMaxReached;
    private GameObject ZoomMinReached;

    // Terrain.
    private GameObject TerrainBtn;
    private GameObject TerrainUnavailable;

    // Compass.
    private GameObject CompassUpStatic, CompassLeftStatic, CompassDownStatic, CompassRightStatic;

    // UI Positioning.
    public string currentUIPosition;
    private Transform frontSide, leftSide, rightSide, backSide;
    public Quaternion originalUIRotation;

    // Sprites.
    private SpriteRenderer coloredSprite;

    // Inventory Buttons.
    private GameObject inventoryButton;
    private GameObject inventoryUnavailable;
    private GameObject inventoryText;

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
        {"CompoundBtn", 8 },
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

        TerrainBtn = GameObject.Find("ToggleTerrainBtn");
        TerrainUnavailable = GameObject.Find("TerrainUnavailable");
    }

    void Update()
    {
        if (BoardInteraction.Instance.tableIsRotating == false)
        {
            CheckUIPosition();
        }
        if (AppState.Instance.Config != null)
        {
            SetZoomLevelMinMaxIcons();
            SetTerrainUnavailableIcon();
        }
    }

    private void SetZoomLevelMinMaxIcons()
    {
        var av = AppState.Instance.Config.ActiveView;

        if (av.Zoom == BoardInteraction.Instance.minZoomLevel)
        {
            ZoomMaxReached.SetActive(true);
            ZoomOutBtn.SetActive(false);
        }
        else
        {
            ZoomMaxReached.SetActive(false);
            ZoomOutBtn.SetActive(true);
        }

        if (av.Zoom == BoardInteraction.Instance.maxZoomLevel)
        {
            ZoomMinReached.SetActive(true);
            ZoomInBtn.SetActive(false);
        }
        else
        {
            if (av.Name == "Compound" && av.Zoom >= 18)
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
    }


    public void SetTerrainUnavailableIcon()
    {
        TerrainUnavailable.SetActive(!AppState.Instance.Config.ActiveView.TerrainHeightsAvailable);
        TerrainBtn.SetActive(AppState.Instance.Config.ActiveView.TerrainHeightsAvailable);
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

    public void SetInventoryWindowBasedOnZoom()
    {
        // Disables inventory when at zoom level 16 or lower, as objects would become too small.
        if (AppState.Instance.Config.ActiveView.Zoom <= 17)
        {
            if (inventoryButton == null || inventoryUnavailable == null || inventoryText == null)
            {
                inventoryButton = GameObject.Find("InventoryBtn");
                inventoryText = GameObject.Find("InventoryTxt");
                inventoryUnavailable = GameObject.Find("InventoryUnavailableTxt");
            }
            inventoryButton.SetActive(false);
            inventoryText.GetComponent<TextMesh>().text = "Inventory";
            inventoryUnavailable.SetActive(true);
        }
        else
        {
            if (inventoryButton == null || inventoryUnavailable == null || inventoryText == null)
            {
                inventoryButton = GameObject.Find("InventoryBtn");
                inventoryText = GameObject.Find("InventoryTxt");
                inventoryUnavailable = GameObject.Find("InventoryUnavailableTxt");
            }
            inventoryButton.SetActive(true);
            inventoryText.GetComponent<TextMesh>().text = "Open \nInventory";
            inventoryUnavailable.SetActive(false);
        }
    }

    private void SetCompassStatic(string up, string left, string right, string down)
    {
        CompassUpStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(up);
        CompassLeftStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(left);
        CompassRightStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(right);
        CompassDownStatic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(down);
    }

    public void SetSpriteColor(GameObject go)
    {
        try
        {
            if (coloredSprite != null)
            {
                coloredSprite.color = Color.white;
            }
            coloredSprite = go.GetComponent<SpriteRenderer>();
            coloredSprite.color = Color.black;
        }
        catch (Exception e)
        {
            Debug.Log("Sprite not set, probably because function was called through speechmanager." + e);
        }
    }

    // Checks the UI position relative to the user and adjusts the compass accordingly.
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
                SetCompassStatic("CompassArrow", null, null, null);
            }
            if (nearestSide == distanceToLeftSide && currentUIPosition != "Left")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 90, originalUIRotation.z);
                currentUIPosition = "Left";
                SetCompassStatic(null, "CompassArrow", null, null);
            }
            if (nearestSide == distanceToBackSide && currentUIPosition != "Back")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 180, originalUIRotation.z);
                currentUIPosition = "Back";
                SetCompassStatic(null, null, null, "CompassArrow");
            }
            if (nearestSide == distanceToRightSide && currentUIPosition != "right")
            {
                UI.transform.localRotation = Quaternion.Euler(originalUIRotation.x, originalUIRotation.y + 270, originalUIRotation.z);
                currentUIPosition = "Right";
                SetCompassStatic(null, null, "CompassArrow", null);
            }
        }
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
                UIInteraction.Instance.SwitchMode("");
                break;
            // case "TileMinBtn":
            //     BoardInteraction.Instance.DecreaseTiles();
            //     break;
            // case "TilePlusBtn":
            //     BoardInteraction.Instance.IncreaseTiles();
            //     break;
            case "MarkMapBtn":
                currentMode = keyword;
                InventoryObjectInteraction.Instance.Spawn("MarkMapBtn");
                break;
            case "InventoryBtn":
                UIInteraction.Instance.SetInventoryWindow();
                break;
            case "ResetTableInteractable":
                BoardInteraction.Instance.ResetTable();
                break;
            case "FinishedInteractable":
                UIInteraction.Instance.SetDragHandlerWindow();
                UIInteraction.Instance.SwitchMode("BoardBtn");
                break;
            case "ToggleTerrainBtn":
                BoardInteraction.Instance.ToggleTerrainHeights();
                break;
            case "UitdamBtn":
            case "BeachBtn":
            case "StationBtn":
            case "LCFBtn":
            case "CityBtn":
            case "BunkerBtn":
            case "LhasaBtn":
            case "CavesBtn":
            case "CompoundBtn":
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
                SetSpriteColor(go);
                CursorManagerCustom.Instance.SetCursorIcon(currentMode);
                break;
        }
    }
}
