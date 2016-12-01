using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Classes
{
    public class ViewState
    {
        public void FromJson(JSONObject json)
        {
            Name = json.GetString("name");
            Lat = json.GetFloat("lat");
            Lon = json.GetFloat("lon");
            Zoom = json.GetInt("zoom");
            Scale = json.GetInt("scale");
            Range = json.GetInt("range");
            TileSize = json.GetInt("tileSize");
            Layers = new List<string>();
            if (json.HasField("layers"))
            {
                var ll = json["layers"];
                for (var l = 0; l < ll.Count; l++)
                {
                    Layers.Add(ll[l].str);
                }
            };

            TileLayers = new List<string>();
            if (json.HasField("tileLayers"))
            {
                var ll = json["tileLayers"];
                for (var l = 0; l < ll.Count; l++)
                {
                    TileLayers.Add(ll[l].str);
                }
            };

            Mapzen = new List<string>();
            if (json.HasField("mapzen"))
            {
                var ll = json["mapzen"];
                for (var l = 0; l < ll.Count; l++)
                {
                    Mapzen.Add(ll[l].str);
                }
            };
        }

        public ViewState Clone()
        {
            return new ViewState()
            {
                Name = Name,
                Lat = Lat,
                Lon = Lon,
                Zoom = Zoom,
                Scale = Scale,
                Range = Range,
                TileSize = TileSize,
                Layers = Layers,
                TileLayers = TileLayers,
                Mapzen = Mapzen
            };
        }

        public bool Equal(float lat, float lon, int zoom, int range) {
            return Zoom == zoom && Lat == lat && Lon == lon && Range == range;
        }

        public void SetView(float lat, float lon, int zoom, int range)
        {
            Lat = lat;
            Lon = lon;
            Zoom = zoom;
            Range = range;
        }

        /// <summary>
        /// Resolution in meters per tile
        /// </summary>
        /// <source>http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale</source>
        /// <returns></returns>
        public double Resolution
        {
            get
            {
                return TileSize * 156543.03 * Math.Cos(Lat) / (2 ^ Zoom);
            }
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
