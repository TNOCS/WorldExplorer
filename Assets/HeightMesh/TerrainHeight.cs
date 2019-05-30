using MapzenGo.Models.Plugins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeight 
{

    public const float heightOffset = 0f; // The heightmesh will nog exactly match the JSON polygon mesh; 
    // returns the height in the local space of pOrgin (this is unity measurement; not meter!)
    public static float GetTerrainHeight(GameObject pOrgin, double offsetX, double offsetZ)
    {
       
        // Get world position
        var worldPos = pOrgin.gameObject.transform.TransformPoint((float)offsetX, 0, (float)offsetZ);
        var hitPoint = TerrainHeight.GetTerrainHeight(worldPos.x, worldPos.z, TileLayerPlugin.TerrainLayer);
        if (!hitPoint.HasValue) return 0;
        
        // y-as is height
        var local = pOrgin.transform.InverseTransformPoint(hitPoint.Value);
        return local.y + heightOffset;

    }

    /// <summary>
    /// Doe een raycast haaks op het terrein
    /// </summary>
    /// <param name="pWorldX"></param>
    /// <param name="pWorldZ"></param>
    /// <param name="pLayerMask"></param>
    /// <returns></returns>
    public static Vector3? GetTerrainHeight(float pWorldX, float pWorldZ, int pLayerMask)
    {
        int mask = (1 << pLayerMask);
     
        Vector3 pos = new Vector3(pWorldX, 10000, pWorldZ);
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(pos, -Vector3.up, out hit, Mathf.Infinity, mask))
        {
            //Vector3 hitPoint = pos + (-Vector3.up * hit.distance);
            //return hitPoint.y;
            return hit.point;
        }
        return null;
    }
}
