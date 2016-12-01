using System.Collections.Generic;
using System.Linq;

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
    }
}
