using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorldExplorerServer.Config;

namespace WorldExplorerServer.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IHostingEnvironment mHostingEnvironment;
        private readonly IConfiguration mConfigFiles;
        public ConfigurationService(
            IHostingEnvironment pEnvironment,
            IConfiguration pAspDotNetConfig)
        {
            mConfigFiles = pAspDotNetConfig;
            mHostingEnvironment = pEnvironment;
            DataFolder = Path.Combine(mHostingEnvironment.ContentRootPath, "Data");
            QuantizedMeshFolder = Path.Combine(DataFolder, "QuantizedMesh","tiles");
            ConfigFile = pAspDotNetConfig.GetSection("WorldExplorer").Get<WorldExplorerCfg>();
            // https://tile.nextzen.org/tilezen/vector/v1/all/{1}/{2}/{3}.json?api_key=_Q7ntvdRSF6OxwTb9Sw2pw
            OpenMaptilesUrl = pAspDotNetConfig.GetValue<string>("WorldExplorer:MapzenUrl", "http://localhost:8080/data/v3/{zoom}/{x}/{y}.geojson");
            //Cfg.RasterLayers = pAspDotNetConfig.GetSection("WorldExplorer:RasterLayers").Get<RasterLayer[]>(x => x.BindNonPublicProperties = true);
        }

        public WorldExplorerCfg ConfigFile { get; private set; }
        public string QuantizedMeshFolder { get; private set; }
        public string DataFolder { get; private set; }
        public string OpenMaptilesUrl { get; private set; }
    }
}
