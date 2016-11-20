using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models.Enums;

public class BuildingFactory : MapzenGo.Models.Factories.BuildingFactory {

	// Use this for initialization
	public override void Start () {
        Order = 2;
        MergeMeshes = true;
        _useTriangulationNet = true;


        //var appState = AppStateSettings.Instance;
        //FactorySettings = appState.BuildingFactorySettings;
        var bfs = ScriptableObject.CreateInstance("BuildingFactorySettings") as BuildingFactorySettings;
        //var bfs = new BuildingFactorySettings();
        bfs.DefaultBuilding = createBuildingSettings(BuildingType.Unknown, 3, 6, "Default");

        bfs.SettingsBuildings = new System.Collections.Generic.List<BuildingSettings> {
            createBuildingSettings(BuildingType.Hospital, 16, 16, "Hospital"),
            createBuildingSettings(BuildingType.School, 5, 5, "University"),
            createBuildingSettings(BuildingType.Residential, 7, 7, "Residential"),
            createBuildingSettings(BuildingType.Industrial, 4, 8, "Industrial"),
            createBuildingSettings(BuildingType.Commercial, 4, 10, "Commercial"),
            createBuildingSettings(BuildingType.University, 12, 12, "University")
        };

        FactorySettings = bfs;
        base.Start();	
	}	

    protected BuildingSettings createBuildingSettings(BuildingType type, int min, int max, string material)
    {
        var bs = new BuildingSettings();
        bs.Type = type;
        bs.Material = (Material)Resources.Load(material, typeof(Material));
        bs.MinimumBuildingHeight = min;
        bs.MaximumBuildingHeight = max;
        bs.IsVolumetric = true;
        return bs;
    }
}
