using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldExplorerServer.Config;

namespace WorldExplorerServer.Services
{
    public interface IConfigurationService
    {
        WorldExplorerCfg ConfigFile { get; }

        string DataFolder { get; }
        string QuantizedMeshFolder { get; }
        string OpenMaptilesUrl { get; }

    }
}
