using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using Assets.MapzenGo.Models.Enums;

public class PoiFactory : MapzenGo.Models.Factories.PoiFactory {

	// Use this for initialization
	public override void Start () {
        _labelPrefab = Resources.Load("Poi", typeof(GameObject)) as GameObject;
        _container = GameObject.Find("PoiContainer");

        var pfs = new PoiFactorySettings();
        pfs.DefaultPoi = createPoiSettings(PoiType.Unknown);
        FactorySettings = pfs;

        base.Start();
    }

    protected PoiSettings createPoiSettings(PoiType type)
    {
        var ps = new PoiSettings();
        ps.Type = type;
        //ps.Sprite
        return ps;
    }

}
