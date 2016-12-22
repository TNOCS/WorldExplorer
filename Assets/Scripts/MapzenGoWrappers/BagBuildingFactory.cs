using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using MapzenGo.Models.Enums;
using MapzenGo.Models.Settings;
using TriangleNet;
using TriangleNet.Geometry;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapzenGo.Models.Factories
{
    public enum BagFunctionType {
        Unknown,
        Residential,
        Meeting,
        Prison,
        Health,
        Industrial,
        Office,
        Lodgings,
        Education,
        Sport,
        Shop,
        Other
    }

    public static class FunctionToBuilding
    {
        public static BagFunctionType ConvertFunctionToBuildingType(this string function)
        {
            switch (function.ToLowerInvariant())
            {
                case "woonfunctie": return BagFunctionType.Residential;
                case "bijeenkomstfunctie": return BagFunctionType.Meeting;
                case "celfunctie": return BagFunctionType.Prison;
                case "gezondheidszorgfunctie": return BagFunctionType.Health;
                case "industriefunctie": return BagFunctionType.Industrial;
                case "kantoorfunctie": return BagFunctionType.Office;
                case "logiesfunctie": return BagFunctionType.Lodgings;
                case "onderwijsfunctie": return BagFunctionType.Education;
                case "sportfunctie": return BagFunctionType.Sport;
                case "winkelfunctie": return BagFunctionType.Shop;
                case "overige gebruiksfunctie": return BagFunctionType.Other;
                default: return BagFunctionType.Unknown;
            }
        }
    }

    public class BagBuildingFactory : Factory
    {
        [SerializeField]
        protected bool _useTriangulationNet;
        public override string XmlTag { get { return "gebouwen"; } }
        private HashSet<string> _active = new HashSet<string>();
        [SerializeField]
        protected BagBuildingFactorySettings FactorySettings;
        private TriangleNet.Mesh _mesh;

        public override void Start()
        {
            base.Start();
            _mesh = new TriangleNet.Mesh();
            Query = (geo) => geo["geometry"]["type"].str == "Polygon";

            Order = 2;
            MergeMeshes = true;
            _useTriangulationNet = true;

            var bfs = ScriptableObject.CreateInstance("BagBuildingFactorySettings") as BagBuildingFactorySettings;
            bfs.DefaultBuilding = createBuildingSettings(BagFunctionType.Unknown, 3, 6, "Default");

            bfs.SettingsBuildings = new List<BagBuildingSettings> {
                createBuildingSettings(BagFunctionType.Health, 16, 16, "Hospital"),
                createBuildingSettings(BagFunctionType.Education, 10, 10, "University"),
                createBuildingSettings(BagFunctionType.Residential, 7, 7, "Residential"),
                createBuildingSettings(BagFunctionType.Office, 4, 8, "Industrial"),
                createBuildingSettings(BagFunctionType.Industrial, 4, 8, "Industrial"),
                createBuildingSettings(BagFunctionType.Shop, 4, 10, "Commercial"),
                createBuildingSettings(BagFunctionType.Meeting, 4, 10, "Commercial"),
                createBuildingSettings(BagFunctionType.Lodgings, 4, 10, "Commercial"),
                createBuildingSettings(BagFunctionType.Other, 4, 10, "Default"),
                createBuildingSettings(BagFunctionType.Sport, 12, 12, "Industrial")
            };

            FactorySettings = bfs;
        }

        protected BagBuildingSettings createBuildingSettings(BagFunctionType type, int min, int max, string material)
        {
            const int EXAGERATION_FACTOR = 1;
            var bs = new BagBuildingSettings();
            bs.Type = type;
            bs.Material = (Material)Resources.Load(material, typeof(Material));
            bs.MinimumBuildingHeight = EXAGERATION_FACTOR * min;
            bs.MaximumBuildingHeight = EXAGERATION_FACTOR * max;
            bs.IsVolumetric = true;
            return bs;
        }

        protected override IEnumerable<MonoBehaviour> Create(Tile tile, JSONObject geo)
        {
            var key = geo["properties"]["id"].ToString();
            var kind = geo["properties"].HasField("functie")
                ? geo["properties"]["functie"].str.ConvertFunctionToBuildingType()
                : BagFunctionType.Unknown;
            if (!_active.Contains(key))
            {
                _active.Add(key);
                tile.Destroyed += (s, e) => { _active.Remove(key); };

                var typeSettings = FactorySettings.GetSettingsFor<BagBuildingSettings>(kind);
                var buildingCorners = new List<Vector3>();

                float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
                var bb = geo["geometry"]["coordinates"].list[0]; //this is wrong but cant fix it now
                for (int i = 0; i < bb.list.Count - 1; i++)
                {
                    var c = bb.list[i];
                    var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
                    var localMercPos = dotMerc - tile.Rect.Center;

                    if (localMercPos.x < minx) minx = (float)localMercPos.x;
                    if (localMercPos.y < miny) miny = (float)localMercPos.y;
                    if (localMercPos.x > maxx) maxx = (float)localMercPos.x;
                    if (localMercPos.y > maxy) maxy = (float)localMercPos.y;

                    buildingCorners.Add(localMercPos.ToVector3());
                }

                var building = new GameObject("Building").AddComponent<Building>();
                var mesh = building.GetComponent<MeshFilter>().mesh;

                var buildingCenter = ChangeToRelativePositions(buildingCorners);
                building.transform.localPosition = buildingCenter;

                SetProperties(geo, building, typeSettings);

                var height = 0f;
                var minHeight = 0f;
                if (typeSettings.IsVolumetric)
                {
                    height = geo["properties"].HasField("height") ? geo["properties"]["height"].f : Random.Range(typeSettings.MinimumBuildingHeight, typeSettings.MaximumBuildingHeight);
                    minHeight = GetMinHeight(geo);
                }

                var tb = new MeshData();
                CreateMesh(buildingCorners, minHeight, height, typeSettings, tb, new Vector2(minx, miny), new Vector2(maxx - minx, maxy - miny));

                mesh.vertices = tb.Vertices.ToArray();
                mesh.triangles = tb.Indices.ToArray();
                mesh.SetUVs(0, tb.UV);
                mesh.RecalculateNormals();

                yield return building;
            }
        }

        protected override GameObject CreateLayer(Tile tile, List<JSONObject> items)
        {
            var main = new GameObject("Buildings Layer");
            var finalList = new Dictionary<BagFunctionType, MeshData>();
            var openList = new Dictionary<BagFunctionType, MeshData>();

            foreach (var geo in items.Where(x => Query(x)))
            {
                if (!geo["properties"].HasField("id")) continue;
                var key = geo["properties"]["id"].ToString();
                if (_active.Contains(key))
                    continue;

                //to prevent duplicate buildings
                _active.Add(key);
                tile.Destroyed += (s, e) => { _active.Remove(key); };

                var kind = geo["properties"].HasField("functie")
                ? geo["properties"]["functie"].str.ConvertFunctionToBuildingType()
                : BagFunctionType.Unknown;
                // Debug.Log(kind);
                var typeSettings = FactorySettings.GetSettingsFor<BagBuildingSettings>(kind);

                //if we dont have a setting defined for that, it'Ll be merged to "unknown" 
                if (!FactorySettings.HasSettingsFor(kind))
                    kind = BagFunctionType.Unknown;

                if (!openList.ContainsKey(kind))
                    openList.Add(kind, new MeshData());

                var buildingCorners = new List<Vector3>();
                //foreach (var bb in geo["geometry"]["coordinates"].list)z
                //{

                float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
                var bb = geo["geometry"]["coordinates"].list[0]; //this is wrong but cant fix it now
                for (int i = 0; i < bb.list.Count - 1; i++)
                {
                    var c = bb.list[i];
                    var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
                    var localMercPos = new Vector2((float)(dotMerc.x - tile.TileCenter.x), (float)(dotMerc.y - tile.TileCenter.y));

                    if (localMercPos.x < minx) minx = localMercPos.x;
                    if (localMercPos.y < miny) miny = localMercPos.y;
                    if (localMercPos.x > maxx) maxx = localMercPos.x;
                    if (localMercPos.y > maxy) maxy = localMercPos.y;

                    buildingCorners.Add(localMercPos.ToVector3xz());
                }

                //returns random height if real value not available
                var height = GetHeights(geo, typeSettings.MinimumBuildingHeight, typeSettings.MaximumBuildingHeight);
                var minHeight = GetMinHeight(geo);
                //create mesh, actually just to get vertice&indices
                //filling last two parameters, horrible call yea
                CreateMesh(buildingCorners, minHeight, height, typeSettings, openList[kind], new Vector2(minx, miny), new Vector2(maxx - minx, maxy - miny));

                //unity cant handle more than 65k on single mesh
                //so we'll finish current and start a new one
                if (openList[kind].Vertices.Count > 64000)
                {
                    //var tb = new MeshData();
                    //tb.Vertices.AddRange(openList[kind].Vertices);
                    //tb.Indices.AddRange(openList[kind].Indices);
                    //tb.UV.AddRange(openList[kind].UV);
                    //finalList.Add(kind, tb);
                    CreateGameObject(kind, openList[kind], main);
                    openList[kind] = new MeshData();
                }
                //}
            }

            foreach (var tuple in openList)
            {
                var tb = new MeshData();
                tb.Vertices.AddRange(tuple.Value.Vertices);
                tb.Indices.AddRange(tuple.Value.Indices);
                tb.UV.AddRange(tuple.Value.UV);
                finalList.Add(tuple.Key, tb);
            }

            foreach (var group in finalList)
            {
                CreateGameObject(group.Key, group.Value, main);
            }
            return main;
        }

        private float GetHeights(JSONObject geo, float min, float max)
        {
            var height = 0f;
            if (FactorySettings.DefaultBuilding.IsVolumetric)
            {
                height = geo["properties"].HasField("height")
                    ? geo["properties"]["height"].f
                    : Random.Range(min, max);
            }
            return height;
        }

        private float GetMinHeight(JSONObject geo)
        {
            var height = 0f;
            if (FactorySettings.DefaultBuilding.IsVolumetric)
            {
                height = geo["properties"].HasField("min_height")
                    ? geo["properties"]["min_height"].f
                    : 0;
            }
            return height;
        }

        private Vector3 ChangeToRelativePositions(List<Vector3> buildingCorners)
        {
            var buildingCenter = buildingCorners.Aggregate((acc, cur) => acc + cur) / buildingCorners.Count;
            for (int i = 0; i < buildingCorners.Count; i++)
            {
                //using corner position relative to building center
                buildingCorners[i] = buildingCorners[i] - buildingCenter;
            }
            return buildingCenter;
        }

        private static void SetProperties(JSONObject geo, Building building, BagBuildingSettings typeSettings)
        {
            building.name = "building " + geo["properties"]["id"].ToString();
            if (geo["properties"].HasField("name"))
                building.Name = geo["properties"]["name"].str;

            building.Id = geo["properties"]["id"].ToString();
            building.Type = geo["type"].str;
            building.SortKey = (int)geo["properties"]["sort_key"].f;
            building.Kind = typeSettings.Type.ToString();
            // building.Type = typeSettings.Type.ToString();
            building.GetComponent<MeshRenderer>().material = typeSettings.Material;
        }

        private void CreateMesh(List<Vector3> corners, float min_height, float height, BagBuildingSettings typeSettings, MeshData data, Vector2 min, Vector2 size)
        {
            var vertsStartCount = _useTriangulationNet
                    ? CreateRoofTriangulation(corners, height, data)
                    : CreateRoofClass(corners, height, data);

            foreach (var c in corners)
            {
                data.UV.Add(new Vector2((c.x - min.x), (c.z - min.y)));
            }

            if (typeSettings.IsVolumetric)
            {
                float d = 0f;
                Vector3 v1;
                Vector3 v2;
                int ind = 0;
                //for (int i = corners.Count - 1; i < 1; i--)
                for (int i = 1; i < corners.Count; i++)
                {
                    v1 = data.Vertices[vertsStartCount + i - 1];
                    v2 = data.Vertices[vertsStartCount + i];
                    ind = data.Vertices.Count;
                    data.Vertices.Add(v1);
                    data.Vertices.Add(v2);
                    data.Vertices.Add(new Vector3(v1.x, min_height, v1.z));
                    data.Vertices.Add(new Vector3(v2.x, min_height, v2.z));

                    d = (v2 - v1).magnitude;

                    data.UV.Add(new Vector2(0, 0));
                    data.UV.Add(new Vector2(d, 0));
                    data.UV.Add(new Vector2(0, height));
                    data.UV.Add(new Vector2(d, height));

                    data.Indices.Add(ind);
                    data.Indices.Add(ind + 2);
                    data.Indices.Add(ind + 1);

                    data.Indices.Add(ind + 1);
                    data.Indices.Add(ind + 2);
                    data.Indices.Add(ind + 3);
                }

                v1 = data.Vertices[vertsStartCount];
                v2 = data.Vertices[vertsStartCount + corners.Count - 1];
                ind = data.Vertices.Count;
                data.Vertices.Add(v1);
                data.Vertices.Add(v2);
                data.Vertices.Add(new Vector3(v1.x, min_height, v1.z));
                data.Vertices.Add(new Vector3(v2.x, min_height, v2.z));

                d = (v2 - v1).magnitude;

                data.UV.Add(new Vector2(0, 0));
                data.UV.Add(new Vector2(d, 0));
                data.UV.Add(new Vector2(0, height));
                data.UV.Add(new Vector2(d, height));

                data.Indices.Add(ind);
                data.Indices.Add(ind + 1);
                data.Indices.Add(ind + 2);

                data.Indices.Add(ind + 1);
                data.Indices.Add(ind + 3);
                data.Indices.Add(ind + 2);
            }
        }

        private static int CreateRoofClass(List<Vector3> corners, float height, MeshData data)
        {
            var vertsStartCount = data.Vertices.Count;
            var tris = new Triangulator(corners);
            data.Vertices.AddRange(corners.Select(x => new Vector3(x.x, height, x.z)).ToList());
            data.Indices.AddRange(tris.Triangulate().Select(x => vertsStartCount + x));
            return vertsStartCount;
        }

        private int CreateRoofTriangulation(List<Vector3> corners, float height, MeshData data)
        {
            _mesh = new TriangleNet.Mesh();
            var inp = new InputGeometry(corners.Count);
            for (int i = 0; i < corners.Count; i++)
            {
                var v = corners[i];
                inp.AddPoint(v.x, v.z);
                inp.AddSegment(i, (i + 1) % corners.Count);
            }
            _mesh.Behavior.Algorithm = TriangulationAlgorithm.SweepLine;
            _mesh.Behavior.Quality = true;
            _mesh.Triangulate(inp);

            var vertsStartCount = data.Vertices.Count;
            data.Vertices.AddRange(corners.Select(x => new Vector3(x.x, height, x.z)).ToList());

            foreach (var tri in _mesh.Triangles)
            {
                data.Indices.Add(vertsStartCount + tri.P1);
                data.Indices.Add(vertsStartCount + tri.P0);
                data.Indices.Add(vertsStartCount + tri.P2);
            }
            return vertsStartCount;
        }

        private void CreateGameObject(BagFunctionType kind, MeshData data, GameObject main)
        {
            var go = new GameObject(kind + " Buildings");
            var mesh = go.AddComponent<MeshFilter>().mesh;
            go.AddComponent<MeshRenderer>();
            mesh.vertices = data.Vertices.ToArray();
            mesh.triangles = data.Indices.ToArray();
            mesh.SetUVs(0, data.UV);
            mesh.RecalculateNormals();
            go.GetComponent<MeshRenderer>().material = FactorySettings.GetSettingsFor<BagBuildingSettings>(kind).Material;
            go.transform.position += Vector3.up * Order;
            go.transform.SetParent(main.transform, false);
        }
    }
}
