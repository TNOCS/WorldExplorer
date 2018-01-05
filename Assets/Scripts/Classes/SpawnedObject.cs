using Assets.Scripts;
using MapzenGo.Helpers;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnedObject {

    public GameObject obj;
    public Vector3 centerPosition;
    public Vector3 localScale;
    public Quaternion rotation;
    public double lat;
    public double lon;
    public Vector2d meters;
    
    public SpawnedObject(GameObject go, Vector3 centerPos, double lat, double lon, Vector3 scale, Quaternion rotation /*Vector2d meters*/)
    {
        obj = go;
        centerPosition = centerPos;
        this.lat = lat;
        this.lon = lon;
        this.localScale = scale;
        this.rotation = rotation;
        //this.meters = meters;
    }    
}
