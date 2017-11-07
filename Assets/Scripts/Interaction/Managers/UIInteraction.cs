using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInteraction : Singleton<UIInteraction>{

    public GameObject BoardUI;
    public GameObject EditUI;
    public GameObject DrawUI;
    public GameObject MapPanel;
    public GameObject HandlerPanel;
    public GameObject InventoryPanel;
    public GameObject minTileRangeButton;
    public GameObject plusTileRangeButton;
    public TextMesh InventoryTextMesh;

   /* [SerializeField]
    private List<GameObject> allInventoryItems = new List<GameObject>();
    [SerializeField]
    private List<GameObject> visibleInventoryItems = new List<GameObject>();
    private List<GameObject> itemPositions = new List<GameObject>();
    private int visibleItems = 12;
    [SerializeField]
    private Dictionary<int, Vector3> positionDict = new Dictionary<int, Vector3>();
    */
    private void Awake()
    {
        BoardUI = GameObject.Find("BoardUI");
        EditUI = GameObject.Find("EditUI");
        DrawUI = GameObject.Find("DrawUI");

        MapPanel = GameObject.Find("MapPanel");
        HandlerPanel = GameObject.Find("HandlerPanel");       
        InventoryPanel = GameObject.Find("InventoryPanel");
        InventoryTextMesh = GameObject.Find("InventoryTxt").GetComponent<TextMesh>();
    }

    private void Start()
    {
        EditUI.SetActive(false);
        DrawUI.SetActive(false);

        /*
        // Retrieves all items positions and adds them to a list
        var inventoryItems = GameObject.Find("InventoryItems");
        for (int i = 0; i < visibleItems; i++)
        {
            Debug.Log("Retrieving item" + i);
            positionDict[i] = inventoryItems.transform.GetChild(i).gameObject.transform.position;
        }

        // Retrieves all prefabs from the Inventory folder in Resources and add them to a list
        Object[] retrievedInventoryItems = Resources.LoadAll("Inventory", typeof(GameObject));
        foreach (GameObject retrievedObject in retrievedInventoryItems)
        {            
            GameObject go = (GameObject)retrievedObject;
            Debug.Log("Retrieving prefab" + go);
            allInventoryItems.Add(go);
        }

        // Instantiate and set each item to the correct position
        var count = 0;
        foreach (GameObject go in allInventoryItems)
        {
            var obj = Instantiate(go) as GameObject;
            var emptyGo = new GameObject();
            obj.transform.SetParent(emptyGo.transform, false);
            if (count < visibleItems)
            {
                emptyGo.transform.SetParent(inventoryItems.transform.GetChild(count), false);
                //obj.transform.localScale = obj.transform.localScale;
                Debug.Log("Setting Position of item" + count);
                emptyGo.transform.position = positionDict[count];

            }
            else
            {
                Debug.Log("Set inactive " + obj);
                obj.SetActive(false);
            }
            count++;
        }*/
    }

    public void SwitchMode(string mode)
    {
        if (mode == "BoardBtn")
        {
            BoardUI.SetActive(true);
            EditUI.SetActive(false);
            DrawUI.SetActive(false);

            HandlerPanel.SetActive(false);
            MapPanel.SetActive(false);
            InventoryPanel.SetActive(false);
            InventoryTextMesh.text = "Open \nInventory";
        }

        if (mode == "EditBtn")
        {
            BoardUI.SetActive(false);
            EditUI.SetActive(true);
            DrawUI.SetActive(false);            
        }

        if (mode == "DrawBtn")
        {
            BoardUI.SetActive(false);
            EditUI.SetActive(false);
            DrawUI.SetActive(true);

            HandlerPanel.SetActive(false);
            MapPanel.SetActive(false);
            InventoryPanel.SetActive(false);
            InventoryTextMesh.text = "Open \nInventory";
        }
    }

    public void SetMapWindow( )
    {
        Debug.Log("Setting map window to " + !MapPanel.activeInHierarchy);
        MapPanel.SetActive(!MapPanel.activeInHierarchy);
        HandlerPanel.SetActive(false);
    }
    
    public void SetDragHandlerWindow()
    {
        HandlerPanel.SetActive(!HandlerPanel.activeInHierarchy);
        MapPanel.SetActive(false);        
    }

    public void SetInventoryWindow()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeInHierarchy);

        if (InventoryPanel.activeInHierarchy)
        {
            InventoryTextMesh.text = "Close \nInventory";
        }
        else
        {
            InventoryTextMesh.text = "Open \nInventory";
        }
    }

    public void SetTileRangeButtons(int range)
    {
        if (minTileRangeButton == null)
        {
            minTileRangeButton = GameObject.Find("TileMinBtn");
        }
        if (plusTileRangeButton == null)
        {
            plusTileRangeButton = GameObject.Find("TilePlusBtn");
        }

        if (range == BoardInteraction.Instance.minTileRange)
        {
            minTileRangeButton.SetActive(false);
        }
        if (range == BoardInteraction.Instance.maxTileRange)
        {
            plusTileRangeButton.SetActive(false);
        }
        if (range != BoardInteraction.Instance.minTileRange && range != BoardInteraction.Instance.maxTileRange)
        {
            minTileRangeButton.SetActive(true);
            plusTileRangeButton.SetActive(true);
        }
    }

    // For SpeechManager
    public void OpenHandlers()
    {
        HandlerPanel.SetActive(true);
        MapPanel.SetActive(false);
    }

    // For SpeechManager
    public void CloseHandlers()
    {
        HandlerPanel.SetActive(false);
    }

    // For SpeechManager
    public void OpenMaps()
    {
        MapPanel.SetActive(true);
        HandlerPanel.SetActive(false);
    }

    // For SpeechManager
    public void CloseMaps()
    {
        HandlerPanel.SetActive(false);
    }

    // For SpeechManager
    public void OpenInventory()
    {
        InventoryPanel.SetActive(true);
        InventoryTextMesh.text = "Close \nInventory";
    }

    // For SpeechManager
    public void CloseInventory()
    {
        InventoryPanel.SetActive(false);
        InventoryTextMesh.text = "Open \nInventory";
    }
}
