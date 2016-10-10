using UnityEngine;
using System.Collections.Generic;
using MapzenGo.Models.Settings;
using MapzenGo.Models.Enums;

public class LanduseFactory : MapzenGo.Models.Factories.LanduseFactory
{

    // Use this for initialization
    public override void Start()
    {
        Order = 1.5F;
        MergeMeshes = true;
        var lfs = new LanduseFactorySettings();

        //lfs.DefaultLanduse = createLanduseSettings(LanduseKind.Unknown, "Default");

        //lfs.SettingsLanduse = new List<LanduseSettings> {
        //    createLanduseSettings(LanduseKind.University, "University"),
        //    createLanduseSettings(LanduseKind.Park, "Park"),
        //    createLanduseSettings(LanduseKind.National_park, "Park"),
        //    createLanduseSettings(LanduseKind.Nature_reserve, "Park"),
        //    createLanduseSettings(LanduseKind.Meadow, "Park"),
        //    createLanduseSettings(LanduseKind.Industrial, "Industrial"),
        //    createLanduseSettings(LanduseKind.Residential, "Residential"),
        //    createLanduseSettings(LanduseKind.Railway, "Rail")
        //};

        FactorySettings = lfs;
        base.Start();
    }
    protected LanduseSettings createLanduseSettings(LanduseKind type, string material)
    {
        var ls = new LanduseSettings();
        ls.Type = type;
        ls.Material = (Material)Resources.Load(material, typeof(Material));
        return ls;
    }

}
