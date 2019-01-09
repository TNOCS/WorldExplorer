using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace MapzenGo.Models
{
    public class CachedTileManager : DynamicTileManager
    {
        private const string RelativeCachePath = "/{0}/";
        protected string CacheFolderPath;

        public override void Start()
        {
            CacheFolderPath = string.Format(Application.temporaryCachePath + RelativeCachePath, Zoom);
            if (!Directory.Exists(CacheFolderPath)) Directory.CreateDirectory(CacheFolderPath);
            base.Start();
        }

        public void ClearCache()
        {
            if (Directory.Exists(CacheFolderPath)) Directory.Delete(CacheFolderPath, true);
        }

        protected override void LoadTile(Vector2d tileTms, Tile tile)
        {
            var tilePath = Path.Combine(CacheFolderPath, _mapzenLayers.Replace(',', '_') + "_" + tileTms.x + "_" + tileTms.y) + ".json";
            //Debug.Log(tilePath);
            if (File.Exists(tilePath))
            {
                using (var r = new StreamReader(new FileStream(tilePath, FileMode.Open)))
                {
                    var mapData = r.ReadToEnd();
                    ConstructTile(mapData, tile);
                }
            }
            else
            {
                var url = string.Format(_mapzenUrl, _mapzenLayers, Zoom, tileTms.x, tileTms.y, _mapzenFormat, _key);
                Task.Factory.StartNew<byte[]>(() =>
                {
                    WebClient wc = new WebClient();
                    byte[] data = wc.DownloadData(url);
                    return data;
                }).ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        // faulted with exception
                        Exception ex = t.Exception;
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        
                    }
                    else if (t.IsCanceled)
                    {
                        
                    }
                    else
                    {
                        string text = System.Text.Encoding.Default.GetString(t.Result);
                        // completed successfully
                        using (var sr = new StreamWriter(new FileStream(tilePath, FileMode.Create)))
                        {
                            sr.Write(t.Result);
                        }
                        ConstructTile(text, tile);
                        
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
        }

        
    }
}