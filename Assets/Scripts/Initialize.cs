using UnityEngine;
using MapzenGo.Models;
using MapzenGo.Models.Plugins;

public class Initialize : MonoBehaviour {
    private GameObject world;
	// Use this for initialization
	void Start () {
        var appState = AppStateSettings.Instance;

		world = new GameObject("SecondWorld");
        var tm = world.AddComponent<CachedTileManager>();
        tm.Latitude = 51.45179F;
        tm.Longitude = 5.481454F;
        tm.Range = 2;
        tm.Zoom = 17;
        tm.TileSize = 100;
        tm._key = "vector-tiles-dB21RAF";

        var factories = new GameObject("Factories");
        factories.transform.parent = world.transform;

        var buildings = new GameObject("BuildingFactory");
        buildings.transform.parent = factories.transform;
        var buildingFactory = buildings.AddComponent<BuildingFactory>();

        var roads = new GameObject("RoadFactory");
        roads.transform.parent = factories.transform;
        var roadFactory = roads.AddComponent<RoadFactory>();

        var water = new GameObject("WaterFactory");
        water.transform.parent = factories.transform;
        var waterFactory = water.AddComponent<WaterFactory>();

        var landuse = new GameObject("LanduseFactory");
        landuse.transform.parent = factories.transform;
        var landuseFactory = landuse.AddComponent<LanduseFactory>();


        var tilePlugins = new GameObject("TilePlugins");
        tilePlugins.transform.parent = world.transform;

        var mapImage = new GameObject("MapImage");
        mapImage.transform.parent = tilePlugins.transform;
        var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
        mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;
    }

    // Update is called once per frame
    void Update()
    {
        world.transform.localScale = new Vector3(0.001F, 0.001F, 0.001F);
    }
}
