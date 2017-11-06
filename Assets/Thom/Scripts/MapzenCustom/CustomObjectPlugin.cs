using System;
using System.Collections.Generic;
using MapzenGo.Helpers;
using MapzenGo.Models;
using UnityEngine;
using Assets.Scripts;

namespace MapzenGo.Models.Plugins
{
    public class CustomObjectPlugin : Plugin
    {
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
                    // Scale and position have to be set again to stop rescaling or repositioning
                    spawnedObject.obj.transform.SetParent(newlySpawned.transform, false);
                    spawnedObject.obj.transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);

                    spawnedObject.obj.transform.position = globalPosition;

                }
            }
        }
    }
}