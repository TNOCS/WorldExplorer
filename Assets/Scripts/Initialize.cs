using UnityEngine;
using Assets.Scripts;
using MapzenGo.Models;
#if (NETFX_CORE)
using System;
using System.Text;
using Assets.MapzenGo.Models.Enums;
using Assets.Scripts.Utils;
#endif

public class Initialize : MonoBehaviour
{


    // Use this for initialization
    private GameObject _cursorFab;
    private GameObject cursor;
    private AppState appState;

    void includeAnchorMovingScript()
    {


       

        cursor = Instantiate(_cursorFab, new Vector3(0, 0, -1), transform.rotation);
        cursor.name = "Cursor";
 
    }

    void Awake()
    {
        var threadDispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();
        _cursorFab = Resources.Load("Prefabs\\Input\\Cursor") as GameObject;
        appState = AppState.Instance;
        appState.LoadConfig();
    }

    void Start()
    {
        appState.Camera = gameObject;
        appState.Speech = new Assets.Scripts.SpeechManager();

        appState.AddTerrain();
        InitViews();
        InitSpeech();


#if (NETFX_CORE)
        InitMqtt();
#endif
        includeAnchorMovingScript();

        appState.Speech.Init();
    }

    void InitSpeech()
    {
        appState.Speech.Keywords.Add("Zoom out", () =>
        {
            appState.Center = new Vector3(appState.Center.x, appState.Center.y, appState.Center.z + 1);
            // appState.TileManager.UpdateTiles();
        });

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
            appState.Speech.Keywords.Add("Switch to " + v.Name, () =>
            {
                appState.Config.ActiveView = v;
                appState.ResetMap();
            });
        });
    }

#if (NETFX_CORE)
    protected void InitMqtt()
    {
        var client = new uPLibrary.Networking.M2Mqtt.MqttClient(appState.Config.MqttServer, int.Parse(appState.Config.MqttPort), false);
        try
        {
            client.Connect("holoclient");
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to mqtt");
        }
        if (client.IsConnected)
        {
            // register to message received 
            client.MqttMsgPublishReceived += (sender, e) =>
            {                
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var msg = Encoding.UTF8.GetString(e.Message);
                    switch (e.Topic)
                    {
                        case "view":
                            SetView(msg);
                            break;
                    }
                    GameObject _3dText = GameObject.Find("tbTemp");
                    _3dText.GetComponent<TextMesh>().text = msg;
                });
            };

            //// subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "view" }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
    }

    protected void SetView(string msg)
    {
        var view = new JSONObject(msg);
        var iv = appState.Config.InitalView;
        iv.Lat = view.GetFloat("Lat");
        iv.Lon = view.GetFloat("Lon");
        iv.Zoom = view.GetInt("Zoom");
        if (appState.TileManager)
        {
            appState.TileManager.Latitude = iv.Lat;
            appState.TileManager.Longitude = iv.Lon;
            appState.TileManager.Zoom = iv.Zoom;
            appState.ResetMap();
        }
    }
#endif

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
        for (var i = 0; i < Mathf.Min(8, appState.Config.Views.Count); i++)
        {
            if (!Input.GetKeyDown(string.Format("{0}", i + 1))) continue;
            appState.Config.ActiveView = appState.Config.Views[i];
            appState.ResetMap();
            return;
        }
    }
}
