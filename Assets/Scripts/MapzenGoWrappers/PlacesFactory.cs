using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models.Enums;

public class PlacesFactory : MapzenGo.Models.Factories.PlacesFactory
{

	// Use this for initialization
	public override void Start () {
        _labelPrefab = Resources.Load("Place", typeof(GameObject)) as GameObject;
        _container = GameObject.Find("PlaceContainer");

        var pfs = ScriptableObject.CreateInstance("PlacesFactorySettings") as PlacesFactorySettings;

        pfs.DefaultPlace = createPlaceSettings(PlaceType.Unknown, 16, Color.white, Color.black);
        pfs.SettingsPlace = new System.Collections.Generic.List<PlaceSettings> {
            createPlaceSettings(PlaceType.Borough, 12, Color.white, Color.black),
            createPlaceSettings(PlaceType.Suburb, 12, Color.white, Color.black),
            createPlaceSettings(PlaceType.Neighbourhood, 12, Color.white, Color.black),
            createPlaceSettings(PlaceType.Village, 14, Color.white, Color.black),
            createPlaceSettings(PlaceType.Town, 14, Color.white, Color.black),
            createPlaceSettings(PlaceType.City, 16, Color.white, Color.black)
        };

        FactorySettings = pfs;

        base.Start();
    }

    protected PlaceSettings createPlaceSettings(PlaceType type, int fontSize, Color color, Color stroke)
    {
        var ps = new PlaceSettings();
        ps.Type = type;
        ps.FontSize = fontSize;
        ps.Color = color;
        ps.OutlineColor = stroke;
        return ps;
    }
}
