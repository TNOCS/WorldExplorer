using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
#if (NETFX_CORE)
using Assets.MapzenGo.Models.Enums;
using Assets.Scripts.Utils;
#endif

public class Initialize : MonoBehaviour
{
    [SerializeField]

    private string configUrl = "https://dl.dropboxusercontent.com/s/co6kjmkgls2ptnq/config_ronaldb.json?dl=0";
    public const string SwitchToSpeech = "Switch to ";

    // Use this for initialization
    private GameObject _cursorFab;
    private GameObject _cursorFabOther;
    private GameObject cursor;
    private GameObject HoloManagers;
    private AppState appState;
    private GameObject Hud;
    private Dictionary<string, string> audioCommands;
    private Font font;
    private AudioClip fingerPressedSound;
    private SessionManager sessionMgr;

    void Awake()
    {
        Debug.Log("Waking up...");
        // We need this so the MQTT thread can receive messages
        // var mtd = gameObject.AddComponent<UnityMainThreadDispatcher>();
        _cursorFab = Resources.Load("Prefabs\\Input\\Cursor") as GameObject;
        _cursorFabOther  = Resources.Load("Prefabs\\Input\\CursorOther") as GameObject;
        
         appState = AppState.Instance;
        appState.LoadConfig(configUrl);
        Hud = GameObject.Find("HUDCanvas");
        audioCommands = new Dictionary<string, string>();
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        fingerPressedSound = (AudioClip)Resources.Load("FingerPressed");
    }

    void InitHud()
    {
        HoloManagers = new GameObject("HoloManagers");
        var Handsmanager = HoloManagers.AddComponent<Assets.Scripts.Utils.HandsManager>();
        Handsmanager.FingerPressedSound = fingerPressedSound;
        
        GameObject textO = new GameObject("Commands-Help");
        textO.transform.SetParent(Hud.transform);
        Text info = textO.AddComponent<Text>();

        RectTransform rt = textO.GetComponent(typeof(RectTransform)) as RectTransform;
       
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1f, 1f);
        rt.position = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(0, 0);

        info.font = font;
        info.resizeTextForBestFit = true;
        info.verticalOverflow = VerticalWrapMode.Truncate;

        StringBuilder s = new StringBuilder();
        s.AppendLine("Commands:");
        int h = 1;
        foreach (var item in audioCommands)
        {
            s.AppendLine(item.Key + ": " + item.Value);
            h++;
        }
        rt.sizeDelta = new Vector2(350, (h + 1) * 25);
        info.text = s.ToString();
        Hud.SetActive(false);
    }

    void Start()
    {
        Debug.Log("Initializing...");
        appState.Camera = gameObject;

        //appState.Speech = SpeechManager.Instance;
        cursor = Instantiate(_cursorFab, new Vector3(0, 0, -1), transform.rotation);
        cursor.name = "Cursor";
        appState.AddTerrain();
        InitSpeech();
        InitViews();
        InitHud();
        sessionMgr = SessionManager.Instance;
        sessionMgr.cursorPrefab = _cursorFabOther;
        sessionMgr.Init();

        appState.Speech.Init();
    }

    void InitSpeech()
    {



        audioCommands.Add("Hide Commands", " Hides the voice commands");
        appState.Speech.Keywords.Add("Hide Commands", () =>
        {
            Hud.SetActive(false);
            // appState.TileManager.UpdateTiles();
        });
        audioCommands.Add("Show Commands", " Displays the voice commands");
        appState.Speech.Keywords.Add("Show Commands", () =>
        {
            Hud.SetActive(true);
            // appState.TileManager.UpdateTiles();
        });
        audioCommands.Add("Center table", " Places the table at your current position");
        appState.Speech.Keywords.Add("Center Table", () =>
        {
            appState.Table.transform.position = new Vector3(gameObject.transform.position.x, 0.7f, gameObject.transform.position.z);
            //Center = new Vector3(Center.x, Center.y, Center.z + 1);
        });
    }

    void InitViews()
    {

        appState.Config.Views.ForEach(v =>
        {
            var cmd = SwitchToSpeech + v.Name;
            audioCommands.Add(cmd, " displays the view");

            appState.Speech.Keywords.Add(cmd + v.Name, () =>
            {
                appState.Config.ActiveView = v.Clone();
                appState.ResetMap();
                sessionMgr.UpdateView(appState.Config.ActiveView);
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.LeftArrow)) appState.Center = new Vector3(-1, 0, 0);
        //if (Input.GetKeyDown(KeyCode.RightArrow)) appState.Center = new Vector3(1, 0, 0);
        //if (Input.GetKeyDown(KeyCode.DownArrow)) appState.Center = new Vector3(0, 0, -1);
        //if (Input.GetKeyDown(KeyCode.UpArrow)) appState.Center = new Vector3(0, 0, 1);
        //if (Input.GetKeyDown(KeyCode.I)) appState.Center = new Vector3(0, 1, 0);
        //if (Input.GetKeyDown(KeyCode.O)) appState.Center = new Vector3(0, -1, 0);
        if (Input.GetKeyDown(KeyCode.C))
        {
            appState.ClearCache();
        }
        if (appState.Config == null) return;
        for (var i = 0; i < Mathf.Min(9, appState.Config.Views.Count); i++)
        {
            if (!Input.GetKeyDown(string.Format("{0}", i + 1))) continue;
            appState.Config.ActiveView = appState.Config.Views[i].Clone();
            appState.ResetMap();
            return;
        }
    }
}
