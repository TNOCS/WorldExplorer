using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WorldExplorerServer.Services;
using WorldExplorerServer.Util;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WorldExplorerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableCors]
    public class VectorTileController : ControllerBase
    {
        // https://tile.nextzen.org/tilezen/vector/v1/all/{1}/{2}/{3}.json?api_key=_Q7ntvdRSF6OxwTb9Sw2pw
        private readonly IConfigurationService mConfig;
        private readonly ILogger mLogger;

        private readonly Dictionary<string, string> mRenameTable = new Dictionary<string, string>()
        {
            {"building", "buildings" },
            {"boundary", "boundries" },
            {"place", "places" },
            {"road", "roads" },
            {"water", "water" },
            {"pois", "poi" },
            {"landuse", "landuse" },
        };

        private readonly string[] mAllLayers = new string[] { "buildings", "roads", "water" };

        public VectorTileController(IConfigurationService pConfig,
            ILogger<ConfigurationController> pLogger)
        {
            mConfig = pConfig;

        }

        private string GetTilePath(int pZoom, int pX, int pY)
        {
           return Path.Combine(new string[] { mConfig.DataFolder, "mapzen", "tiles", pZoom.ToString(), pX.ToString(), pY.ToString() });
        }

        [HttpGet("{pLayers}/{pZoom}/{pX}/{pY}.json")]
        public async Task<ContentResult> Get(string pLayers, int pZoom, int pX, int pY)
        {
            // What layers to return? For 'all' return fixed set
            string[] layers = pLayers.Equals("all", StringComparison.InvariantCultureIgnoreCase) ? mAllLayers : pLayers.Split(',');
            var tilePath = GetTilePath(pZoom, pX, pY);
            if (!Directory.Exists(tilePath) || (Directory.GetFiles(tilePath).Length == 0))
                await DownloadOpenTilesTileGeoJson(pZoom, pX, pY);
            if (Directory.Exists(tilePath))
            {
                // Compose all json files into one 
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                bool first = true;
                foreach (var layer in layers)
                {
                    
                    string jsonFile = Path.Combine(tilePath, $"{layer}.json");
                    if (System.IO.File.Exists(jsonFile))
                    {
                        FileStream fileStream = new FileStream(jsonFile, FileMode.Open);
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            string json = await reader.ReadToEndAsync();
                            if (!first) sb.Append(",");
                            first = false;
                            sb.Append($"\"{layer}\": {json}");
                        }
                    }
                }
                sb.Append("}");
                return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
            }
            return new ContentResult
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        /* Download GeoJSON from other source and cache the result */
        private async Task<bool> DownloadOpenTilesTileGeoJson(int pZoom, int pX, int pY)
        {
            try
            {
                var tilePath = GetTilePath(pZoom, pX, pY);
                if (!Directory.Exists(tilePath)) Directory.CreateDirectory(tilePath);
                WebClient client = new WebClient();
                var url = mConfig.OpenMaptilesUrl
                    .Replace("{zoom}", pZoom.ToString())
                    .Replace("{x}", pX.ToString())
                    .Replace("{y}", pY.ToString());
                byte[] rawData = await client.DownloadDataTaskAsync(url);
                var encoding = WebUtil.GetEncodingFrom(client.ResponseHeaders, Encoding.UTF8);
                string json = encoding.GetString(WebUtil.DecodeGzip(rawData));
                System.IO.File.WriteAllText(Path.Combine(tilePath, $"all.json"), json);
                var collection = JsonConvert.DeserializeObject<FeatureCollection>(json);
                // Group by feature property 'layer' and write to single json file per layer
                var layerNames = collection.Features.Where(x => x.Properties.ContainsKey("layer")).GroupBy(y => y.Properties["layer"]).Select(z => (string)z.FirstOrDefault().Properties["layer"]);

                
                foreach (var layerName in layerNames)
                {
                    var layerFeatures = collection.Features.Where(x => ((x.Properties.ContainsKey("layer")) && (layerName.Equals((string)x.Properties["layer"])))).ToList();
                    var jsonSection = JsonConvert.SerializeObject(new FeatureCollection(layerFeatures));
                    string name = mRenameTable.ContainsKey(layerName) ? mRenameTable[layerName] : layerName;
                    System.IO.File.WriteAllText(Path.Combine(tilePath, $"{name}.json"), jsonSection);
                }
                return true;
            } catch(Exception ex)
            {
                mLogger.LogError(ex, $"Failed to cache GeoJSON for url '{Url}'.");
                return false;
            }
        }
    }
}