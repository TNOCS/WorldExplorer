using MapzenGo.Models.Settings.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapzenGo.Models.Factories
{
    public class BagBuildingFactorySettings : SettingsLayers
    {
        public BagBuildingSettings DefaultBuilding = new BagBuildingSettings();
        public List<BagBuildingSettings> SettingsBuildings;

        public override BagBuildingSettingsField GetSettingsFor<BagBuildingSettingsField>(Enum type)
        {
            if ((BagFunctionType)type == BagFunctionType.Unknown)
                return DefaultBuilding as BagBuildingSettingsField;
            return SettingsBuildings.FirstOrDefault(x => x.Type == (BagFunctionType)type) as BagBuildingSettingsField ?? DefaultBuilding as BagBuildingSettingsField;
        }

        public override bool HasSettingsFor(Enum type)
        {
            return SettingsBuildings.Any(x => x.Type == (BagFunctionType)type);
        }
    }

    [System.Serializable]
    public class BagBuildingSettings : BaseSetting
    {
        public BagFunctionType Type;
        public Material Material;
        public int MinimumBuildingHeight = 2;
        public int MaximumBuildingHeight = 5;
        public bool IsVolumetric = true;
    }

}