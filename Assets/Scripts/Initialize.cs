using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using UnityEngine.UI;
using System.Text;
using System.Collections;

public class Initialize : MonoBehaviour
{
    private const string SwitchToSpeech = "Switch to ";
    private string configUrl = "http://thomvdm.com/hololensconfig.json";
    // Use this for initialization
    /// <summary>
    /// Your own cursor
    /// </summary>
    private GameObject _cursorFab;
    /// <summary>
    /// Cursor for other users
    /// </summary>
    private GameObject _cursorFabOther;
    private GameObject cursor;
    private GameObject HoloManagers;
    private AppState appState;
    private GameObject Hud;

    private Font font;
    private AudioClip fingerPressedSound;

    void Awake()
    {
        Debug.Log("Waking up...");
        // We need this so the MQTT thread can receive messages
        // var mtd = gameObject.AddComponent<UnityMainThreadDispatcher>();
        _cursorFab = Resources.Load("C:\\Afstudeerstage\\UnityProjects\\WorldExplorer - WIP\\Assets\\HoloToolkit\\Input\\Prefabs\\Cursor") as GameObject;
       _cursorFabOther = Resources.Load("Prefabs\\Input\\CursorOther") as GameObject;

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        fingerPressedSound = (AudioClip)Resources.Load("FingerPressed");

        appState = AppState.Instance;
    }

    IEnumerator Start()
    {
        Debug.Log("Initializing...");
        CursorManagerCustom.Instance.InitCursor();
        UIManager.Instance.currentMode = "MoveBtn";

        yield return StartCoroutine(appState.LoadConfiguration(configUrl));

        UIManager.Instance.InitUI();

        appState.Camera = gameObject;
        // init cursor next for sessionmanager
        //appState.Cursor = cursor = Instantiate(_cursorFab, new Vector3(0, 0, 1), transform.rotation);        
        //cursor.GetComponent<Cursor>().enabled = true;
        appState.Cursor = GameObject.Find("Cursor");
        cursor = appState.Cursor;
        //cursor.name = "Cursor";

        // session manager is nesscary for speech so init that next
        // debug mode disables sessionmanager to speed up Unity Play Mode start speed
        if (DebugExplorer.Instance.debugMode == false)
        {
            SessionManager.Instance.cursorPrefab = _cursorFabOther;
            SessionManager.Instance.Init(cursor);
        }

        // then add the terrain this will build the symboltargethandler which needs the sessionmanagr
        appState.AddTerrain();


    }

    // Update is called once per frame
    void Update()
    { 
    }
    public void SetOverlay()
    {
        appState.Config.ActiveView = appState.Config.Views[2].Clone();
        appState.ResetMap();
        SessionManager.Instance.UpdateView(appState.Config.ActiveView);
    }
}
