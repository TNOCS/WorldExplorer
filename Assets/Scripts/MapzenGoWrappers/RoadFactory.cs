using UnityEngine;
using System.Collections.Generic;
using MapzenGo.Models.Settings;
using MapzenGo.Models.Enums;

public class RoadFactory : MapzenGo.Models.Factories.RoadFactory {

	// Use this for initialization
	public override void Start () {
        Order = 1;
        MergeMeshes = true;
        var rfs = new RoadFactorySettings();

        rfs.DefaultRoad = createRoadSettings(RoadType.Unknown, 3, "RoadMaterial/Road");

        rfs.SettingsRoad = new List<RoadSettings> {
            createRoadSettings(RoadType.Highway, 10, "RoadMaterial/Highway"),
            createRoadSettings(RoadType.Major_Road, 8, "RoadMaterial/MajorRoad"),
            createRoadSettings(RoadType.Minor_Road, 6, "RoadMaterial/MinorRoad"),
            createRoadSettings(RoadType.Path, 1, "RoadMaterial/Path"),
            createRoadSettings(RoadType.Rail, 2, "RoadMaterial/Rail"),
        };

        FactorySettings = rfs;
        base.Start();
	}
    protected RoadSettings createRoadSettings(RoadType type, int width, string material)
    {
        var rs = new RoadSettings();
        rs.Type = type;
        rs.Material = (Material)Resources.Load(material, typeof(Material));
        rs.Width = width;
        return rs;
    }
}
