using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    public class ViewState
    {
        public ViewState()
        {

        }

        public ViewState(List<RasterLayer> pRasterLayers, List<GeoJsonLayer> pGeoJsonLayers, JSONObject pJson)
        {
            FromJson(pRasterLayers, pGeoJsonLayers, pJson);

        }

        public void FromJson(List<RasterLayer> pRasterLayers, List<GeoJsonLayer> pGeoJsonLayers, JSONObject json)
        {
            Name = json.GetString("name");
            Lat = json.GetFloat("lat");
            Lon = json.GetFloat("lon");
            Zoom = json.GetInt("zoom");
            Scale = json.GetInt("scale");
            Range = json.GetInt("range");
            TileSize = json.GetInt("tileSize");
            
            TerrainHeightsAvailable = json.GetBoolean("terrainHeightAvailable");
            GeoJsonLayers = new List<GeoJsonLayer>();
            if (json.HasField("geoJsonLayers"))
            {
                var ll = json["geoJsonLayers"];
                for (var l = 0; l < ll.Count; l++)
                {
                   
                    var layer = pGeoJsonLayers.Find(x => x.LayerId == ll[l].str);
                    if (layer != null)
                    {
                        GeoJsonLayers.Add(layer);
                    }
                    else
                    {
                        Debug.Log($"GeoJson layer name {ll[l].str} not found in config.");
                    }
                }
            };

            RasterTileLayers = new List<RasterLayer>();
            if (json.HasField("rasterLayers"))
            {
                var ll = json["rasterLayers"];
                for (var l = 0; l < ll.Count; l++)
                {
                    var layer = pRasterLayers.Find(x => x.LayerId == ll[l].str);
                    if (layer != null)
                    {
                        RasterTileLayers.Add(layer);
                    } else
                    {
                        Debug.Log($"Failed to lookup raster layer name '{ll[l].str}' in view {Name}.");
                    }
                    
                }
            };
            if (RasterTileLayers.Count == 0)
            {
                Debug.LogError($"View {Name} has zero raster tile layers " );
            }

            Mapzen = new List<string>();
            if (json.HasField("mapzen"))
            {
                var ll = json["mapzen"];
                for (var l = 0; l < ll.Count; l++)
                {
                    Mapzen.Add(ll[l].str);
                }
            };

            if ((Lat < -90.0) || (Lat > 90.0) || (Lon < -180.0) || (Lon > 180.0))
            {
                Debug.LogError($"Invalid lat/lon coordinate in config file (Lat:{Lat} Lon:{Lon})");
            }
            if ((Zoom < 0) || (Zoom > 19))
            {
                Debug.LogError($"Invalid zoom level {Zoom} in config file (must be in range [0..19])");
            }
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
                GeoJsonLayers = GeoJsonLayers,
                RasterTileLayers = RasterTileLayers,
                TerrainHeightsAvailable = TerrainHeightsAvailable,
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
                // 256 is the TileSize in GM
                return 256 * 156543.03 * Math.Cos(Lat * Math.PI/180) / (Math.Pow(2, Zoom));
            }
        }

        /// <summary>
        /// This is a simplified representation of the view state, ignoring things like layers, name etc.
        /// It is intended for synchronization of view state between different session users.
        /// </summary>
        /// <returns></returns>
        public string ToLimitedJSON()
        {
            return string.Format(@"{{ ""lat"": {0}, ""lon"": {1}, ""zoom"": {2}, ""range"": {3} }}", Lat, Lon, Zoom, Range);
        }

        public override string ToString()
        {
            return string.Format(@"Name: {0}, lat: {1}, lon: {2}, zoom: {3}, range: {4}", Name, Lat, Lon, Zoom, Range);
        }

        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public int Zoom { get; set; }
        public int Scale { get; set; }
        public int Range { get; set; }
        public int TileSize { get; set; }
        public bool TerrainHeightsAvailable { get; set; }
        public List<GeoJsonLayer> GeoJsonLayers { get; set; }
        public List<RasterLayer> RasterTileLayers { get; set; }
        public List<string> Mapzen { get; set; }
    }
}
