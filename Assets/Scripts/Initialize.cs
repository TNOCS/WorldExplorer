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
    //private SpeechManager speech;
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
        CursorManagerThom.Instance.InitCursor();
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
        InitViews();


    }

    // Update is called once per frame
    void Update()
    { 
        if (appState.Config == null) return;
       /* for (var i = 0; i < Mathf.Min(9, appState.Config.Views.Count); i++)
        {
            if (!Input.GetKeyDown(string.Format("{0}", i + 1))) continue;
            appState.Config.ActiveView = appState.Config.Views[i].Clone();
            appState.ResetMap();
            SessionManager.Instance.UpdateView(appState.Config.ActiveView);
            return;
        }*/
    }
    public void SetOverlay()
    {
        appState.Config.ActiveView = appState.Config.Views[2].Clone();
        appState.ResetMap();
        SessionManager.Instance.UpdateView(appState.Config.ActiveView);
    }

    void InitViews()
    {
      /*  appState.Config.Views.ForEach(v =>
        {
            var cmd = SwitchToSpeech + v.Name;
            speech.audioCommands.Add(cmd, " displays the view");

            appState.Speech.Keywords.Add(cmd, () =>
            {
                appState.Config.ActiveView = v.Clone();
                appState.ResetMap();
                SessionManager.Instance.UpdateView(appState.Config.ActiveView);
            });
        });*/
    }
}
