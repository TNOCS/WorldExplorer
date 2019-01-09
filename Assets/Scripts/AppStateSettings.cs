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

        public void LoadConfig(string url)
        {
            StartCoroutine(LoadConfiguration(url));
        }

        public IEnumerator LoadConfiguration(string url)
        {
            string json;

            WWW www = new WWW(url);

            while (!www.isDone) { yield return new WaitForSeconds(0.05F); ; }

            if (!string.IsNullOrEmpty(www.error))
            {
                var targetFile = Resources.Load<TextAsset>("config");
                json = targetFile.text;
            }
            else
            {
                json = www.text;
            }

            Config = new AppConfig();
            Config.FromJson(new JSONObject(json));
            yield return null;
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

            Config.Layers.ForEach(l =>
            {
                DestroyGeojsonLayer(l);
            });

            DoDeleteAll(World);
            DoDeleteAll(Layers);

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
            var iv = Config.ActiveView;
            var t = Config.Table;

            #region create map & terrain

            Terrain = new GameObject("terrain");
            Terrain.transform.position = new Vector3(0f, 0f, 0f);

            Table = new GameObject("Table"); // Empty game object
            Table.transform.position = new Vector3(0f, t.Position, 0f);
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

            #endregion
            InitMap();
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
            if (av.Name == "Compound")
            {
                BoardInteraction.Instance.maxZoomLevel = 18;
            }
            else
            {
                BoardInteraction.Instance.maxZoomLevel = 19;
            }

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
#if DEBUG
            var tm = World.AddComponent<TileManager>();
#else
            var tm = World.AddComponent<TileManager>();
            //Speech.AddKeyword("Clear tiles", () => { tm.ClearCache(); });
            //tm._key = "vector-tiles-dB21RAF";
            //tm._mapzenUrl = "http://134.221.20.226:3999/{0}/{1}/{2}/{3}.{4}";
#endif
            tm._mapzenUrl = "http://" + Config.TileServer + "/{0}/{1}/{2}/{3}.{4}"; // "http://169.254.80.80:10733/{0}/{1}/{2}/{3}.{4}";
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
            if (av.Name == "Compound")
            {
                customObjectsPlugin.hasVMGObjects = true;
            }

            var tileLayer = new GameObject("TileLayer");
            tileLayer.transform.SetParent(tilePlugins.transform, false);
            var tileLayerPlugin = tileLayer.AddComponent<TileLayerPlugin>();
            tileLayerPlugin.tileLayers = Config.Layers.Where(k => { return av.TileLayers.Contains(k.Title) && k.Type.ToLower() == "tilelayer"; }).ToList();

            Layers = new GameObject("Layers");
            Layers.transform.SetParent(Table.transform);
            Layers.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Layers.transform.localScale = new Vector3(mapScale, mapScale, mapScale);
            av.Layers.ForEach(layer =>
            {
                var l = Config.Layers.FirstOrDefault(k => k.Title == layer);
                if (l != null)
                {
                    switch (l.Type)
                    {
                        case "geojson":
                            InitGeojsonLayer(l);
                            break;
                        case "tilelayer":
                            tileLayerPlugin.tileLayers.Add(l);
                            break;
                    }
                }
            });

            #endregion

        }

        public void DestroyGeojsonLayer(Layer l)
        {
            if (l._refreshTimer != null)
            {
                l._refreshTimer.Dispose();
                l._refreshTimer = null;
            }
            if (l._active) RemoveGeojsonLayer(l);
        }

        public void InitGeojsonLayer(Layer l)
        {
            AddGeojsonLayer(l);
            if (l.Refresh > 0)
            {
                var interval = l.Refresh * 1000;
                l._refreshTimer = new System.Threading.Timer(RefreshLayer, l, interval, interval);
            }
        }

        public void RefreshLayer(object d)
        {
            var l = (Layer)d;

            if (l._active)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RemoveGeojsonLayer(l);
                    AddGeojsonLayer(l);
                });
            }
        }

        public void RemoveGeojsonLayer(Layer l)
        {
            Destroy(l._object);
            l._active = false;
        }

        public void AddGeojsonLayer(Layer l)
        {
            if (l._active) return;
            var av = Config.ActiveView;
            Task.Factory.StartNew<string>(() =>
            {
                WebClient wc = new WebClient();
                return wc.DownloadString(l.Url);
            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    // faulted with exception
                    Exception ex = t.Exception;
                    while (ex is AggregateException && ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.LogError(ex.Message);
                }
                else if (t.IsCanceled)
                {

                }
                else
                {
                    var layerObject = new GameObject(l.Title);

                    layerObject.transform.SetParent(Layers.transform, false);
                    l._object = layerObject;
                    l._active = true;

                    var symbolFactory = layerObject.AddComponent<SymbolFactory>();
                    symbolFactory.InitLayer(l, t.Result, av.Zoom, av.Lat, av.Lon, av.TileSize, av.Range);

                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

        

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