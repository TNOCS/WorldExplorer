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

namespace Assets.Scripts
{

    public class AppState : Singleton<AppState>
    {

        public GameObject World;
        public GameObject Terrain;
        public GameObject Table;
        public GameObject Camera;
        public GameObject Map;
        public GameObject SymbolMap;

        public float[] mapScales = new float[] { 0.004f, 0.002f, 0.00143f, 0.00111f, 0.00091f, 0.00077f, 0.000666f };


        protected AppState()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public Vector3 Center { get; set; }
        public CachedTileManager TileManager { get; set; }

        public AppConfig Config { get; set; }
        public ViewState State { get; set; }
        public SpeechManager Speech { get; set; }

        public void Init()
        {
            Speech = new SpeechManager();
        }

        public void LoadConfig()
        {


            var targetFile = Resources.Load<TextAsset>("config");
            var test = new JSONObject(targetFile.text);

            Config = new AppConfig();
            Config.FromJson(test);
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

            Speech.Keywords.Add("Center Table", () => {
                Table.transform.position = new Vector3(gameObject.transform.position.x, 0.7f, gameObject.transform.position.z);
                //Center = new Vector3(Center.x, Center.y, Center.z + 1);
            });

            Map = new GameObject("Map");
            Map.transform.SetParent(Table.transform);
            Map.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            #endregion
            InitMap();
        }

        public void InitMap()
        {
            var iv = Config.InitalView;

            var i = iv.Range;
            if (i > mapScales.Length) i = mapScales.Length;
            var mapScale = mapScales[i - 1];
            Map.transform.localScale = new Vector3(mapScale, mapScale, mapScale);



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

            var buildings = new GameObject("BuildingFactory");
            buildings.transform.SetParent(factories.transform, false);
            var buildingFactory = buildings.AddComponent<BuildingFactory>();



            //var flatBuildings = new GameObject("FlatBuildingFactory");
            //flatBuildings.transform.SetParent(factories.transform, false);
            //var flatBuildingFactory = flatBuildings.AddComponent<FlatBuildingFactory>();

            var roads = new GameObject("RoadFactory");
            roads.transform.SetParent(factories.transform, false);
            var roadFactory = roads.AddComponent<RoadFactory>();

            //var water = new GameObject("WaterFactory");
            //water.transform.SetParent(factories.transform, false);
            //var waterFactory = water.AddComponent<WaterFactory>();

            //var boundary = new GameObject("BoundaryFactory");
            //boundary.transform.SetParent(factories.transform, false);
            //var boundaryFactory = boundary.AddComponent<BoundaryFactory>();

            var landuse = new GameObject("LanduseFactory");
            landuse.transform.SetParent(factories.transform, false);
            var landuseFactory = landuse.AddComponent<LanduseFactory>();

            var places = new GameObject("PlacesFactory");
            places.transform.SetParent(factories.transform, false);
            var placesFactory = places.AddComponent<PlacesFactory>();

            var pois = new GameObject("PoiFactory");
            pois.transform.SetParent(factories.transform, false);
            var poisFactory = pois.AddComponent<PoiFactory>();
            #endregion


            SymbolMap = new GameObject("Layers");
            SymbolMap.transform.SetParent(Table.transform);
            SymbolMap.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            SymbolMap.transform.localScale = new Vector3(mapScale, mapScale, mapScale);



            var Symbolworld = new GameObject("Symbols");
            Symbolworld.transform.SetParent(SymbolMap.transform, false);

            Config.Layers.ForEach(l =>
            {
                if (l.Type == "geojson" && l.Enabled)
                {
                    ObservableWWW.GetWWW(l.Url).Subscribe(
                               success =>
                               {
                                   var symbolFactory = Symbolworld.AddComponent<SymbolFactory>();
                                   symbolFactory.baseUrl = "http://gamelab.tno.nl/Missieprep/";

                                   symbolFactory.geojson = success.text;  //"{   \"type\": \"FeatureCollection\",   \"features\": [     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.109840,           52.458125         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"forest\",         \"area\": 35879,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119757239, 		 \"symbol\": \"liaise.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.072250366210937,           53.29523415150025         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"forest\",         \"area\": 1651,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119757777, 		 \"symbol\": \"counterattack_fire.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.066671371459961,           53.29469549493482         ]       },       \"type\": \"Feature\",       \"properties\": {         \"marker-color\": \"#7e7e7e\",         \"marker-size\": \"medium\",         \"marker-symbol\": \"circle-stroked\",         \"kind\": \"app-622\",         \"area\": 18729,         \"source\": \"openstreetmap.org\",         \"min_zoom\": 14,         \"tier\": 2,         \"id\": 119758146,         \"symbol\": \"warrant_served.png\"       }     },     {       \"geometry\": {         \"type\": \"Point\",         \"coordinates\": [           5.068731307983398,           53.29497764922103         ]       },       \"type\": \"Feature\",       \"properties\": {         \"kind\": \"bus_stop\",         \"name\": \"Eureka\",         \"source\": \"openstreetmap.org\",         \"min_zoom\": 17,         \"operator\": \"TCR\",         \"id\": 2833355779, 		 \"symbol\": \"activity.png\"       }     }   ] }";
                               symbolFactory.zoom = iv.Zoom;
                                   symbolFactory.Latitude = iv.Lat;
                                   symbolFactory.Longitude = iv.Lon;
                                   symbolFactory.TileSize = iv.TileSize;
                                   symbolFactory.Layer = l;
                                   symbolFactory.Range = iv.Range;
                                   symbolFactory.AddSymbols();


                               },
                               error =>
                               {
                                   Debug.Log(error);
                               }
                           );

                }
            });


            #endregion

            #region TILE PLUGINS

            var tilePlugins = new GameObject("TilePlugins");
            tilePlugins.transform.SetParent(World.transform, false);

            var mapImage = new GameObject("MapImage");
            mapImage.transform.SetParent(tilePlugins.transform, false);
            var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
            mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;

            var tileLayer = new GameObject("TileLayer");
            tileLayer.transform.SetParent(tilePlugins.transform, false);
            var tileLayerPlugin = tileLayer.AddComponent<TileLayerPlugin>();
            tileLayerPlugin.tileLayers = Config.Layers;

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


    }





    public class AppConfig
    {

        public AppConfig()
        {
            //this.Layers = new List<Layer>();
        }

        public void FromJson(JSONObject json)
        {
            TileServer = json.GetString("TileServer");
            MqttServer = json.GetString("MqttServer");
            MqttPort = json.GetString("MqttPort");


            Layers = new List<Layer>();
            var ll = json["Layers"];
            for (var l = 0; l < ll.Count; l++)
            {
                var layer = new Layer();
                layer.FromJson(ll[l]);
                Layers.Add(layer);
            }

            Views = new List<ViewState>();
            var vs = json["Views"];
            for (var l = 0; l < vs.Count; l++)
            {
                var view = new ViewState();
                view.FromJson(vs[l]);
                Views.Add(view);
            }

            InitalView = new ViewState();
            InitalView = Views.FirstOrDefault(v => v.Name == json.GetString("InitialView"));
            Table = new Table();
            Table.FromJson(json["Table"]);

        }

        public List<Layer> Layers { get; set; }
        public string TileServer { get; set; }
        public string MqttServer { get; set; }
        public string MqttPort { get; set; }
        public List<ViewState> Views { get; set; }
        public Table Table { get; set; }
        public ViewState InitalView { get; set; }
    }

}