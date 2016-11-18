using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
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
