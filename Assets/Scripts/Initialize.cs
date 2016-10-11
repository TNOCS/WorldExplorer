using UnityEngine;
using MapzenGo.Models;
using MapzenGo.Models.Plugins;

public class Initialize : MonoBehaviour {
    private GameObject world;
	// Use this for initialization
	void Start () {
        var appState = AppStateSettings.Instance;

		world = new GameObject("World");
        var tm = world.AddComponent<CachedTileManager>();
        tm.Latitude = 51.45179F;
        tm.Longitude = 5.481454F;
        tm.Range = 2;
        tm.Zoom = 17;
        tm.TileSize = 100;
        tm._key = "vector-tiles-dB21RAF";

        #region UI

        var ui = new GameObject("UI"); // Placeholder (root element in UI tree)

        var place = new GameObject("PlaceContainer");
        AddRectTransformToGameObject(place);
        place.transform.parent = ui.transform;

        var poi = new GameObject("PoiContainer");
        AddRectTransformToGameObject(poi);
        poi.transform.parent = ui.transform;

        #endregion

        #region FACTORIES

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

        var places = new GameObject("PlacesFactory");
        places.transform.parent = factories.transform;
        var placesFactory = places.AddComponent<PlacesFactory>();

        #endregion

        #region TILE PLUGINS

        var tilePlugins = new GameObject("TilePlugins");
        tilePlugins.transform.parent = world.transform;

        var mapImage = new GameObject("MapImage");
        mapImage.transform.parent = tilePlugins.transform;
        var mapImagePlugin = mapImage.AddComponent<MapImagePlugin>();
        mapImagePlugin.TileService = MapImagePlugin.TileServices.Default;

        #endregion
    }

    protected void AddRectTransformToGameObject(GameObject go)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);
        //rt.anchoredPosition = new Vector2(0, 0);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        world.transform.localScale = new Vector3(0.001F, 0.001F, 0.001F);
    }
}
