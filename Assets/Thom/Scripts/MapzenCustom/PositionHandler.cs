using MapzenGo.Helpers;
using MapzenGo.Models;
using MapzenGo.Models.Plugins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class PositionHandler : MonoBehaviour {

    public class CustomObjectPlugin : Plugin
    {
        private InventoryObjectInteraction inventoryObjectInteraction;

        void Start()
        {
            Debug.Log("awake poshandler");
            inventoryObjectInteraction = InventoryObjectInteraction.Instance;
        }

        public override void Create(Tile tile)
        {
            Debug.Log("Create");
            base.Create(tile);
            Debug.Log("Create");
            foreach (SpawnedObject go in inventoryObjectInteraction.spawnedObjectsList)
            {
                var meters = GM.LatLonToMeters(go.lat, go.lon);
                go.obj.transform.position = meters.ToVector3();
                var rect = tile.GetComponent<Tile>().Rect;
                if (rect.Contains(meters))
                {
                    go.obj.transform.position = (meters - rect.Center).ToVector3();
                }
            }
        }
    }
}
*/