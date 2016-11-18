using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{

    public class ViewState
    {

        public void FromJson(JSONObject json)
        {
            Name = json.GetString("Name");
            Lat = json.GetFloat("Lat");
            Lon = json.GetFloat("Lon");
            Zoom = json.GetInt("Zoom");
            Scale = json.GetInt("Scale");
            Range = json.GetInt("Range");
            TileSize = json.GetInt("TileSize");
            Layers = new List<string>();
            if (json.HasField("Layers"))
            {
                var ll = json["Layers"];
                for (var l = 0; l < ll.Count; l++)
                {
                    Layers.Add(ll[l].str);
                }
            };

            TileLayers = new List<string>();
            if (json.HasField("TileLayers"))
            {
                var ll = json["TileLayers"];
                for (var l = 0; l < ll.Count; l++)
                {
                    TileLayers.Add(ll[l].str);
                }
            };

            Mapzen = new List<string>();
            if (json.HasField("Mapzen"))
            {
                var ll = json["Mapzen"];
                for (var l = 0; l < ll.Count; l++)
                {
                    Mapzen.Add(ll[l].str);
                }
            };
        }

        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public int Zoom { get; set; }
        public int Scale { get; set; }
        public int Range { get; set; }
        public int TileSize { get; set; }
        public List<string> Layers { get; set; }
        public List<string> TileLayers { get; set; }
        public List<string> Mapzen { get; set; }

    }

}
