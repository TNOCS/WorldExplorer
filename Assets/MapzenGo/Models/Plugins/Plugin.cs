using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapzenGo.Models;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    public abstract class TilePlugin : MonoBehaviour
    {
        public abstract void TileCreated(Tile tile);

        public abstract void GeoJsonDataLoaded(Tile tile);
    }
}
