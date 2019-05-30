using UnityEngine;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Classes;
using MapzenGo.Models.Plugins;
using Symbols;
using MapzenGo.Models.Factories;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using System.Threading.Tasks;
using System.Net;
using System;
using UnityEngine.Events;
using System.Text;
using System.Net.Http;
using System.Threading;

namespace Assets.Scripts
{
    public class AppState : SingletonCustom<AppState>
    {
        public GameObject World;
        public GameObject Terrain;
        public GameObject Table;
        public GameObject Board;
        public GameObject BoardVisual;
        public GameObject Camera;
        public GameObject Map;
        public GameObject Layers;
        public GameObject Cursor;
        public GameObject UI;
        public GameObject UIMain;

        public Vector3 worldOffset;
        public Vector3 scaleOffset;
        public Vector3 rotationOffset;
        public Vector3 terrainOffset;

        public UnityEvent m_ConfiguationLoadFailed;

        //public float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };
        public float[] mapScales = new float[] { 0.008f, 0.004f, 0.00286f, 0.00222f, 0.00182f, 0.00154f, 0.001332f };
        public Vector3 Center { get; set; }
        public TileManager TileManager { get; set; }
        public AppConfig Config { get; set; }
        public ViewState State { get; set; }
        public SpeechManager Speech { get; set; }
        public List<string> MapzenTags = new List<string>(new string[] { "buildings", "water", "roads", "pois", "landuse" });
        public SelectionHandler selectionHandler;
        public UIInteraction uiInteraction;


        public string BaseUrlConfigurationServer { get; private set; }


        private HttpClient httpClient = new HttpClient();

        protected AppState() { } // guarantee this will be always a singleton only - can't use the constructor!\

        protected void Awake()
        {
            selectionHandler = SelectionHandler.Instance;
        }

        private void ToggleMapzen(string tag)
        {
            if (Config == null || Config.ActiveView == null) return;
            if (Config.ActiveView.Mapzen.Contains(tag))
            {
                Config.ActiveView.Mapzen.Remove(tag);
            }
            else
            {
                Config.ActiveView.Mapzen.Add(tag);
            }
            ResetMap();
        }

        private void DisplayLocalHostNotAllowedOnHololens()
        {
            NotifyManager.Instance.AddMessage(NotifyManager.NotifyType.Error, $"Localhost will not work for hololens (only in unity editor).");
        }

        public async void LoadConfigAsync(string pUrl)
        {
            Debug.Log($"Load config async (thread id {Thread.CurrentThread.ManagedThreadId})");
            if (String.IsNullOrEmpty(pUrl)) return;
            bool isLocalHost = ((pUrl.IndexOf("localhost") != -1) || ((pUrl.IndexOf("127.0.0.1") != -1)));
#if !UNITY_EDITOR && UNITY_WSA
            if (isLocalHost)
            {
               DisplayLocalHostNotAllowedOnHololens();
            }
#endif


            int index = pUrl.IndexOf(@"/api/configuration/", StringComparison.InvariantCultureIgnoreCase);
            BaseUrlConfigurationServer = (index == -1) ? pUrl : pUrl.Substring(0, index);
            Debug.Log($"Configuration server is {BaseUrlConfigurationServer}");
            string json; 
            try
            {
                json = await httpClient.GetStringAsync(pUrl);
            }
            catch (Exception ex)
            {
                NotifyManager.Instance.AddMessage(NotifyManager.NotifyType.Error, $"Failed to download config at url {pUrl}, error: {ex.Message} (fallback to fixed config)");
                Debug.Log($"Failed to download config at url {pUrl}, error: {ex.Message} (fallback to fixed config)");
                json = Resources.Load<TextAsset>("config").text;
            }
            var cfg = new AppConfig();
            cfg.FromJson(new JSONObject(json));
            Debug.Log("Configuration file loaded.");
            Config = cfg;
            AddTerrain();
        }

