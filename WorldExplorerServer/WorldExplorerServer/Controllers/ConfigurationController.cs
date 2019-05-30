using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorldExplorerServer.Logging;
using WorldExplorerServer.Services;

namespace WorldExplorerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService mConfig;
        private readonly ILogger mLogger;

        public ConfigurationController(
            IConfigurationService pConfig, 
            ILogger<ConfigurationController> pLogger,
            IConfiguration pAspDotNetConfig)
        {
            string name = pAspDotNetConfig.GetValue<string>("ServerName", "");
            string name1 = pAspDotNetConfig.GetValue<string>("section0:Key1", "");
            mConfig = pConfig;
            mLogger = pLogger;
        }

        private string GetConfigPath()
        {
            return Path.Combine(new string[] { mConfig.DataFolder, "Configuration" });
        }

        public string GetFileName(string pConfigname)
        {
            return Path.Combine(GetConfigPath(), pConfigname);
        }

        [HttpGet("Url")]
        public JsonResult GetUrl()
        {

            //var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            //var localIpAddress = httpConnectionFeature?.LocalIpAddress;


            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
                IPAddress ipAddress = ipHostInfo.AddressList[0];


            return new JsonResult("Server ip-addresses: "+ String.Join("   ", ipHostInfo.AddressList.Select(ip => ip.ToString())));
        }

        [HttpGet("{pConfigurationName}.json")]
        public ContentResult Get(string pConfigurationName)
        {
            mLogger.LogInformation(LoggingEvents.Configuration, "Request log configuration {pConfigurationName}");
            string cfgFile  = GetFileName(pConfigurationName + ".json");
            if ((System.IO.File.Exists(cfgFile)))
            {
                return new ContentResult { Content = System.IO.File.ReadAllText(cfgFile), ContentType = "application/json" };
            }
            else
            {
                mLogger.LogError(LoggingEvents.Configuration, "Configuration '{cfgFile}' not found, return 404");
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }


        }
    }
}