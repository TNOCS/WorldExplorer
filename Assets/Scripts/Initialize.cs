using UnityEngine;
using MapzenGo.Models;
using MapzenGo.Models.Plugins;
using System;
using System.Text;

public class Initialize : MonoBehaviour
{
    private GameObject world;
    private GameObject terrain;
    private GameObject table;


    private GameObject Layers;
    private GameObject SymbolMap;
    private GameObject Symboltable;
    // Use this for initialization
    private GameObject spatialMapping;
    private GameObject _cursorFab;
    private GameObject cursor;
    private AppState appState;

    float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };
    public string json = "{   \"type\": \"FeatureCollection\",   \"features\": [     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.070362091064453,           53.295336751980656         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"forest\",         \"area\": 35879,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119757239, 		 \"symbol\": \"liaise.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.072250366210937,           53.29523415150025         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"forest\",         \"area\": 1651,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119757777, 		 \"symbol\": \"counterattack_fire.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.066671371459961,           53.29469549493482         ]       },       \"type\": \"Feature\",       \"properties\": {         \"marker-color\": \"#7e7e7e\",         \"marker-size\": \"medium\",         \"marker-symbol\": \"circle-stroked\",         \"kind\": \"app-622\",         \"area\": 18729,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119758146,         \"symbol\": \"warrant_served.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.068731307983398,           53.29497764922103         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"bus_stop\",         \"name\": \"Eureka\",         \"source\": \"openstreetmap.org\",         \"min_zoom\": 17,         \"operator\": \"TCR\",         \"id\": 2833355779, 		 \"symbol\": \"activity.png\"       }     }   ] }";

    void includeAnchorMovingScript()
    {
        var gazeGesture = terrain.AddComponent<GazeGestureManager>();
        var AnchorPlacemant = terrain.AddComponent<TapToPlaceParent>();
        spatialMapping = new GameObject("Spatial Mapping");
        spatialMapping.AddComponent<UnityEngine.VR.WSA.SpatialMappingCollider>();
        spatialMapping.AddComponent<UnityEngine.VR.WSA.SpatialMappingRenderer>();

        var _spatial = spatialMapping.AddComponent<SpatialMapping>();
        _spatial.DrawMaterial = Resources.Load("Wireframe", typeof(Material)) as Material;

        _cursorFab = Resources.Load("_cursor") as GameObject;
        if (_cursorFab)
        {
            cursor = (GameObject)Instantiate(_cursorFab, new Vector3(0, 0, -1), transform.rotation);
            cursor.name = "Cursor";
            var t = cursor.GetComponentInChildren<Transform>().Find("CursorMesh");

            var r = t.GetComponent<MeshRenderer>();
            r.enabled = true;
        }
    }

    void Start()
    {
        var threadDispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();

        appState = AppState.Instance;
        appState.LoadConfig();

        AddTerrain();
#if (NETFX_CORE)
        //InitMqtt();
#endif
       // includeAnchorMovingScript();
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
                            setView(msg);
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
#endif

    protected void setView(string msg)
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

    protected void AddTerrain()
    {
        var iv = appState.Config.InitalView;

        #region create map & terrain

        terrain = new GameObject("terrain");
        terrain.transform.position = new Vector3(0f, 0f, 0f);


        table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.transform.position = new Vector3(0f, 0f, 4f);
        table.transform.localScale = new Vector3(iv.TableSize, iv.TableHeight, iv.TableSize);
        table.transform.SetParent(terrain.transform, false);


        var map = new GameObject("Map");
        map.transform.SetParent(table.transform);
        map.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        var i = iv.Range;
        if (i > mapScales.Length) i = mapScales.Length;
        var mapScale = mapScales[i - 1];
        map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

        

        #endregion       
        #region init map

        world = new GameObject("World");
        world.transform.SetParent(map.transform, false);

        // init map
        var tm = world.AddComponent<CachedTileManager>();
        tm.Latitude = iv.Lat;
        tm.Longitude = iv.Lon;
        tm.Range = iv.Range;
        tm.Zoom = iv.Zoom;
        tm.TileSize = iv.TileSize;
        tm._key = "vector-tiles-dB21RAF";

        appState.TileManager = tm;

      

        #endregion

        #region UI

        var ui = new GameObject("UI"); // Placeholder (root element in UI tree)
        ui.transform.SetParent(world.transform, false);
        var place = new GameObject("PlaceContainer");
        AddRectTransformToGameObject(place);
        place.transform.SetParent(ui.transform, false);

        var poi = new GameObject("PoiContainer");
        AddRectTransformToGameObject(poi);
        poi.transform.SetParent(ui.transform, false);

        #endregion

        #region FACTORIES

        #region defaultfactories
        var factories = new GameObject("Factories");
        factories.transform.SetParent(world.transform, false);

        var buildings = new GameObject("BuildingFactory");
        buildings.transform.SetParent(factories.transform, false);
        var buildingFactory = buildings.AddComponent<BuildingFactory>();



        //var flatBuildings = new GameObject("FlatBuildingFactory");
        //flatBuildings.transform.SetParent(factories.transform, false);
        //var flatBuildingFactory = flatBuildings.AddComponent<FlatBuildingFactory>();

        var roads = new GameObject("RoadFactory");
        roads.transform.SetParent(factories.transform, false);
        var roadFactory = roads.AddComponent<RoadFactory>();

        var water = new GameObject("WaterFactory");
        water.transform.SetParent(factories.transform, false);
        var waterFactory = water.AddComponent<WaterFactory>();

        //var boundary = new GameObject("BoundaryFactory");
        //boundary.transform.SetParent(factories.transform, false);
        //var boundaryFactory = boundary.AddComponent<BoundaryFactory>();

        //var landuse = new GameObject("LanduseFactory");
        //landuse.transform.SetParent(factories.transform, false);
        //var landuseFactory = landuse.AddComponent<LanduseFactory>();

        //var places = new GameObject("PlacesFactory");
        //places.transform.SetParent(factories.transform, false);
        //var placesFactory = places.AddComponent<PlacesFactory>();

        var pois = new GameObject("PoiFactory");
        pois.transform.SetParent(factories.transform, false);
        var poisFactory = pois.AddComponent<PoiFactory>();
        #endregion


        SymbolMap = new GameObject("Layers");
        SymbolMap.transform.SetParent(table.transform);
        SymbolMap.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        SymbolMap.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

        var Symbolworld = new GameObject("Symbols");
        Symbolworld.transform.SetParent(SymbolMap.transform, false);
        var symbolFactory = Symbolworld.AddComponent<SymbolFactory>();
        symbolFactory.geojson = json;
        symbolFactory.zoom = iv.Zoom;
        symbolFactory.Latitude = iv.Lat;
        symbolFactory.Longitude = iv.Lon;
        symbolFactory.TileSize = iv.TileSize;
        symbolFactory.Range = iv.Range;
        symbolFactory.AddSymbols();

        #endregion

        #region TILE PLUGINS

        var tilePlugins = new GameObject("TilePlugins");
        tilePlugins.transform.SetParent(world.transform, false);

        var mapImage = new GameObject("MapImage");
        mapImage.transform.SetParent(tilePlugins.transform, false);
        var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
        mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;

        //var tileLayer = new GameObject("TileLayer");
        //tileLayer.transform.SetParent(tilePlugins.transform, false);
        //var tileLayerPlugin = tileLayer.AddComponent<TileLayerPlugin>();
        //tileLayerPlugin.tileLayers = appState.Config.Layers;

        #endregion

    }

    protected void AddRectTransformToGameObject(GameObject go)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (world != null)
        {
            //world.transform.localScale = new Vector3(0.001F, 0.001F, 0.001F);
            //world.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
