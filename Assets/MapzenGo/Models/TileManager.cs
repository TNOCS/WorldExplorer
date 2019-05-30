using MapzenGo.Helpers;
using MapzenGo.Models.Factories;
using MapzenGo.Models.Plugins;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MapzenGo.Models
{
    // GeoJSON tiles
    public class TileManager : MonoBehaviour
    {
        [SerializeField] public float Latitude = 39.921864f;
        [SerializeField] public float Longitude = 32.818442f;
        [SerializeField] public int Range = 3;
        [SerializeField] public int Zoom = 16;
        [SerializeField] public float TileSize = 200;

        public string BaseUrl; // The Url to the WorldExplorer sever (vector data: {baseurl}/api/VectorTile/{layers}/{zoom}/{x}/{y}.json)

        [SerializeField] public string _key = ""; // Not used anymore
        protected string _mapzenLayers;
        [SerializeField] protected Material MapMaterial;
        //protected readonly string _mapzenFormat = "json";
        protected Transform TileHost;

        private ConcurrentDictionary<string, Mesh> HeightMeshes = new ConcurrentDictionary<string, Mesh>();

        private List<TilePlugin> _plugins;

        protected Dictionary<Vector2d, Tile> Tiles; //will use this later on
        protected Vector2d CenterTms; //tms tile coordinate
        protected Vector2d CenterInMercator; //this is like distance (meters) in mercator 

        public CountdownEvent allHeightMeshesLoad;
        private int batchCount = 0;
        private List<Tuple<Vector2d, Tile>> jsonTiles = new List<Tuple<Vector2d, Tile>>();

        public virtual void Start()
        {
            if (MapMaterial == null)
            {
                MapMaterial = Resources.Load<Material>("Ground");
            }

            
            _plugins = new List<TilePlugin>();
            foreach (var plugin in GetComponentsInChildren<TilePlugin>()) _plugins.Add(plugin);
            _mapzenLayers = String.Join(",", _plugins.OfType<Factory>().Select(x => x.XmlTag).Distinct().ToArray());
            Debug.Log($"{_plugins.Count} geojson factories (mapzen layers: {_mapzenLayers})");

            if ((Latitude < -90.0) || (Latitude > 90.0) || (Longitude < -180.0) || (Longitude > 180.0))
            {
                Debug.LogError($"Invalid lat/lon coordinate (Lat:{Latitude} Lon:{Longitude})");
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

        protected string CreateMapzenUrl(Vector2d pTile)
        {
            
            
            var url = BaseUrl ?? "http://localhost";
            url = url.TrimEnd(new char[] { '/' }); // Remove ending /
           
            // _mapzenUrlFormat = "https://tile.nextzen.org/tilezen/vector/v1/all/{zoom}/{x}/{y}.json?api_key=_Q7ntvdRSF6OxwTb9Sw2pw";
            //  _mapzenUrlFormat = "http://localhost:8080/data/v3/{zoom}/{x}/{y}.geojson";
            //     https://tile.nextzen.org/tilezen/vector/v1/all/0/0/0.json?api_key=_Q7ntvdRSF6OxwTb9Sw2pw
           
            return "{baseurl}/api/VectorTile/{layers}/{zoom}/{x}/{y}.json"
                .Replace("{baseurl}", url)
                .Replace("{x}", Convert.ToString(pTile.x, CultureInfo.InvariantCulture))
                .Replace("{y}", Convert.ToString(pTile.y, CultureInfo.InvariantCulture))
                .Replace("{zoom}", Convert.ToString(Zoom, CultureInfo.InvariantCulture))
                .Replace("{layers}", _mapzenLayers ?? "");

        }

        /*
         * Download all GeoJSON tiles
         * Rendering of GeoJSON can start heightmesh is created (WaitForHeightMeshesLoaded) 
         */
        protected void LoadTiles(Vector2d tms, Vector2d center)
        {
            
            batchCount++;
            int numberOfTiles = ((Range * 2) + 1) * ((Range * 2) + 1);
            allHeightMeshesLoad = new CountdownEvent(numberOfTiles);
            WaitForHeightMeshesLoaded();
            Debug.Log($"Start downloading {numberOfTiles} GeoJson tiles for center[{Latitude} {Longitude}] with tile range {Range}.");
            for (int i = -Range; i <= Range; i++)
            {
                for (int j = -Range; j <= Range; j++)
                {
                    var v = new Vector2d(tms.x + i, tms.y + j);
                    if (Tiles.ContainsKey(v))
                    {
                        allHeightMeshesLoad.Signal();
                        continue;
                    }
                    // {Thread.CurrentThread.ManagedThreadId}
                    StartCoroutine(CreateTile(v, center));
                }
            }

        }
        // Raycast is needed for terrainheight (json has no height data)
        // Some of the vector data is rendered a little bit outside the tile; so wait for all tiles.
        private void WaitForHeightMeshesLoaded()
        {
            Task.Factory.StartNew<bool>(() =>
            {
                this.allHeightMeshesLoad.Wait(); // Block thread until it recieves the signal all height mesh tiles are loaded.
                return true;
            }).ContinueWith((t) =>
            {
                Debug.Log($"Terrein DEM meshes loaded, raycast possible, start download GeoJSON files ({jsonTiles.Count} tiles).");
                foreach (var x in jsonTiles)
                {
                    StartDownloadJsonVectorDataForTile(x.Item1, x.Item2);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected virtual IEnumerator CreateTile(Vector2d tileTms, Vector2d centerInMercator)
        {
            var rect = GM.TileBounds(tileTms, Zoom);
            var go = new GameObject("tile " + tileTms.x + "-" + tileTms.y);
            var tile = go.AddComponent<Tile>();

            tile.Zoom = Zoom;
            tile.TileTms = tileTms;
            tile.TileCenter = rect.Center;
            tile.Material = MapMaterial;
            tile.Rect = GM.TileBounds(tileTms, Zoom);
            Tiles.Add(tileTms, tile);
            tile.transform.position = (rect.Center - centerInMercator).ToVector3();
            tile.transform.SetParent(TileHost, false);
            //go.AddComponent<HeightRaycast>();
            LoadTile(tileTms, tile);
            yield return null;
        }

        protected virtual void LoadTile(Vector2d tileTms, Tile tile)
        {

            // Wait with download geojson till terrain height mesh is loaded
            jsonTiles.Add(Tuple.Create(tileTms, tile));

            // Notify all plugin there is a new tile
            _plugins.ForEach(plugin => plugin.TileCreated(tile));

            // Some plugin will wait for JSON data (plugin.GeoJsonDataLoaded)




        }



        /// <summary>
        /// Download (mapzen) JSON vector data for tile
        /// </summary>
        /// <param name="tileTms"></param>
        /// <param name="tile"></param>
        private async void StartDownloadJsonVectorDataForTile(Vector2d tileTms, Tile tile)
        {
            var url = CreateMapzenUrl(tileTms);
            try
            {
                // Load JSON vector data for tile:
                var json = await NetworkUtil.DownloadStringAsync(CancellationToken.None, url);
                await ConstructTile(json, tile);
            } catch(Exception ex)
            {
                Debug.LogError($"TileManager: Failed to download url '{url}'; error: {ex.Message}");
            }
           


        }

        protected async Task ConstructTile(string json, Tile tile)
        {
            Debug.Log($"Convert GeoJSON for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} to object.");
            // The JSON vector data is shared across multiple plugins
            JSONObject jsonObject = await Task.Run( () => {
                Debug.Log($"Load GeoJSON for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} in factories (thread id {Thread.CurrentThread.ManagedThreadId}).");
                return new JSONObject(json);
                });
           
           
            if (!tile) // checks if tile still exists and haven't destroyed yet
            {
                return;
            }
            Debug.Log($"Process GeoJSON for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} in factories (thread id {Thread.CurrentThread.ManagedThreadId}).");
            tile.Data = jsonObject;
            foreach (var factory in _plugins)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => factory.GeoJsonDataLoaded(tile));
            }
            Debug.Log($"Finished Process GeoJSON for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} in factories (thread id {Thread.CurrentThread.ManagedThreadId}).");
        }

        public void HeightTileLoaded(Tile tile)
        {
            //Debug.Log($"Heightmesh loaded for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y}");
            allHeightMeshesLoad.Signal();
        }

        public void CacheMesh(string pTileNumber, Mesh pHeightMesh)
        {
            HeightMeshes.AddOrUpdate(pTileNumber, pHeightMesh, (key, oldValue) => pHeightMesh);
        }

        public Mesh GetHeightMeshFromCache(string pTileNumber)
        {
            Mesh result = null;
            return HeightMeshes.TryGetValue(pTileNumber, out result) ? result : null;
        }
    }
}
