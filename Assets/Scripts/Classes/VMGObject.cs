using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VMGObject : MonoBehaviour {
    
    /// <summary>
    /// The data of a VMG Object.
    /// </summary>
    public string id;
    public string objName;
    public float lat;
    public float lon;

    public VMGObject(string id, string objName, float lat, float lon)
    {
        this.id = id;
        this.objName = objName;
        this.lat = lat;
        this.lon = lon;
    }
}
