using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorldExplorerServer.Services;

namespace WorldExplorerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeoJsonController : ControllerBase
    {

        private readonly IConfigurationService mConfig;
        private readonly ILogger mLogger;


        public GeoJsonController(IConfigurationService pConfig,
            ILogger<ConfigurationController> pLogger)
        {
            mConfig = pConfig;

        }
    }
}