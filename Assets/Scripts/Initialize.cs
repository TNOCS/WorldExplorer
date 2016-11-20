using UnityEngine;
using Assets.Scripts;

public class Initialize : MonoBehaviour
{


    // Use this for initialization
    private GameObject _cursorFab;
    private GameObject cursor;
    private AppState appState;
    public string json = @"{
""type"": ""FeatureCollection"",
""features"": [{
    ""geometry"": {
    ""type"": ""Point"",
    ""coordinates"": [5.070362091064453, 53.295336751980656]
},  ""type"": ""Feature"",
    ""properties"": {
        ""kind"": ""forest"",
        ""area"": 35879,
        ""source"": ""openstreetmap.org"",
        ""min_zoom"": 14,
        ""tier"": 2,
        ""id"": 119757239, 		
        ""symbol"": ""liaise.png""
    }
}, {
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [5.072250366210937, 53.29523415150025]
    },
    ""type"": ""Feature"",
    ""properties"": {
        ""kind"": ""forest"",
        ""area"": 1651,
        ""source"": ""openstreetmap.org"",
        ""min_zoom"": 14,
        ""tier"": 2,
        ""id"": 119757777, 		
        ""symbol"": ""counterattack_fire.png""
    }
}, {
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [5.066671371459961, 53.29469549493482]
    }, ""type"": ""Feature"",
        ""properties"": {
            ""marker-color"": ""#7e7e7e"",
            ""marker-size"": ""medium"",
            ""marker-symbol"": ""circle-stroked"",
            ""kind"": ""app-622"",
            ""area"": 18729,
            ""source"": ""openstreetmap.org"",
            ""min_zoom"": 14,
            ""tier"": 2,
            ""id"": 119758146,
            ""symbol"": ""warrant_served.png""
    }
}, {
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [5.068731307983398, 53.29497764922103]
    }, ""type"": ""Feature"",
        ""properties"": {
            ""kind"": ""bus_stop"",
            ""name"": ""Eureka"",
            ""source"": ""openstreetmap.org"",
            ""min_zoom"": 17,
            ""operator"": ""TCR"",
            ""id"": 2833355779, 		
            ""symbol"": ""activity.png""
    }
}]}";

    void includeAnchorMovingScript()
    {
        //var gazeGesture = terrain.AddComponent<GazeGestureManager>();
        //var AnchorPlacemant = terrain.AddComponent<TapToPlaceParent>();
        //spatialMapping = new GameObject("Spatial Mapping");
        //spatialMapping.AddComponent<UnityEngine.VR.WSA.SpatialMappingCollider>();
        //spatialMapping.AddComponent<UnityEngine.VR.WSA.SpatialMappingRenderer>();

        //var _spatial = spatialMapping.AddComponent<SpatialMapping>();
        //_spatial.DrawMaterial = Resources.Load("Wireframe", typeof(Material)) as Material;

        _cursorFab = Resources.Load("_cursor") as GameObject;

        cursor = Instantiate(_cursorFab, new Vector3(0, 0, -1), transform.rotation);
        cursor.name = "Cursor";
        var t = cursor.GetComponentInChildren<Transform>().Find("CursorMesh");

        var r = t.GetComponent<MeshRenderer>();
        r.enabled = true;
    }

    void Awake()
    {
        var threadDispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();

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
                appState.Config.InitalView = v;
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
            appState.TileManager.Start();
        }
    }
#endif

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < Mathf.Min(8, appState.Config.Views.Count); i++)
        {
            if (!Input.GetKeyDown(string.Format("{0}", i + 1))) continue;
            appState.Config.InitalView = appState.Config.Views[i];
            appState.ResetMap();
            return;
        }
    }
}
