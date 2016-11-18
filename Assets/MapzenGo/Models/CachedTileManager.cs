using System.IO;
using MapzenGo.Helpers;
using UniRx;
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
                ObservableWWW.Get(url).Subscribe(
                    success =>
                    {
                        using (var sr = new StreamWriter(new FileStream(tilePath, FileMode.Create)))
                        {
                            sr.Write(success);
                        }
                        ConstructTile(success, tile);
                    },
                    error =>
                    {
                        Debug.Log(error);
                    });
            }
        }
    }
}