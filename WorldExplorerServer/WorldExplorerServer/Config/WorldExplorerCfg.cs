using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldExplorerServer.Config
{
    public class WorldExplorerCfg 
    {
        public string Name { get; set; }
        public  IEnumerable<RasterLayer> RasterLayers  { get; set; }
    }
}
