using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using System.Collections;

public class Initialize : MonoBehaviour
{
    private string configUrl = "http://thomvdm.com/hololensconfig.json";
    // Use this for initialization
    /// <summary>
    /// Your own cursor
    /// </summary>
    private GameObject cursorFabSelf;
    /// <summary>
    /// Cursor for other users
    /// </summary>
    private GameObject cursorFabOther;
    private GameObject cursor;
    private GameObject HoloManagers;
    private AppState appState;
    private Font font;
    private AudioClip fingerPressedSound;

    void Awake()
    {
        Debug.Log("Waking up...");
        // We need this so the MQTT thread can receive messages
        // var mtd = gameObject.AddComponent<UnityMainThreadDispatcher
        cursorFabSelf = Resources.Load("Prefabs/BasicCursor") as GameObject;
        cursorFabOther = Resources.Load("Prefabs/BasicCursorOther") as GameObject;
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
        appState.Cursor = cursor = GameObject.Find("Cursor");
        SessionManager.Instance.cursorPrefab = cursorFabOther;
        SessionManager.Instance.Init(cursor);
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
