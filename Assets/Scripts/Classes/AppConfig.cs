using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    public class AppConfig
    {
        public void FromJson(JSONObject json)
        {
            TileServer = json.GetString("tileServer");
            MqttServer = json.GetString("mqttServer");
            MqttPort = json.GetString("mqttPort");
            SessionName = json.GetString("sessionName");
            UserName = json.GetString("userName", "John Doe");
            if (json.HasField("selectionColor"))
            {
                var a = json["selectionColor"].GetFloat("a", 1);
                var r = json["selectionColor"].GetFloat("r", 1);
                var g = json["selectionColor"].GetFloat("g", 1);
                var b = json["selectionColor"].GetFloat("b", 1);
                SelectionColor = new Color(r, g, b, a);
            } else
            {
                SelectionColor = Color.yellow;
            }

            Layers = new List<Layer>();
            var ll = json["layers"];
            for (var l = 0; l < ll.Count; l++)
            {
                var layer = new Layer();
                layer.FromJson(ll[l]);
                Layers.Add(layer);
            }

            Views = new List<ViewState>();
            var vs = json["views"];
            for (var l = 0; l < vs.Count; l++)
            {
                var view = new ViewState();
                view.FromJson(vs[l]);
                Views.Add(view);
            }

           //InitalView = new ViewState();
            ActiveView = Views.FirstOrDefault(v => v.Name == json.GetString("initialView"));
            Table = new Table();
            Table.FromJson(json["table"]);
        }

        public List<Layer> Layers { get; set; }
        public string TileServer { get; set; }
        public string MqttServer { get; set; }
        public string MqttPort { get; set; }
        public List<ViewState> Views { get; set; }
        public Table Table { get; set; }
        public ViewState ActiveView { get; set; }
        public string SessionName { get; set; }
        public string UserName { get; set; }
        public Color SelectionColor { get; set; }

    }
}
