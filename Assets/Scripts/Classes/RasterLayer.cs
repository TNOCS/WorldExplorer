using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Classes
{
    [DisplayName("Raster: {Title})")]
    public class RasterLayer : Layer
    {
        public RasterLayer()
        {

        }

        public RasterLayer(JSONObject json)
        {
            FromJson(json);
        }

        public override void FromJson(JSONObject json) 
        {
            base.FromJson(json);
        }
    }
}
