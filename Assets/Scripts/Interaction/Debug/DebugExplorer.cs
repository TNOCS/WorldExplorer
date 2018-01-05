using Assets.Scripts;
using Assets.Scripts.Plugins;
using UnityEngine;
public class DebugExplorer : SingletonCustom<DebugExplorer>
{

    public bool debugMode;
    public string currentView;
    private int overlayCounter;
    public GameObject UI, UI1, debugCoordsText;
    public Vector3 devicePosition;
    public Vector3 gazeDirection;

    // For Videomode
    public GameObject Holograms;
    public GameObject[] pois = new GameObject[33];

    void Start()
    {
        debugCoordsText = GameObject.Find("DebugCursorCoords");
        UI = GameObject.Find("UI");

        if (debugMode)
        {
            Debug.Log("REMINDER: DEBUG MODE IS ON.");
        }
    }

    public void ToggleVideoMode()
    {
        if (Holograms == null)
        {
            Holograms = GameObject.Find("Holograms");
        }

        if (pois.Length == 0)
        {
            pois = GameObject.FindGameObjectsWithTag("poi");
        }

        Holograms.SetActive(!Holograms.activeInHierarchy);

        foreach (GameObject poi in pois)
        {
            poi.SetActive(!poi.activeInHierarchy);
        }

    }

    // Update is called once per frame
    void Update()
    {

        // Enable to view lat lon of cursor in the Scene.
        /*
        var view = AppState.Instance.Config.ActiveView;
        var cursorCoords = SessionManager.Instance.me.CursorLocationToVector2d();
        var cursorCoordsv2 = MapzenGo.Helpers.Extensions.ToVector2(cursorCoords);
        var latv2 = cursorCoordsv2.x;
        var lonv2 = cursorCoordsv2.y;        
        var text = debugCoordsText.GetComponent<TextMesh>();        
        text.text = latv2.ToString() + " \n " + lonv2.ToString() + " \n ";
        */

        if (Input.GetKeyDown(KeyCode.Z))
        {
            BoardInteraction.Instance.ToggleTerrainHeights();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            var board = GameObject.Find("terrain");
            board.transform.position = new Vector3(board.transform.position.x, board.transform.position.y, board.transform.position.z + 3);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            var cursorLatLon = SessionManager.Instance.me.CursorLocationToVector2d();
            Debug.Log(cursorLatLon);
            Debug.Log(AppState.Instance.Config.ActiveView.Lat + " | " + AppState.Instance.Config.ActiveView.Lon);

        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.currentMode = "DeleteBtn";
            CursorManagerCustom.Instance.SetCursorIcon("DeleteBtn");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            UIManager.Instance.currentMode = "MoveBtn";
            CursorManagerCustom.Instance.SetCursorIcon("MoveBtn");
        }
    }
}
