using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Classes;
using Assets.Scripts.Utils;
using MapzenGo.Models.Plugins;
using UniRx;
using System;
using Assets.MapzenGo.Models.Enums;

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


        protected AppState()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public Vector3 Center { get; set; }
        public CachedTileManager TileManager { get; set; }

        public AppConfig Config { get; set; }
        public ViewState State { get; set; }
        public SpeechManager Speech { get; set; }

        public List<string> MapzenTags = new List<string>(new string[] { "buildings", "water", "roads", "pois", "landuse" });

        public void Init()
        {
            Speech = new SpeechManager();

        }

        private void ToggleMapzen(string tag)
        {

            if (Config == null || Config.InitalView == null) return;
            if (Config.InitalView.Mapzen.Contains(tag))
            {
                Config.InitalView.Mapzen.Remove(tag);
            }
            else
            {
                Config.InitalView.Mapzen.Add(tag);
            }
            ResetMap();
        }

        public void LoadConfig()
        {


            var targetFile = Resources.Load<TextAsset>("config");
            var test = new JSONObject(targetFile.text);

            Config = new AppConfig();
            Config.FromJson(test);
        }

        public void ResetMap()
        {
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
            var iv = Config.InitalView;
            var t = Config.Table;

            #region create map & terrain

            Terrain = new GameObject("terrain");
            Terrain.transform.position = new Vector3(0f, 0f, 0f);


            Table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Table.transform.position = new Vector3(0f, 0.7f, 0f);
            Table.transform.localScale = new Vector3(t.TableSize, t.TableHeight, t.TableSize);
            Table.transform.SetParent(Terrain.transform, false);



            Map = new GameObject("Map");
            Map.transform.SetParent(Table.transform);
            Map.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            #endregion
            InitMap();
        }

        public void InitMap()
        {
            var iv = Config.InitalView;

            //Enum.GetNames(typeof(PoiType)).ToList().ForEach(pt =>
            //{
            //    if (!MapzenTags.Contains(pt)) MapzenTags.Add(pt);
            //});

            var i = iv.Range;
            if (i > mapScales.Length) i = mapScales.Length;
            var mapScale = mapScales[i - 1];
            Map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);

            MapzenTags.ForEach(k => Speech.AddKeyword(ToggleSpeech + k, () => ToggleMapzen(k)));

            World = new GameObject("World");
            World.transform.SetParent(Map.transform, false);

            // init map
            var tm = World.AddComponent<CachedTileManager>();
            tm.Latitude = iv.Lat;
            tm.Longitude = iv.Lon;
            tm.Range = iv.Range;
            tm.Zoom = iv.Zoom;
            tm.TileSize = iv.TileSize;
            tm._key = "vector-tiles-dB21RAF";

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
            //var boundary = new GameObject("BoundaryFactory");
            //boundary.transform.SetParent(factories.transform, false);
            //var boundaryFactory = boundary.AddComponent<BoundaryFactory>();

            if (iv.Mapzen.Contains("landuse"))
            {
                var landuse = new GameObject("LanduseFactory");
                landuse.transform.SetParent(factories.transform, false);
                var landuseFactory = landuse.AddComponent<LanduseFactory>();
            }

            var places = new GameObject("PlacesFactory");
            places.transform.SetParent(factories.transform, false);
            var placesFactory = places.AddComponent<PlacesFactory>();

            if (iv.Mapzen.Contains("pois"))
            {
                var pois = new GameObject("PoiFactory");
                pois.transform.SetParent(factories.transform, false);
                var poisFactory = pois.AddComponent<PoiFactory>();
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
            var iv = Config.InitalView;
            ObservableWWW.GetWWW(l.Url).Subscribe(
                success =>
                {
                    var layerObject = new GameObject("Layer-" + l.Title);

                    layerObject.transform.SetParent(Layers.transform, false);
                    l._object = layerObject;
                    l._active = true;

                    var symbolFactory = layerObject.AddComponent<SymbolFactory>();
                    //symbolFactory.baseUrl = "http://gamelab.tno.nl/Missieprep/";
                    symbolFactory.geojson = "{   \"type\": \"FeatureCollection\",   \"features\": [     {       \"type\": \"Feature\",       \"properties\": {          \"IconUrl\": \"http://134.221.20.241:3000/images/pomp.png\",  				\"stats\":[{ 				\"name\":\"ammo\", 				\"type\":\"bar\", 				\"value\":\"10\", 				\"maxValue\":\"100\" 				},{ 				\"name\":\"ammo\", 				\"type\":\"bar\", 				\"value\":\"10\", 				\"maxValue\":\"100\" 				},{ 				\"name\":\"ammo\", 				\"type\":\"bar\", 				\"value\":\"10\", 				\"maxValue\":\"100\" 				},{ 				\"name\":\"ammo\", 				\"type\":\"bar\", 				\"value\":\"10\", 				\"maxValue\":\"100\" 				},{ 				\"name\":\"ammo\", 				\"type\":\"bar\", 				\"value\":\"10\", 				\"maxValue\":\"100\" 				}], 				\"Lan\":\"5.0466084480285645\",         \"Lon\":\"52.45997114230474\" 			}, 		 	       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.0466084480285645,           52.45997114230474         ]       }     },     {       \"type\": \"Feature\",       \"properties\": {\"IconUrl\": \"http://134.221.20.241:3000/images/ambulanceposten.png\"},       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.048539638519287,           52.45887287117959         ]       }     },     {       \"type\": \"Feature\",       \"properties\": {\"IconUrl\": \"http://134.221.20.241:3000/images/politie.png\"},       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.046522617340088,           52.45781379807768         ]       }     },     {       \"type\": \"Feature\",       \"properties\": {\"IconUrl\": \"http://134.221.20.241:3000/images/politie.png\"},       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.0501275062561035,           52.461265498103494         ]       }     }   ] }";// success.text;  
                    symbolFactory.zoom = iv.Zoom;
                    symbolFactory.Latitude = iv.Lat;
                    symbolFactory.Longitude = iv.Lon;
                    symbolFactory.TileSize = iv.TileSize;
                    symbolFactory.Layer = l;
                    symbolFactory.Range = iv.Range;
                    symbolFactory.InitLayer();
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
            while (Holder.transform.childCount > 0)
            {
                var childs = Holder.transform.transform.childCount;
                for (var i = 0; i <= childs - 1; i++)
                {
                    var go = Holder.transform.transform.GetChild(i).gameObject;
                    DoDeleteAll(go);
                }
            }
            Destroy(Holder);
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