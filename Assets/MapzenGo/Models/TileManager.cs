﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using MapzenGo.Models.Factories;
using MapzenGo.Models.Plugins;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Classes;
using System;
using System.Threading.Tasks;
using System.Net;

namespace MapzenGo.Models
{
    public class TileManager : MonoBehaviour
    {
        [SerializeField] public float Latitude = 39.921864f;
        [SerializeField] public float Longitude = 32.818442f;
        [SerializeField] public int Range = 3;
        [SerializeField] public int Zoom = 16;
        [SerializeField] public float TileSize = 200;

        public string _mapzenUrl = "http://tile.mapzen.com/mapzen/vector/v1/{0}/{1}/{2}/{3}.{4}?api_key={5}";
        [SerializeField] public string _key = ""; //try getting your own key if this doesn't work
        protected string _mapzenLayers;
        [SerializeField] protected Material MapMaterial;
        protected readonly string _mapzenFormat = "json";
        protected Transform TileHost;

        private List<TilePlugin> _plugins;

        protected Dictionary<Vector2d, Tile> Tiles; //will use this later on
        protected Vector2d CenterTms; //tms tile coordinate
        protected Vector2d CenterInMercator; //this is like distance (meters) in mercator 

        public virtual void Start()
        {
            if (MapMaterial == null)
                MapMaterial = Resources.Load<Material>("Ground");

            InitFactories();
            InitLayers();

            if ((Latitude < -90.0) || (Latitude > 90.0) || (Longitude < -180.0) || (Longitude > 180.0))
            {
                Debug.LogError($"Invalid lat/lon coordinate (Lat:{Latitude} Lon:{Longitude}");
            }

            var v2 = GM.LatLonToMeters(Latitude, Longitude);
            var tile = GM.MetersToTile(v2, Zoom);

            TileHost = new GameObject("Tiles").transform;
            TileHost.SetParent(transform, false);

            Tiles = new Dictionary<Vector2d, Tile>();
            CenterTms = tile;
            CenterInMercator = GM.TileBounds(CenterTms, Zoom).Center;

            LoadTiles(CenterTms, CenterInMercator);

            var rect = GM.TileBounds(CenterTms, Zoom);
            transform.localScale = Vector3.one * (float)(TileSize / rect.Width);
        }

        public void UpdateView(double lat, double lon)
        {
            var v2 = GM.LatLonToMeters(lat, lon);
            var tile = GM.MetersToTile(v2, Zoom);

            CenterTms = tile;
            CenterInMercator = GM.TileBounds(CenterTms, Zoom).Center;

            LoadTiles(CenterTms, CenterInMercator);
            Debug.Log(Zoom);
            var rect = GM.TileBounds(CenterTms, Zoom);
            transform.localScale = Vector3.one * (float)(TileSize / rect.Width);
        }

        public virtual void Update()
        {
            // Do not delete (overridden).

        }

        private void InitLayers()
        {
            var layers = new List<string>();
            foreach (var plugin in _plugins.OfType<Factory>())
            {
                if (layers.Contains(plugin.XmlTag)) continue;
                layers.Add(plugin.XmlTag);
            }
            _mapzenLayers = string.Join(",", layers.ToArray());
        }

        private void InitFactories()
        {
            _plugins = new List<TilePlugin>();
            foreach (var plugin in GetComponentsInChildren<TilePlugin>())
            {
                _plugins.Add(plugin);
            }
        }

        protected void LoadTiles(Vector2d tms, Vector2d center)
        {
            for (int i = -Range; i <= Range; i++)
            {
                for (int j = -Range; j <= Range; j++)
                {
                    var v = new Vector2d(tms.x + i, tms.y + j);
                    if (Tiles.ContainsKey(v))
                        continue;
                    StartCoroutine(CreateTile(v, center));
                }
            }
        }

        protected virtual IEnumerator CreateTile(Vector2d tileTms, Vector2d centerInMercator)
        {

            var rect = GM.TileBounds(tileTms, Zoom);
            var tile = new GameObject("tile " + tileTms.x + "-" + tileTms.y).AddComponent<Tile>();
            tile.Zoom = Zoom;
            tile.TileTms = tileTms;
            tile.TileCenter = rect.Center;
            tile.Material = MapMaterial;
            tile.Rect = GM.TileBounds(tileTms, Zoom);
            Tiles.Add(tileTms, tile);
            tile.transform.position = (rect.Center - centerInMercator).ToVector3();
            tile.transform.SetParent(TileHost, false);
          

            LoadTile(tileTms, tile);
            yield return null;
        }

        protected virtual void LoadTile(Vector2d tileTms, Tile tile)
        {
            _plugins.ForEach(plugin => plugin.TileCreated(tile));
            var url = string.Format(_mapzenUrl, _mapzenLayers, Zoom, tileTms.x, tileTms.y, _mapzenFormat, _key);
            Task.Factory.StartNew<string>(() =>
            {
                WebClient wc = new WebClient();
                return wc.DownloadString(url);

            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    // faulted with exception
                    Exception ex = t.Exception;
                    while (ex is AggregateException && ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.LogError(ex.Message);
                }
                else if (t.IsCanceled)
                {

                }
                else
                {
                    ConstructTile(t.Result, tile);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        protected void ConstructTile(string json, Tile tile)
        {
            Task.Factory.StartNew<JSONObject>(() =>
            {
                return new JSONObject(json);
            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    // faulted with exception
                    Exception ex = t.Exception;
                    while (ex is AggregateException && ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.LogError(ex.Message);
                }
                else if (t.IsCanceled)
                {

                }
                else
                {
                    if (!tile) // checks if tile still exists and haven't destroyed yet
                        return;
                    tile.Data = t.Result;
                    _plugins.ForEach(plugin => plugin.GeoJsonDataLoaded(tile));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }
    }
}