        private void InitializeTable()
        {
            Terrain = new GameObject("terrain");
            Terrain.transform.position = new Vector3(0, 0, 0);
            Terrain.transform.SetParent(GameObject.Find("Floor").transform, false);
                       
            Table = new GameObject("Table"); // Empty game object
            Table.transform.position = new Vector3(0f, Config.Table.Position, 0f);
            //Table.transform.localScale = new Vector3(t.Size, t.Size, t.Size);
            Table.transform.localScale = new Vector3(1, 1, 1);
            Table.transform.SetParent(Terrain.transform, false);

            Board = GameObject.Find("Board"); // Although we could create the board in code, this way I can add buttons etc to the object in Unity.
            Board.AddComponent<BoardTapHandler>();
            Board.transform.position = new Vector3(0f, 0f, 0f);
            //Board.transform.localScale = new Vector3(t.Size *2, t.Thickness*2, t.Size*2);

            Board.transform.localScale = new Vector3(1, 1, 1);
            Board.transform.SetParent(Table.transform, false);

            BoardVisual = GameObject.Find("BoardVisual");
            BoardVisual.transform.SetParent(Table.transform, false);

            Map = new GameObject("Map");
            Map.transform.SetParent(Table.transform);
            Map.transform.localPosition = new Vector3(0f, 0f, 0f);
            //Map.transform.localPosition = new Vector3(0f, (t.Thickness / 2) / t.Thickness + 0.01F, 0f);

            UIMain = new GameObject("UIMain");
            UIMain.transform.SetParent(Table.transform, false);

            UI = GameObject.Find("UI");
            UI.transform.SetParent(UIMain.transform, false);
            UIMain.transform.localScale = new Vector3(2, 2, 2);
            Terrain.transform.position = new Vector3(Terrain.transform.position.x, Terrain.transform.position.y - 2, Terrain.transform.position.z + 5);

            BoardInteraction.Instance.terrain = Terrain;
        }



        public void ResetMap(ViewState view = null)
        {
            if (view != null)
            {
                TileManager.Latitude = view.Lat;
                TileManager.Longitude = view.Lon;
                TileManager.Zoom = view.Zoom;
                TileManager.Range = view.Range;
            }

            Config.GeoJsonLayers.ForEach(l =>
            {
                l.DestroyGeojsonLayer();
            });

            Destroy(World);
            Destroy(Layers);

            // Sets each spawned object inactive. Relevant objects are reactivated in CustomObjectsPlugin.cs during a map load.
            foreach (SpawnedObject go in InventoryObjectInteraction.Instance.spawnedObjectsList)
            {
                go.obj.SetActive(false);
            }

            UIInteraction.Instance.MapPanel.SetActive(false);
            UIInteraction.Instance.HandlerPanel.SetActive(false);
            ObjectInteraction.Instance.CloseLabel();

            InitMap();
        }

        public void AddTerrain()
        {
            Debug.Log("Creating the terrain model");
            SessionManager.Instance.Init(AppState.Instance.Cursor);
            var iv = Config.ActiveView;
            var t = Config.Table;

            #region create map & terrain

            InitializeTable();

            #endregion
            InitMap();
        }

        private void ResetTable()
        {

        }

        public void ClearCache()
        {
            var tm = TileManager as CachedTileManager;
            if (tm != null)
            {
                tm.ClearCache();
                ResetMap();
            }
        }

