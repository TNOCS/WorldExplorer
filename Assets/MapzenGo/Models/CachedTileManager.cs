using System.IO;
using System.Threading;
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
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            base.Start();
        }

        public void ClearCache()
        {
            if (Directory.Exists(CacheFolderPath))
            {
                Directory.Delete(CacheFolderPath, true);
            }
        }



        protected override void LoadTile(Vector2d tileTms, Tile tile)
        {
            LoadTileAsync(tileTms, tile);
        }

        private async void LoadTileAsync(Vector2d tileTms, Tile tile)
        {
            var tilePath = Path.Combine(CacheFolderPath, _mapzenLayers.Replace(',', '_') + "_" + tileTms.x + "_" + tileTms.y) + ".json";
            //Debug.Log(tilePath);
            if (File.Exists(tilePath))
            {
                using (var r = new StreamReader(new FileStream(tilePath, FileMode.Open)))
                {
                    var mapData = r.ReadToEnd();
                    await ConstructTile(mapData, tile);
                }
            }
            else
            {
                var url = CreateMapzenUrl(tileTms);
                var data = await NetworkUtil.DownloadDataAsync(CancellationToken.None, url);


                string text = System.Text.Encoding.Default.GetString(data);
                // completed successfully
                using (var sr = new StreamWriter(new FileStream(tilePath, FileMode.Create)))
                {
                    sr.Write(data);
                }
                await ConstructTile(text, tile);




            }
        }
    }
}