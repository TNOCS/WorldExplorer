using UnityEngine;
using System.Collections;

public class InitHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var center = GameObject.Find("_centerIndicator");
        var world = GameObject.Find("World");
        var tiles = new GameObject("mapzen");
        var ctm = tiles.AddComponent<MapzenGo.Models.CachedTileManager>();  
        //ctm._centerIndicator = center.transform;
        

        //tiles.AddComponent()
        //world.AddComponent()

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
