using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using Assets.MapzenGo.Models.Enums;

public class PoiFactory : MapzenGo.Models.Factories.PoiFactory {

	// Use this for initialization
	public override void Start () {
        _labelPrefab = Resources.Load("Poi", typeof(GameObject)) as GameObject;
        _container = GameObject.Find("PoiContainer");

        var pfs = ScriptableObject.CreateInstance("PoiFactorySettings") as PoiFactorySettings;

        pfs.DefaultPoi = createPoiSettings(PoiType.Unknown, "Textures/poi_icons_18@2x_139");

        pfs.SettingsPoi = new System.Collections.Generic.List<PoiSettings> {
            createPoiSettings(PoiType.Library, "Textures/poi_icons_18@2x_0"),
            createPoiSettings(PoiType.TramStop, "Textures/poi_icons_18@2x_15"),
            createPoiSettings(PoiType.Station, "Textures/poi_icons_18@2x_9"),
            createPoiSettings(PoiType.Hospital, "Textures/poi_icons_18@2x_4"),
            createPoiSettings(PoiType.Police, "Textures/poi_icons_18@2x_5"),
            createPoiSettings(PoiType.FireStation, "Textures/poi_icons_18@2x_7"),
            createPoiSettings(PoiType.Hotel, "Textures/poi_icons_18@2x_16"),
            createPoiSettings(PoiType.BusStop, "Textures/poi_icons_18@2x_17"),
            createPoiSettings(PoiType.FerryTerminal, "Textures/poi_icons_18@2x_18"),
            createPoiSettings(PoiType.Airport, "Textures/poi_icons_18@2x_19"),
            createPoiSettings(PoiType.Toys, "Textures/poi_icons_18@2x_20"),
            createPoiSettings(PoiType.Playground, "Textures/poi_icons_18@2x_32"),
            createPoiSettings(PoiType.Forest, "Textures/poi_icons_18@2x_111"),

            createPoiSettings(PoiType.Zoo, "Textures/poi_icons_18@2x_65"),
            createPoiSettings(PoiType.Museum, "Textures/poi_icons_18@2x_67"),
            createPoiSettings(PoiType.Parking, "Textures/poi_icons_18@2x_81"),
            createPoiSettings(PoiType.Atm, "Textures/poi_icons_18@2x_86"),
            createPoiSettings(PoiType.PostBox, "Textures/poi_icons_18@2x_95"),
            createPoiSettings(PoiType.Pub, "Textures/poi_icons_18@2x_93"),
            createPoiSettings(PoiType.Restaurant, "Textures/poi_icons_18@2x_42"),
            createPoiSettings(PoiType.Stadium, "Textures/poi_icons_18@2x_76"),
            createPoiSettings(PoiType.Cinema, "Textures/poi_icons_18@2x_105"),
            createPoiSettings(PoiType.University, "Textures/poi_icons_18@2x_107"),
            createPoiSettings(PoiType.Gas, "Textures/poi_icons_18@2x_109"),
            createPoiSettings(PoiType.Information, "Textures/poi_icons_18@2x_118")
        };

        FactorySettings = pfs;

        base.Start();
    }

    protected PoiSettings createPoiSettings(PoiType type, string path)
    {
        var ps = new PoiSettings();
        ps.Type = type;
        ps.Sprite = SpriteRepository.GetSpriteFromSheet(path);
        return ps;
    }

}
