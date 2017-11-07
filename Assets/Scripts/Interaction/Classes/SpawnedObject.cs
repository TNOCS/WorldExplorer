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
    public double lat;
    public double lon;
    public Vector2d meters;
    // String for visualization in Inspector (Vector2d can't be displayed)
    public string meterString;
    
    public SpawnedObject(GameObject go, Vector3 centerPos, double lat, double lon, Vector2d meters)
    {
        obj = go;
        centerPosition = centerPos;
        this.lat = lat;
        this.lon = lon;
        this.meters = meters;
        meterString = meters.ToString();
        Debug.Log("New Object Created: " + obj + " at position: " + centerPosition+ "at lat: "+ lat + " and lon: " + lon);
    }    
}
