using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using MapzenGo.Models.Settings;
using UnityEngine;
using Assets.Scripts.MapzenGoWrappers;

namespace MapzenGo.Models.Factories
{
    public class PoiFactory : Factory
    {
        [SerializeField]
        protected GameObject _labelPrefab;
        [SerializeField]
        protected GameObject _container;
        public override string XmlTag { get { return "pois"; } }
        [SerializeField]
        protected PoiFactorySettings FactorySettings;
        public override void Start()
        {
            base.Start();
            Query = (geo) => geo["geometry"]["type"].str == "Point" && geo["properties"].HasField("name");
        }

        public override void GeoJsonDataLoaded(Tile tile)
        {
            if (!(tile.Data.HasField(XmlTag) && tile.Data[XmlTag].HasField("features")))
                return;

            string temp = tile.Data[XmlTag].ToString();
            var featureList = tile.Data[XmlTag]["features"].list;
            if (featureList != null)
            { 
                foreach (var entity in featureList.Where(x => Query(x)).SelectMany(geo => Create(tile, geo)))
                {
                    //if (entity != null)
                    //{
                    //    entity.transform.SetParent(_container.transform, true);
                    //    entity.transform.localScale = new Vector3(100, 100, 0);
                    //}
                }
            }
        }

        protected override IEnumerable<MonoBehaviour> Create(Tile tile, JSONObject geo)
        {
            var kind = geo["properties"]["kind"].str.ConvertToPoiType();

            if (!FactorySettings.HasSettingsFor(kind))
                yield break;

            var typeSettings = FactorySettings.GetSettingsFor<PoiSettings>(kind);

            var go = new GameObject("Poi"); //Instantiate(_labelPrefab);
            var poi = go.AddComponent<Poi>();
            go.name = "poi-" + tile.name;
            go.tag = "poi";
            //RJ added spriteRenderer
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = typeSettings.Sprite;
            //RJ DELETE? Sprite as 3d objects works better and Image doesn't work either?
            //poi.GetComponentInChildren<Image>().sprite = typeSettings.Sprite;


            //if (geo["properties"].HasField("name"))
            //    go.GetComponentInChildren<TextMesh>().text = geo["properties"]["name"].str;
            var c = geo["geometry"]["coordinates"];
            var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
            var localMercPos = dotMerc - tile.Rect.Center;
            float terrainHeight = TerrainHeight.GetTerrainHeight(tile.gameObject, (float)localMercPos.x, (float)localMercPos.y);
            go.transform.position = new Vector3((float)localMercPos.x, terrainHeight + 30F, (float)localMercPos.y);
            var target = new GameObject("poiTarget");
            var targetScript = target.AddComponent<targetForPoi>();
            target.transform.position = localMercPos.ToVector3();
            target.transform.SetParent(tile.transform, false);
            poi.Stick(target.transform);
            poi.transform.localScale = new Vector3(.2f, .2f, .2f);
            poi.transform.SetParent(target.transform, true);

            var spriteRend = poi.GetComponent<SpriteRenderer>();
            spriteRend.sortingOrder = -2;

            SetProperties(geo, poi, typeSettings);
            targetScript.Name = (poi.Name != null) ? poi.Name : poi.name;
            targetScript.Kind = poi.Kind;
            targetScript.Properties = geo["properties"].ToString();
            yield return poi;
        }

        private static void SetProperties(JSONObject geo, Poi poi, PoiSettings typeSettings)
        {
            poi.Id = geo["properties"]["id"].ToString();
            if (geo["properties"].HasField("name"))
                poi.Name = geo["properties"]["name"].str;
            poi.Type = geo["type"].str;
            poi.Kind = geo["properties"]["kind"].str;
            // poi.name = "poi";
        }
    }
}
