using Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.ComponentModel;

namespace Assets.Scripts.Classes
{
    [DisplayName("GeoJson: {Title})")]
    public class GeoJsonLayer : Layer
    {

        public GeoJsonLayer()
        {

        }

        public GeoJsonLayer(JSONObject json)
        {
            FromJson(json);
        }

        public Timer _refreshTimer { get; set; }

        public override void FromJson(JSONObject json)
        {
            base.FromJson(json);
            IconUrl = json.GetString("iconUrl");
            Scale = json.GetInt("scale", 30);
            if (json.HasField("refresh")) Refresh = json.GetInt("refresh");
        }

        public void InitGeojsonLayer()
        {
            AddGeojsonLayer();
            if (Refresh > 0)
            {
                var interval = Refresh * 1000;
                _refreshTimer = new System.Threading.Timer(RefreshLayer, this, interval, interval);
            }
        }

        public string GeoJsonUrl()
        {
            Uri baseUri = new Uri(AppState.Instance.BaseUrlConfigurationServer);
            Uri uri = new Uri(baseUri, $"api/GeoJson/{LayerId}");
            return uri.ToString();
        }

        public async void AddGeojsonLayer()
        {
            if (_active) return;
            try
            {
                
                string geojson = await NetworkUtil.DownloadStringAsync(CancellationToken.None, GeoJsonUrl());

                var layerObject = new GameObject(LayerId);

                layerObject.transform.SetParent(AppState.Instance.Layers.transform, false);
                _object = layerObject;
                _active = true;

                var symbolFactory = layerObject.AddComponent<SymbolFactory>();
                var av = AppState.Instance.Config.ActiveView;
                symbolFactory.InitLayer(this, geojson, av.Zoom, av.Lat, av.Lon, av.TileSize, av.Range);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load geoJSON for url '{ GeoJsonUrl()}'; error: {ex.Message}");
            }


        }

        public void RemoveGeojsonLayer()
        {
           AppState.Instance.DoDeleteAll(_object);
            _active = false;
        }

        public void DestroyGeojsonLayer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
            if (_active) RemoveGeojsonLayer();
        }

        public void RefreshLayer(object d)
        {
            var l = (Layer)d;

            if (_active)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RemoveGeojsonLayer();
                    AddGeojsonLayer();
                });
            }
        }

        /// <summary>
        /// Url of the image being used
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Scale of the icon
        /// </summary>
        public int Scale { get; set; }


        /// <summary>
        /// Interval in seconds to refresh layer
        /// </summary>
        public int Refresh { get; set; }

        public GeoJson GeoJSON { get; set; }

        public string ToJSON()
        {
            var features = new StringBuilder();
            GeoJSON.features.ForEach(f => {
                features.Append(f.ToJSON());
                features.Append(",");
            });
            return string.Format(@"{{ ""type"": ""FeatureCollection"", ""features"": [{0}] }}", features.ToString().TrimEnd(new[] { ',' }));
        }
    }
}
