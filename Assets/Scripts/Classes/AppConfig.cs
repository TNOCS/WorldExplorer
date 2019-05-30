using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    public class AppConfig
    {
        public enum MessageBusType
        {
            None = 0,
            Mqtt = 1,
            Kafka = 2,
            SignalR = 3
        }
        public void FromJson(JSONObject json)
        {
            // Mapzen (JSON vector data)
            // Mapzen is now always accessed by World Explorer Proxy


            //TileServer = json.GetString("tileServer");
            HeightServer = json.GetString("heightServer");
            MqttServer = json.GetString("mqttServer");
            MqttPort = json.GetString("mqttPort");
            ObjectServer = json.GetString("vmgObjectServer"); 
            SessionName = json.GetString("sessionName");
            UserName = json.GetString("userName", "John Doe");
            MessageBus = (MessageBusType)json.GetInt("MessageBusType", 0);
            KafkaBootstrapServer = json.GetString("KafkaBootstrapServer", "localhost:3501");
            KafkaSchemaRegistryServer = json.GetString("KafkaSchemaRegistryServer", "localhost:3502");
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

            RasterLayers = new List<RasterLayer>();
            GeoJsonLayers = new List<GeoJsonLayer>();
            var ll = json["layers"]["raster"];
            if (ll != null) for (var l = 0; l < ll.Count; l++) RasterLayers.Add(new RasterLayer(ll[l]));

            ll = json["layers"]["geojson"];
            if (ll != null) for (var l = 0; l < ll.Count; l++) GeoJsonLayers.Add(new GeoJsonLayer(ll[l]));


            Views = new List<ViewState>();
            var vs = json["views"];
            for (var l = 0; l < vs.Count; l++)
            {
                Views.Add(new ViewState(RasterLayers, GeoJsonLayers, vs[l]));
            }

           //InitalView = new ViewState();
            ActiveView = Views.FirstOrDefault(v => v.Name == json.GetString("initialView"));
            Table = new Table();
            Table.FromJson(json["table"]);
        }

        public List<RasterLayer> RasterLayers { get; private set; }
        public List<GeoJsonLayer> GeoJsonLayers { get; private set; }

        public string HeightServer { get; set; }
        public string MqttServer { get; set; }
        public string MqttPort { get; set; }

        public string KafkaBootstrapServer { get; set; }
        public string KafkaSchemaRegistryServer { get; set; }

        public string ObjectServer { get; set; }
        public List<ViewState> Views { get; set; }
        public Table Table { get; set; }
        public ViewState ActiveView { get; set; }
        public string SessionName { get; set; }
        public string UserName { get; set; }
        public Color SelectionColor { get; set; }
        public MessageBusType MessageBus { get; set; }


    }
}
