using UnityEngine;
using MapzenGo.Models;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Classes;
using MapzenGo.Models.Plugins;
using UniRx;
using System.Collections;
using Symbols;

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
        public GameObject Camera;
        public GameObject Map;
        public GameObject Layers;
        public float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };
        public Vector3 Center { get; set; }
        public TileManager TileManager { get; set; }
        public AppConfig Config { get; set; }
        public ViewState State { get; set; }
        public SpeechManager Speech { get; set; }
        public List<string> MapzenTags = new List<string>(new string[] { "buildings", "water", "roads", "pois", "landuse" });

        protected AppState() {} // guarantee this will be always a singleton only - can't use the constructor!

        public void Awake()
        {
            Speech = SpeechManager.Instance;
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
            string json;

            WWW www = new WWW(url);

            while (!www.isDone) Thread.Sleep(50);

            if (!string.IsNullOrEmpty(www.error)) {
                var targetFile = Resources.Load<TextAsset>("config");
                json = targetFile.text;
            } else
            {
                json = www.text;
            }

            Config = new AppConfig();
            Config.FromJson(new JSONObject(json));
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

            Table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Table.name = "Table";
            Table.transform.position = new Vector3(0f, 0.7f, 0f);
            Table.transform.localScale = new Vector3(t.TableSize, t.TableHeight, t.TableSize);
            Table.transform.SetParent(Terrain.transform, false);


            Map = new GameObject("Map");
            Map.transform.SetParent(Table.transform);
            Map.transform.localPosition = new Vector3(0f, 0.5f, 0f);

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
            var iv = Config.ActiveView;

            var i = iv.Range;
            if (i > mapScales.Length) i = mapScales.Length;
            var mapScale = mapScales[i - 1];
            Map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

            World = new GameObject("World");
            World.transform.SetParent(Map.transform, false);
            var gazeManager = World.AddComponent<HoloToolkit.Unity.GazeManager>();
            gazeManager.MaxGazeDistance = 3f;
         
            // init map
#if DEBUG
            var tm = World.AddComponent<TileManager>();
#else
            var tm = World.AddComponent<CachedTileManager>();
            Speech.AddKeyword("Clear cache", () => { tm.ClearCache(); });
            //tm._key = "vector-tiles-dB21RAF";
            //tm._mapzenUrl = "http://134.221.20.226:3999/{0}/{1}/{2}/{3}.{4}";
#endif
            tm._mapzenUrl = "http://" + Config.TileServer + "/{0}/{1}/{2}/{3}.{4}"; // "http://169.254.80.80:10733/{0}/{1}/{2}/{3}.{4}";
            tm.Latitude = iv.Lat;
            tm.Longitude = iv.Lon;
            tm.Range = iv.Range;
            tm.Zoom = iv.Zoom;
            tm.TileSize = iv.TileSize;

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

            if (iv.Mapzen.Contains("buildings"))
            {
                var buildings = new GameObject("BuildingFactory");
                buildings.transform.SetParent(factories.transform, false);
                var buildingFactory = buildings.AddComponent<BuildingFactory>();
            }

            //var flatBuildings = new GameObject("FlatBuildingFactory");
            //flatBuildings.transform.SetParent(factories.transform, false);
            //var flatBuildingFactory = flatBuildings.AddComponent<FlatBuildingFactory>();

            if (iv.Mapzen.Contains("roads"))
            {
                var roads = new GameObject("RoadFactory");
                roads.transform.SetParent(factories.transform, false);
                var roadFactory = roads.AddComponent<RoadFactory>();
            }

            if (iv.Mapzen.Contains("buildings"))
            {
                var water = new GameObject("WaterFactory");
                water.transform.SetParent(factories.transform, false);
                var waterFactory = water.AddComponent<WaterFactory>();
            }
            if (iv.Mapzen.Contains("boundaries"))
            {
                var boundary = new GameObject("BoundaryFactory");
                boundary.transform.SetParent(factories.transform, false);
                var boundaryFactory = boundary.AddComponent<BoundaryFactory>();
            }

            if (iv.Mapzen.Contains("landuse"))
            {
                var landuse = new GameObject("LanduseFactory");
                landuse.transform.SetParent(factories.transform, false);
                var landuseFactory = landuse.AddComponent<LanduseFactory>();
            }

            if (iv.Mapzen.Contains("places"))
            {
                var places = new GameObject("PlacesFactory");
                places.transform.SetParent(factories.transform, false);
                var placesFactory = places.AddComponent<PlacesFactory>();
            }

            if (iv.Mapzen.Contains("pois"))
            {
                var pois = new GameObject("PoiFactory");
                pois.transform.SetParent(factories.transform, false);
                var poisFactory = pois.AddComponent<PoiFactory>();
            }

            if (iv.Mapzen.Contains("assets")) // assets
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
            iv.Layers.ForEach(layer =>
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
            tileLayerPlugin.tileLayers = Config.Layers.Where(k => { return iv.TileLayers.Contains(k.Title) && k.Type.ToLower() == "tilelayer"; }).ToList();

            foreach (var tl in Config.Layers.Where(k => { return k.Type.ToLower() == "tilelayer"; }))
            {
                Speech.AddKeyword(ShowLayerSpeech + tl.VoiceCommand, () => {
                    if (tl.Group != null)
                    {
                        var ll = Config.Layers.Where(k => k.Type.ToLower() == "tilelayer" && k.Group == tl.Group).Select(k => k.Title);
                        iv.TileLayers = iv.TileLayers.Where(k => !ll.Contains(k)).ToList();
                        iv.TileLayers.Add(tl.Title);
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
                    var layerObject = new GameObject("Layer-" + l.Title);

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