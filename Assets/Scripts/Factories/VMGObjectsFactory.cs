using Assets.Scripts;
using MapzenGo.Helpers;
using MapzenGo.Models;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class VMGObjectsFactory : SingletonCustom<VMGObjectsFactory>
{
    /// <summary>
    /// Retrieved JSON data from the provided URL in the config, and loops through it to spawn all objects on the map.
    /// </summary>

    private float vmgScaleFactor = 1000;

    public IEnumerator GetJSON(string url, Tile tile)
    {
        using (var www = UnityWebRequest.Get(url))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error + " (Error retrieving objects from " + url + ")");
            }
            else
            {
                var objectJSONString = www.downloadHandler.text;
                LoopThroughJSON(objectJSONString, tile);
            }
        }
    }

    private void LoopThroughJSON(string objectJSONString, Tile tile)
    {
        var json = JSON.Parse(objectJSONString);
        foreach (JSONNode o in json["features"].Children)
        {
            var id = o["properties"]["n"].Value;

            // Makes sure the object isn't already spawned. Based on ID.
            if (!InventoryObjectInteraction.Instance.spawnedObjectsList.Any(spawnedObject => spawnedObject.obj.name == id))
            {
                var objName = o["properties"]["m"].Value;
                // NOTE: JSON provides "lon / lat" instead of "lat / lon".
                var lon = o["geometry"]["coordinates"][0].AsFloat;
                var lat = o["geometry"]["coordinates"][1].AsFloat;

                VMGObject obj = new VMGObject(id, objName, lat, lon);
                SpawnObject(obj, tile);
            }
        }
    }

    private void SpawnObject(VMGObject vmgObject, Tile tile)
    {
        var newlySpawned = GameObject.Find("NewlySpawned");
        var meters = GM.LatLonToMeters(vmgObject.lat, vmgObject.lon);
        if (tile.Rect.Contains(meters))
        {
            // For demo purposes: pools different versions of models into one generic model. i.e. all lighting poles become the same.
            if (vmgObject.objName.Contains("LIGHTING_POLE"))
            {
                vmgObject.objName = "VOGD_M_LIGHTING_POLE_004";
            }
            if (vmgObject.objName.Contains("ANTENNA_MILITARY"))
            {
                vmgObject.objName = "VOGD_M_ANTENNA_MILITARY_005";
            }
            if (vmgObject.objName.Contains("SWITCHBOX"))
            {
                vmgObject.objName = "VOGD_M_ELECTRICITY_SWITCHBOX_003";
            }

            if (Resources.Load("Prefabs/VMG/" + vmgObject.objName) != null)
            {
                //Debug.Log("Spawning Object " + vmgObject.id + " which is a " + vmgObject.objName + " as its new");
                var newObject = Instantiate(Resources.Load("Prefabs/VMG/" + vmgObject.objName)) as GameObject;

                newObject.name = vmgObject.id;
                newObject.transform.position = (meters - tile.Rect.Center).ToVector3();

                // First childs it under the corresponding tile to get the correct global scale and position.               
                newObject.transform.SetParent(tile.transform, false);
                var globalScale = newObject.transform.lossyScale;
                var globalPosition = newObject.transform.position;

                // Then places it under NewlySpawned (so we can keep the object when the tile is destroyed).
                // Scale and position have to be set again to avoid rescaling or repositioning.
                newObject.transform.SetParent(newlySpawned.transform, false);
                newObject.transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
                newObject.transform.localScale = newObject.transform.localScale * InventoryObjectInteraction.Instance.spawnScaleFactors[tile.Zoom] * GameObject.Find("terrain").transform.localScale.x * vmgScaleFactor;
                newObject.transform.position = globalPosition;

                newObject.transform.tag = "spawnobject";
                SpawnedObject spawnedObject = new SpawnedObject(newObject, newObject.transform.TransformDirection(newObject.transform.position), vmgObject.lat, vmgObject.lon, newObject.transform.localScale, newObject.transform.rotation);
                InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
            }
        }
    }
}
