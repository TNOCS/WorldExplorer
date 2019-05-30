using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorldExplorerServer.Config;
using WorldExplorerServer.Services;

namespace WorldExplorerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RasterTileController : ControllerBase
    {
        private readonly IConfigurationService mConfig;
        private readonly ILogger mLogger;

        public RasterTileController(IConfigurationService pConfig,
            ILogger<ConfigurationController> pLogger)
        {
            mConfig = pConfig;

        }

        private string GetTileFile(string pLayerId, int pZoom, int pX, int pY)
        {
            return Path.Combine(new string[] { mConfig.DataFolder, "raster", pLayerId, "tiles", pZoom.ToString(), pX.ToString(), pY.ToString()+".png"});
        }
        // TMS tiling system 
        [HttpGet("{pLayerName}/{pZoom}/{pX}/{pY}")]
        public async Task<IActionResult> Get(string pLayerName, int pZoom, int pX, int pY)
        {
            RasterLayer layer = mConfig
                .ConfigFile
                .RasterLayers?
                .FirstOrDefault(x => string.Equals(pLayerName, x.LayerId, StringComparison.OrdinalIgnoreCase));
            if (layer == null) return await CreateEmptyTile($"Layer '{pLayerName}' not in config", 256);
             var tileFilename = GetTileFile(pLayerName, pZoom, pX, pY); // Is tile local?
            if (!System.IO.File.Exists(tileFilename))
                await DownloadRasterTile(layer, pZoom, pX, pY);
            if (System.IO.File.Exists(tileFilename))
            {
                return File(System.IO.File.OpenRead(tileFilename), "application/octet-stream");
            }
            return NotFound();
        }

        private async Task<IActionResult> CreateEmptyTile(string pText, int pTileSize )
        {
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            Bitmap bitmap = new Bitmap(pTileSize, pTileSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.LightGray, new Rectangle(0, 0, pTileSize, pTileSize));
            graphics.DrawString(pText, new Font(FontFamily.GenericSerif, 8), Brushes.Black, new RectangleF(0, 0, pTileSize, pTileSize), sf);
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                //result.Content = new ByteArrayContent(ms.ToArray());
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return File(ms.ToArray(), "application/octet-stream");
            }
        }

        private async Task<bool> DownloadRasterTile(RasterLayer pLayer, int pZoom, int pX, int pY)
        {
            var urlTemplate = pLayer.Url ?? "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
            var url = urlTemplate
                .Replace("{z}", pZoom.ToString())
                .Replace("{x}", pX.ToString())
                .Replace("{y}", pY.ToString());

            try
            {
                var tileFilename = GetTileFile(pLayer.LayerId, pZoom, pX, pY);
                var tilePath = Path.GetDirectoryName(tileFilename);
                if (!Directory.Exists(tilePath)) Directory.CreateDirectory(tilePath);
                WebClient client = new WebClient();
                byte[] rawData = await client.DownloadDataTaskAsync(url);
                System.IO.File.WriteAllBytes(tileFilename, rawData);
                return true;
            }
            catch (Exception ex)
            { 
                mLogger.LogError(ex, $"Failed to cache for url '{url}'.");
                return false;
            }
        }
    }
}
