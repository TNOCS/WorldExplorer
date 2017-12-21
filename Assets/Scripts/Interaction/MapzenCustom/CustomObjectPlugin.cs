using Assets.Scripts;
using MapzenGo.Helpers;
using System;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    public class CustomObjectPlugin : Plugin
    {
        public bool hasVMGObjects = false;

        public override void Create(Tile tile)
        {
            base.Create(tile);

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

            if (hasVMGObjects)
            {
                var zoom = tile.Zoom;
                var x = tile.TileTms.x;
                var y = tile.TileTms.y;

                var url = "http://" + AppState.Instance.Config.ObjectServer + "/" + zoom + "/" + x + "/" + y + ".geojson";
                //var url = "http://" + AppState.Instance.Config.ObjectServer + "/" + newZoom + "/" + Math.Round(x, 0) + "/" + Math.Round(y, 0) + ".geojson";
                //var url = "http://www.thomvdm.com/testJSONobjects.json";
                //var url = "http://134.221.20.240:8777/vogd_01_building_a_wgs84.min/14/8723/5389.geojson";
                StartCoroutine(VMGObjectsFactory.Instance.GetJSON(url, tile));
            }
        }
    }
}