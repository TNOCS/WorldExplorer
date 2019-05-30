using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorldExplorerServer.Services;

namespace WorldExplorerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuantizedMeshController : ControllerBase
    {
        private readonly IConfigurationService mConfig;

        public QuantizedMeshController(IConfigurationService pConfig)
        {
            mConfig = pConfig;
        }

        [HttpGet("{pZoom}/{pX}/{pY}.terrain")]
        public IActionResult Get(int pZoom, int pX, int pY)
        {
            string zoom = pZoom.ToString();
            string x = pX.ToString();
            string y = ((int)(Math.Pow(2, pZoom) - pY - 1)).ToString();  // Slippy to TMS.
            string filename = Path.Combine(mConfig.QuantizedMeshFolder, zoom, x, (y + ".terrain"));

            if (System.IO.File.Exists(filename))
                return File(System.IO.File.OpenRead(filename), "application/octet-stream");
            return NotFound();

            
        }
    }
}