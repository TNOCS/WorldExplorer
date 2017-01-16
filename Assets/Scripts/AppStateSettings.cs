using UnityEngine;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Classes;
using MapzenGo.Models.Plugins;
using UniRx;
using Symbols;
using MapzenGo.Models.Factories;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using HoloToolkit.Unity;

namespace Assets.Scripts
{
    public class AppState : Singleton<AppState>
    {
        public const string ShowLayerSpeech = "Show ";
        public const string HideLayerSpeech = "Show ";
        public const string ToggleSpeech = "Toggle ";
        public GameObject World;
        public GameObject Terrain;
        public GameObject Table;
        public GameObject Board;
        public GameObject Camera;
        public GameObject Map;
        public GameObject Layers;
        public GameObject Cursor;
        public float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };
        public Vector3 Center { get; set; }
        public TileManager TileManager { get; set; }
        public AppConfig Config { get; set; }
        public ViewState State { get; set; }
        public SpeechManager Speech { get; set; }
        public List<string> MapzenTags = new List<string>(new string[] { "buildings", "water", "roads", "pois", "landuse" });
        public SelectionHandler selectionHandler;
        protected AppState() { } // guarantee this will be always a singleton only - can't use the constructor!

        public void Awake()
        {
            Speech = SpeechManager.Instance;
            selectionHandler = SelectionHandler.Instance;
        }

        public void Init()
        {
            MapzenTags.ForEach(k => Speech.AddKeyword(ToggleSpeech + k, () => ToggleMapzen(k)));
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

            foreach (var tl in Config.Layers)
            {
                if (tl.Type.ToLower() == "tilelayer")
                {
                    Speech.RemoveKeyword(ShowLayerSpeech + tl.VoiceCommand);
                };
            }

            DoDeleteAll(World);
            DoDeleteAll(Layers);
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
            Table.transform.localScale = new Vector3(t.Size, t.Size, t.Size);
            Table.transform.SetParent(Terrain.transform, false);

            Board = GameObject.Find("Board"); // Although we could create the board in code, this way I can add buttons etc to the object in Unity.
            Board.transform.position = new Vector3(0f, t.Position, 0f);
            Board.transform.localScale = new Vector3(t.Size, t.Thickness, t.Size);
            Board.transform.SetParent(Table.transform, true);

            // Add direction indicator
            var di = Board.AddComponent<DirectionIndicator>();
            di.DirectionIndicatorObject = Resources.Load<GameObject>("Components/DirectionalIndicator");
            di.Cursor = Cursor;

            Map = new GameObject("Map");
            Map.transform.SetParent(Table.transform);
            Map.transform.localPosition = new Vector3(0f, t.Thickness/2, 0f);
            //Map.transform.localPosition = new Vector3(0f, (t.Thickness / 2) / t.Thickness + 0.01F, 0f);

            Terrain.transform.position = new Vector3(Terrain.transform.position.x, t.Position, Terrain.transform.position.z);

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
            //Map.transform.localScale = new Vector3(mapScale, mapScale / Config.Table.Thickness, mapScale);
            Map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

            World = new GameObject("World");
            World.transform.SetParent(Map.transform, false);
            var gazeManager = World.AddComponent<HoloToolkit.Unity.GazeManager>();
            gazeManager.MaxGazeDistance = 3f;

            // Set user CenterInMercator to convert the cursor location to lat/lon
            var v2 = GM.LatLonToMeters(av.Lat, av.Lon);
            var tile = GM.MetersToTile(v2, av.Zoom);
            var centerTms = tile;
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
                var bagBuildingFactory = bagBuildings.AddComponent<BagBuildingFactory>();
            }

            //var flatBuildings = new GameObject("FlatBuildingFactory");
            //flatBuildings.transform.SetParent(factories.transform, false);
            //var flatBuildingFactory = flatBuildings.AddComponent<FlatBuildingFactory>();

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
                modelFactory.BundleURL = "http://134.221.20.226:3999/assets/buildings/eindhoven";
                modelFactory.version = 6;
            }

            #endregion

            Layers = new GameObject("Layers");
            Layers.transform.SetParent(Table.transform);
            Layers.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Layers.transform.localScale = new Vector3(mapScale, mapScale, mapScale);
            av.Layers.ForEach(layer =>
            {
                var l = Config.Layers.FirstOrDefault(k => k.Title == layer && k.Type == "geojson");
                if (l != null)
                {
                    InitGeojsonLayer(l);
                }
            });
            #endregion

            #region TILE PLUGINS

            var tilePlugins = new GameObject("TilePlugins");
            tilePlugins.transform.SetParent(World.transform, false);

            //var mapImage = new GameObject("MapImage");
            //mapImage.transform.SetParent(tilePlugins.transform, false);
            //var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
            //mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;

            var tileLayer = new GameObject("TileLayer");
            tileLayer.transform.SetParent(tilePlugins.transform, false);
            var tileLayerPlugin = tileLayer.AddComponent<TileLayerPlugin>();
            tileLayerPlugin.tileLayers = Config.Layers.Where(k => { return av.TileLayers.Contains(k.Title) && k.Type.ToLower() == "tilelayer"; }).ToList();

            foreach (var tl in Config.Layers.Where(k => { return k.Type.ToLower() == "tilelayer"; }))
            {
                Speech.AddKeyword(ShowLayerSpeech + tl.VoiceCommand, () =>
                {
                    if (tl.Group != null)
                    {
                        var ll = Config.Layers.Where(k => k.Type.ToLower() == "tilelayer" && k.Group == tl.Group).Select(k => k.Title);
                        av.TileLayers = av.TileLayers.Where(k => !ll.Contains(k)).ToList();
                        av.TileLayers.Add(tl.Title);
                        ResetMap();
                    }
                });
            }

            #endregion

        }

        public void DestroyGeojsonLayer(Layer l)
        {
            Speech.RemoveKeyword(ShowLayerSpeech + l.VoiceCommand);
            Speech.RemoveKeyword(HideLayerSpeech + l.VoiceCommand);

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

            Speech.AddKeyword(ShowLayerSpeech + l.VoiceCommand, () =>
            {
                AddGeojsonLayer(l);
            });

            Speech.AddKeyword(HideLayerSpeech + l.VoiceCommand, () =>
            {
                RemoveGeojsonLayer(l);
            });

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
            ObservableWWW.GetWWW(l.Url).Subscribe(
                success =>
                {
                    var layerObject = new GameObject(l.Title);

                    layerObject.transform.SetParent(Layers.transform, false);
                    l._object = layerObject;
                    l._active = true;

                    var symbolFactory = layerObject.AddComponent<SymbolFactory>();
                    symbolFactory.InitLayer(l, success.text, av.Zoom, av.Lat, av.Lon, av.TileSize, av.Range);
                },
                error =>
                {
                    Debug.Log(error);
                }
            );
        }

        public void DoDeleteAll(GameObject Holder)
        {
            //Holder.transform.DetachChildren();
            Destroy(Holder);
            return;
            //while (Holder.transform.childCount > 0)
            //{
            //    var childs = Holder.transform.transform.childCount;
            //    for (var i = 0; i <= childs - 1; i++)
            //    {
            //        var go = Holder.transform.transform.GetChild(i).gameObject;
            //        DoDeleteAll(go);
            //    }
            //}
            //Destroy(Holder);
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