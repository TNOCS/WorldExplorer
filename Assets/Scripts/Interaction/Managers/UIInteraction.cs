using Assets.Scripts.Plugins;
using UnityEngine;

public class UIInteraction : SingletonCustom<UIInteraction>
{
    /// <summary>
    /// Handles the interaction with the UI, including switching modes and (de)activating panels/text/buttons.
    /// </summary>

    public GameObject BoardUI;
    public GameObject EditUI;
    public GameObject DrawUI;
    public string currentMode;
    public GameObject MapPanel;
    public GameObject HandlerPanel;
    public GameObject InventoryPanel;
    public GameObject SettingsPanel;
    public TextMesh TableShareText;
    public GameObject minTileRangeButton;
    public GameObject plusTileRangeButton;
    public TextMesh InventoryTextMesh;

    private void Awake()
    {
        BoardUI = GameObject.Find("BoardUI");
        EditUI = GameObject.Find("EditUI");
        DrawUI = GameObject.Find("DrawUI");

        MapPanel = GameObject.Find("MapPanel");
        HandlerPanel = GameObject.Find("HandlerPanel");
        InventoryPanel = GameObject.Find("InventoryPanel");
        SettingsPanel = GameObject.Find("SettingsPanel");
        TableShareText = GameObject.Find("ToggleSharedTableTxt").GetComponent<TextMesh>();
        SettingsPanel.SetActive(false);
        InventoryTextMesh = GameObject.Find("InventoryTxt").GetComponent<TextMesh>();

        SwitchMode("");
    }

    private void Start()
    {
        EditUI.SetActive(false);
        DrawUI.SetActive(false);
    }

    // Handles the switching between board, object and draw menu.
    public void SwitchMode(string mode)
    {
        switch (mode)
        {
            case "BoardBtn":
                if (currentMode == mode)
                {
                    BoardUI.SetActive(false);
                    mode = "";
                }
                else
                {
                    BoardUI.SetActive(true);
                    EditUI.SetActive(false);
                    DrawUI.SetActive(false);
                    HandlerPanel.SetActive(false);
                    MapPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    ObjectInteraction.Instance.CloseLabel();
                }
                break;
            case "EditBtn":
                if (currentMode == mode)
                {
                    EditUI.SetActive(false);
                    ObjectInteraction.Instance.CloseLabel();
                    mode = "";
                }
                else
                {
                    BoardUI.SetActive(false);
                    EditUI.SetActive(true);
                    DrawUI.SetActive(false);
                    HandlerPanel.SetActive(false);
                    UIManager.Instance.SetInventoryWindowBasedOnZoom();
                    if (InventoryPanel.activeInHierarchy)
                    {
                        InventoryTextMesh.text = "Close \nInventory";
                    }
                    else
                    {
                        InventoryTextMesh.text = "Open \nInventory";
                    }
                    //SetTileRangeButtons(AppState.Instance.Config.ActiveView.Range);
                }
                break;
            case "DrawBtn":
                if (currentMode == mode)
                {
                    DrawUI.SetActive(false);
                    mode = "";
                }
                else
                {
                    BoardUI.SetActive(false);
                    EditUI.SetActive(false);
                    DrawUI.SetActive(true);
                    HandlerPanel.SetActive(false);
                    ObjectInteraction.Instance.CloseLabel();
                }
                break;
            default:
                BoardUI.SetActive(false);
                EditUI.SetActive(false);
                DrawUI.SetActive(false);
                break;
        }
        currentMode = mode;

        if (InventoryObjectInteraction.Instance.copyObject != null)
        {
            InventoryObjectInteraction.Instance.copyObject = null;
        }

        if (ObjectInteraction.Instance.objectInFocus != null)
        {
            Destroy(ObjectInteraction.Instance.objectInFocus);
        }
    }

    public void SetMapWindow()
    {
        MapPanel.SetActive(!MapPanel.activeInHierarchy);
        HandlerPanel.SetActive(false);
        ObjectInteraction.Instance.CloseLabel();
    }

    public void SetDragHandlerWindow()
    {
        HandlerPanel.SetActive(!HandlerPanel.activeInHierarchy);
        MapPanel.SetActive(false);
        ObjectInteraction.Instance.CloseLabel();
    }

    public void SetInventoryWindow()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeInHierarchy);
        ObjectInteraction.Instance.CloseLabel();
        if (InventoryPanel.activeInHierarchy)
        {
            InventoryTextMesh.text = "Close \nInventory";
            UIManager.Instance.currentPage = 1;
            UIManager.Instance.SetInventoryArrows();
            UIManager.Instance.SetItems();
        }
        else
        {
            InventoryTextMesh.text = "Open \nInventory";
        }
    }

    // public void SetTileRangeButtons(int range)
    // {
    //     if (minTileRangeButton == null)
    //     {
    //         minTileRangeButton = GameObject.Find("TileMinBtn");
    //     }
    //     if (plusTileRangeButton == null)
    //     {
    //         plusTileRangeButton = GameObject.Find("TilePlusBtn");
    //     }
    //
    //     if (range == BoardInteraction.Instance.minTileRange)
    //     {
    //         minTileRangeButton.SetActive(false);
    //     }
    //     if (range == BoardInteraction.Instance.maxTileRange)
    //     {
    //         plusTileRangeButton.SetActive(false);
    //     }
    //     if (range != BoardInteraction.Instance.minTileRange && range != BoardInteraction.Instance.maxTileRange)
    //     {
    //         minTileRangeButton.SetActive(true);
    //         plusTileRangeButton.SetActive(true);
    //     }
    // }

    public void ToggleSettings()
    {
        SettingsPanel.SetActive(!SettingsPanel.activeInHierarchy);
    }

    public void ToggleSharedTable()
    {
        SessionManager.Instance.ShareTable = !SessionManager.Instance.ShareTable;

        if (SessionManager.Instance.ShareTable)
        {
            TableShareText.text = "Sharing of your \ntable position is ON. \nTap to change.";
        }
        else
        {
            TableShareText.text = "Sharing of your \ntable position is OFF. \nTap to change.";
        }
    }

    #region SpeechManager Functions
    public void OpenHandlers()
    {
        HandlerPanel.SetActive(true);
        MapPanel.SetActive(false);
    }

    public void CloseHandlers()
    {
        HandlerPanel.SetActive(false);
    }

    public void OpenMaps()
    {
        MapPanel.SetActive(true);
        HandlerPanel.SetActive(false);
    }

    public void CloseMaps()
    {
        MapPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        InventoryPanel.SetActive(true);
        UIManager.Instance.currentPage = 1;
        UIManager.Instance.SetInventoryArrows();
        UIManager.Instance.SetItems();
        InventoryTextMesh.text = "Close \nInventory";
    }

    public void CloseInventory()
    {
        InventoryPanel.SetActive(false);
        InventoryTextMesh.text = "Open \nInventory";
    }

    #endregion
}
