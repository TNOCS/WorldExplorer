using UnityEngine;
using MapzenGo.Models.Settings;
using MapzenGo.Models.Enums;

public class WaterFactory : MapzenGo.Models.Factories.WaterFactory
{
    // Use this for initialization
    public override void Start()
    {
        Order = 1;
        MergeMeshes = true;
        var rfs = new WaterFactorySettings();

        rfs.DefaultWater = createWaterSettings(WaterType.Water, "Water");
        FactorySettings = rfs;
        base.Start();
    }
    protected WaterSettings createWaterSettings(WaterType type, string material)
    {
        var ws = new WaterSettings();
        ws.Type = type;
        ws.Material = (Material)Resources.Load(material, typeof(Material));
        return ws;
    }
}
