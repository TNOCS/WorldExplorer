using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Classes;
using Assets.Scripts.Utils;

namespace Assets.Scripts
{

    public class AppState : Singleton<AppState>
    {

        public GameObject World;
        public GameObject Terrain;
        public GameObject Table;
        public GameObject Camera;

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
            for (var l = 0; l < ll.Count; l++)
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