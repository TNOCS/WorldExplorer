using UnityEngine;
using MapzenGo.Models.Enums;

public class BoundaryFactory : MapzenGo.Models.Factories.BoundaryFactory {

	// Use this for initialization
	public override void Start () {
		MergeMeshes = true;
		Order = 5;

		var bfs = new BoundaryFactorySettings();
		bfs.DefaultBoundary = createBoundarySettings(BoundaryType.Unknown, 1, "Default");

		bfs.SettingsBoundary = new System.Collections.Generic.List<BoundarySettings> {
            createBoundarySettings(BoundaryType.City_Wall, 1, "Default"),
            createBoundarySettings(BoundaryType.Dam, 1, "Default"),
		};

        _settings = bfs;
		base.Start();
	}

	protected BoundarySettings createBoundarySettings(BoundaryType type, int width, string material)
	{
		var bs = new BoundarySettings();
		bs.Type = type;
		bs.Material = (Material)Resources.Load(material, typeof(Material));
        bs.Width = width;
		return bs;
	}
}
