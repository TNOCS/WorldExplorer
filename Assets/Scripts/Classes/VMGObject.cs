using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VMGObject : MonoBehaviour {

    public string id;
    public string objName;
    public float lat;
    public float lon;
    // Angle of Orientation.
    // public float aoo;

    public VMGObject(string id, string objName, float lat, float lon)
    {
        this.id = id;
        this.objName = objName;
        this.lat = lat;
        this.lon = lon;
    }
}