        public void InitMap()
        {
            var av = Config.ActiveView;

            var i = av.Range;
            if (i > mapScales.Length) i = mapScales.Length;
            var mapScale = mapScales[i - 1];
            Map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

            World = new GameObject("World");
            World.transform.SetParent(Map.transform, false);

            // Activate if you want to enable the ability to change tile ranges.
            // Also requires you to activate the buttons in the hierachy.
            // UIInteraction.Instance.SetTileRangeButtons(av.Range);

            // Set users CenterInMercator to convert the cursor location to lat/lon
            var v2 = GM.LatLonToMeters(av.Lat, av.Lon);
            var tile = GM.MetersToTile(v2, av.Zoom);
            var centerTms = tile;

            // Compound has no zoom level 19 .pngs.
            BoardInteraction.Instance.maxZoomLevel = (av.Name == "Compound") ? 18 : 19;


            switch (av.Zoom)
            {
                case 15:
                    mapScale = 0.001f;
                    break;
                case 16:
                    mapScale = 0.002f;
                    break;
                case 17:
                    mapScale = 0.004f;
                    break;
                case 18:
                    mapScale = 0.008f;
                    break;
                case 19:
                    mapScale = 0.016f;
                    break;
                case 20:
                    mapScale = 0.032f;
                    break;
                default:
                    break;
            }

            SessionManager.Instance.me.CenterInMercator = GM.TileBounds(centerTms, av.Zoom).Center;
            SessionManager.Instance.me.Scale = mapScale / 2;

            // init map

            // The TileManager components download vector tiles and displays them
            var tm = World.AddComponent<TileManager>(); // Vector data (geojson layers)
            // Mapzen data is always accessed by World Explorer Server (configure server to use other mapzen provider)
            tm.BaseUrl = BaseUrlConfigurationServer;
            tm.Latitude = av.Lat;
            tm.Longitude = av.Lon;
            tm.Range = av.Range;
            tm.Zoom = av.Zoom;
            tm.TileSize = av.TileSize;
            TileManager = tm;

            #region UI

            var ui = new GameObject("UI"); // Placeholder (root element in UI tree)
            ui.transform.SetParent(World.transform, false);
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

            factories.transform.SetParent(World.transform, false);
            if (av.Mapzen.Contains("buildings"))
            {
                var buildings = new GameObject("BuildingFactory");
                buildings.transform.SetParent(factories.transform, false);
                var buildingFactory = buildings.AddComponent<BuildingFactory>();
            }

            if (av.Mapzen.Contains("gebouwen"))
            {
                var bagBuildings = new GameObject("BagBuildingFactory");
                bagBuildings.transform.SetParent(factories.transform, false);
                if (av.Zoom >= 17)
                {
                    var bagBuildingFactory = bagBuildings.AddComponent<BagBuildingFactory>();
                }
            }

            if (av.Mapzen.Contains("roads"))
            {
                var roads = new GameObject("RoadFactory");
                roads.transform.SetParent(factories.transform, false);
                var roadFactory = roads.AddComponent<RoadFactory>();
            }

            if (av.Mapzen.Contains("buildings"))
            {
                var water = new GameObject("WaterFactory");
                water.transform.SetParent(factories.transform, false);
                var waterFactory = water.AddComponent<WaterFactory>();
            }
            if (av.Mapzen.Contains("boundaries"))
            {
                var boundary = new GameObject("BoundaryFactory");
                boundary.transform.SetParent(factories.transform, false);
                var boundaryFactory = boundary.AddComponent<BoundaryFactory>();
            }

            if (av.Mapzen.Contains("landuse"))
            {
                var landuse = new GameObject("LanduseFactory");
                landuse.transform.SetParent(factories.transform, false);
                var landuseFactory = landuse.AddComponent<LanduseFactory>();
            }

            if (av.Mapzen.Contains("places"))
            {
                var places = new GameObject("PlacesFactory");
                places.transform.SetParent(factories.transform, false);
                var placesFactory = places.AddComponent<PlacesFactory>();
            }

            if (av.Mapzen.Contains("pois"))
            {
                var pois = new GameObject("PoiFactory");
                pois.transform.SetParent(factories.transform, false);
                var poisFactory = pois.AddComponent<PoiFactory>();
            }

            if (av.Mapzen.Contains("assets")) // assets
            {
                var models = new GameObject("ModelFactory");
                models.transform.SetParent(factories.transform, false);
                var modelFactory = models.AddComponent<ModelFactory>();
                modelFactory.scale = Table.transform.localScale.x;
                //modelFactory.BundleURL = "http://134.221.20.226:3999/assets/buildings/eindhoven";
                modelFactory.BundleURL = "http://www.thomvdm.com/assets/buildings/eindhoven";
                modelFactory.version = 21;
            }

#endregion

#endregion

#region TILE PLUGINS

            var tilePlugins = new GameObject("TilePlugins");
            tilePlugins.transform.SetParent(World.transform, false);

            var customObjects = new GameObject("CustomObjects");
            customObjects.transform.SetParent(tilePlugins.transform, false);
            var customObjectsPlugin = customObjects.AddComponent<CustomObjectPlugin>();

            // Checks if VMG objects (which are only available in the Compound area) should be loaded.
            customObjectsPlugin.hasVMGObjects = (av.Name == "Compound");


            var tileLayer = new GameObject("TileLayer");
            tileLayer.transform.SetParent(tilePlugins.transform, false);
            var tileLayerPlugin = tileLayer.AddComponent<TileLayerPlugin>();
            tileLayerPlugin.ActiveView = av;

            Layers = new GameObject("Layers");
            Layers.transform.SetParent(Table.transform);
            Layers.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Layers.transform.localScale = new Vector3(mapScale, mapScale, mapScale);
            av.GeoJsonLayers.ForEach(x => x.InitGeojsonLayer());


#endregion

        }









       



        public void DoDeleteAll(GameObject Holder)
        {
            Destroy(Holder);
            return;
        }

        protected void AddRectTransformToGameObject(GameObject go)
        {
            var rt = go.AddComponent<RectTransform>();
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
        }
    }
}