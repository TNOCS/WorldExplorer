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
        }

        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public int Zoom { get; set; }
        public int Scale { get; set; }
        public int Range { get; set; }
        public int TileSize { get; set; }

    }

}
