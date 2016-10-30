using UnityEngine;
using MapzenGo.Models;
using MapzenGo.Models.Plugins;

public class Initialize : MonoBehaviour
{
    private GameObject world;
    private GameObject terrain;
    // Use this for initialization
    private GameObject spatialMapping;
    private GameObject _cursorFab;
    private GameObject cursor;
    private AppState appState;
    private GameObject table;
    float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };
    private float _latitude;
    private float _longitude;
    private int _range;
    private int _zoom;
    //private int _TitleSize = 100;
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
        appState = AppState.Instance;

        // get config
        appState.LoadConfig();
        
        AddTerrain();


        // includeAnchorMovingScript();

    }

    protected void AddTerrain()
    {
        var iv = appState.Config.InitalView;

        #region create map & terrain

        terrain = new GameObject("terrain");
        terrain.transform.position = new Vector3(0f, 0f, 0f);        

        table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.transform.position = new Vector3(0f, 0f, 3f);
        table.transform.localScale = new Vector3(iv.TableSize, iv.TableHeight, iv.TableSize);
        table.transform.parent = terrain.transform;


        var map = new GameObject("Map");
        map.transform.parent = table.transform;
        map.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        
        var mapScale = mapScales[iv.Range-1];
        map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

        #endregion       

        #region init map

        world = new GameObject("World");
        world.transform.parent = map.transform;

        // init map
        var tm = world.AddComponent<TileManager>();
        tm.Latitude = iv.Lat;
        tm.Longitude = iv.Lon;
        tm.Range = iv.Range;
        tm.Zoom = iv.Zoom;
        tm.TileSize = iv.TileSize;
        tm._key = "vector-tiles-dB21RAF";

        #endregion

        #region UI

        var ui = new GameObject("UI"); // Placeholder (root element in UI tree)
        ui.transform.parent = world.transform;
        var place = new GameObject("PlaceContainer");
        AddRectTransformToGameObject(place);
        place.transform.parent = ui.transform;

        var poi = new GameObject("PoiContainer");
        AddRectTransformToGameObject(poi);
        poi.transform.parent = ui.transform;

        #endregion

        #region FACTORIES

        var factories = new GameObject("Factories");
        factories.transform.parent = world.transform;

        var buildings = new GameObject("BuildingFactory");
        buildings.transform.parent = factories.transform;
        var buildingFactory = buildings.AddComponent<BuildingFactory>();

        var flatBuildings = new GameObject("FlatBuildingFactory");
        flatBuildings.transform.parent = factories.transform;
        var flatBuildingFactory = flatBuildings.AddComponent<FlatBuildingFactory>();

        var roads = new GameObject("RoadFactory");
        roads.transform.parent = factories.transform;
        var roadFactory = roads.AddComponent<RoadFactory>();

        var water = new GameObject("WaterFactory");
        water.transform.parent = factories.transform;
        var waterFactory = water.AddComponent<WaterFactory>();

        var boundary = new GameObject("BoundaryFactory");
        boundary.transform.parent = factories.transform;
        var boundaryFactory = boundary.AddComponent<BoundaryFactory>();

        var landuse = new GameObject("LanduseFactory");
        landuse.transform.parent = factories.transform;
        var landuseFactory = landuse.AddComponent<LanduseFactory>();

        var places = new GameObject("PlacesFactory");
        places.transform.parent = factories.transform;
        var placesFactory = places.AddComponent<PlacesFactory>();

        var pois = new GameObject("PoiFactory");
        pois.transform.parent = factories.transform;
        var poisFactory = pois.AddComponent<PoiFactory>();


        //var _symbolicInitHandler = world.AddComponent<SymbolFactory>();
        //_symbolicInitHandler.geojson = json;
        //_symbolicInitHandler.AddSymbols();
        var _symbolicInitHandler = new GameObject("SymbolFactory");
        _symbolicInitHandler.transform.localScale = terrain.transform.localScale;
        //_symbolicInitHandler.transform.parent = factories.transform;
        var symbolFactory = _symbolicInitHandler.AddComponent<SymbolFactory>();
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
        tilePlugins.transform.parent = world.transform;

        var mapImage = new GameObject("MapImage");
        mapImage.transform.parent = tilePlugins.transform;
        var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
        mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;

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
        //world.transform.localScale = new Vector3(0.001F, 0.001F, 0.001F);
        world.transform.localPosition = new Vector3(0,0,0);
    }
}
