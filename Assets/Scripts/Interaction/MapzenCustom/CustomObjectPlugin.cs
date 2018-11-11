using Assets.Scripts;
using MapzenGo.Helpers;
using System;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    /// <summary>
    /// Spawns placed objects and VMGObjects on each tile when they are initialized.
    /// </summary>

    public class CustomObjectPlugin : TilePlugin
    {
        public bool hasVMGObjects = false;

        public override void GeoJsonDataLoaded(Tile tile)
        {
            
        }

        public override void TileCreated(Tile tile)
        {
            

            foreach (var spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
            {
                var meters = GM.LatLonToMeters(spawnedObject.lat, spawnedObject.lon);
                var newlySpawned = GameObject.Find("NewlySpawned");
                if (tile.Rect.Contains(meters))
                {
                    spawnedObject.obj.SetActive(true);
                    // Repositions the objects at the right position through lat / lon after a map edit.
                    spawnedObject.obj.transform.position = (meters - tile.Rect.Center).ToVector3();

                    // First childs it under the corresponding tile to get the correct global scale and position.               
                    spawnedObject.obj.transform.SetParent(tile.transform, false);
                    var globalScale = spawnedObject.obj.transform.lossyScale;
                    var globalPosition = spawnedObject.obj.transform.position;

                    // Then places it under NewlySpawned (so we can keep the object when the tile is destroyed).
                    // Scale and position have to be set again to avoid rescaling or repositioning.
                    spawnedObject.obj.transform.SetParent(newlySpawned.transform, false);
                    spawnedObject.obj.transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
                    spawnedObject.obj.transform.position = globalPosition;
                }
            }

            // If the area has VMGObjects, and if the zoom level is 17 or higher (no need to load objects when zoomed out further as it would cause a lot of stress on the loading for not a lot of added value).
            if (hasVMGObjects)
            {
                var zoom = tile.Zoom;
                if (zoom >= 17)
                {
                    var x = tile.TileTms.x;
                    var y = tile.TileTms.y;
                    var url = "http://" + AppState.Instance.Config.ObjectServer + "/" + zoom + "/" + x + "/" + y + ".geojson";

                    StartCoroutine(VMGObjectsFactory.Instance.GetJSON(url, tile));
                }
            }
        }
    }
}