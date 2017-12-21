using MapzenGo.Helpers;
using MapzenGo.Models.Settings;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    public class CustomBuildingPlugin : Plugin
    {
//       // [SerializeField]
//       // protected BuildingFactorySettings FactorySettings;
//       private TriangleNet.Mesh _mesh;
//       protected bool _useTriangulationNet;
//
//       public void Start()
//       {
//           _mesh = new TriangleNet.Mesh();
//       }
//
//       public override void Create(Tile tile)
//       {
//           base.Create(tile);
//
//           foreach (VMGBuilding vmgBuilding in VMGBuildingsFactory.Instance.VMGBuildingList)
//           {
//               // Either generate mesh of building (for basic buildings)
//               // Or extract middle point of all Vector2s of the list and place building there (use this if you want actual 3D models)
//
//
//
//               //var typeSettings = FactorySettings.GetSettingsFor<BuildingSettings>(kind);
//               var buildingCorners = new List<Vector3>();
//
//               float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
//               //var bb = geo["geometry"]["coordinates"].list[0]; //this is wrong but cant fix it now
//               //for (int i = 0; i < bb.list.Count - 1; i++)
//               for (int i = 0; i < vmgBuilding.latLonList.Count; i++)
//               {
//                   //var c = bb.list[i];
//                   var c = vmgBuilding.id;
//                   //var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
//                   var dotMerc = GM.LatLonToMeters(vmgBuilding.latLonList[i][0], vmgBuilding.latLonList[i][1]);
//                   var localMercPos = dotMerc - tile.Rect.Center;
//
//                   if (localMercPos.x < minx) minx = (float)localMercPos.x;
//                   if (localMercPos.y < miny) miny = (float)localMercPos.y;
//                   if (localMercPos.x > maxx) maxx = (float)localMercPos.x;
//                   if (localMercPos.y > maxy) maxy = (float)localMercPos.y;
//
//                   buildingCorners.Add(localMercPos.ToVector3());
//               }
//
//               var building = new GameObject("VMGBuilding").AddComponent<Building>();
//               var mesh = building.GetComponent<MeshFilter>().mesh;
//
//               var buildingCenter = ChangeToRelativePositions(buildingCorners);
//               building.transform.localPosition = buildingCenter;
//
//               //SetProperties(geo, building, typeSettings);
//
//               var height = 0f;
//               var minHeight = 0f;
//               //  if (typeSettings.IsVolumetric)
//               //  {
//               // height = geo["properties"].HasField("height") ? geo["properties"]["height"].f : Random.Range(typeSettings.MinimumBuildingHeight, typeSettings.MaximumBuildingHeight);
//               // minHeight = GetMinHeight(geo);
//               height = vmgBuilding.height;
//               minHeight = vmgBuilding.height;
//               // }
//
//               var tb = new MeshData();
//               CreateMesh(buildingCorners, minHeight, height, /*typeSettings, */tb, new Vector2(minx, miny), new Vector2(maxx - minx, maxy - miny));
//
//               mesh.vertices = tb.Vertices.ToArray();
//               mesh.triangles = tb.Indices.ToArray();
//               mesh.SetUVs(0, tb.UV);
//               mesh.RecalculateNormals();
//
//
//
//
//               // Check first: buildings are already being spawned through BuildingFactory. Are they the correct ones, or are the ones from the JSON a different kind of building? 
//           }
//       }
//
//       /*private float GetMinHeight(JSONObject geo)
//       {
//           var height = 0f;
//           if (FactorySettings.DefaultBuilding.IsVolumetric)
//           {
//               height = geo["properties"].HasField("min_height")
//                   ? geo["properties"]["min_height"].f
//                   : 0;
//           }
//           return height;
//       }
//       */
//       private Vector3 ChangeToRelativePositions(List<Vector3> buildingCorners)
//       {
//           var buildingCenter = buildingCorners.Aggregate((acc, cur) => acc + cur) / buildingCorners.Count;
//           for (int i = 0; i < buildingCorners.Count; i++)
//           {
//               //using corner position relative to building center
//               buildingCorners[i] = buildingCorners[i] - buildingCenter;
//           }
//           return buildingCenter;
//       }
//
//       /* private static void SetProperties(JSONObject geo, Building building, BuildingSettings typeSettings)
//        {
//            building.name = "building " + geo["properties"]["id"].ToString();
//            if (geo["properties"].HasField("name"))
//                building.Name = geo["properties"]["name"].str;
//
//            building.Id = geo["properties"]["id"].ToString();
//            building.Type = geo["type"].str;
//            building.SortKey = (int)geo["properties"]["sort_key"].f;
//            building.Kind = typeSettings.Type.ToString();
//            // building.Type = typeSettings.Type.ToString();
//            building.GetComponent<MeshRenderer>().material = typeSettings.Material;
//
//        }*/
//
//       private void CreateMesh(List<Vector3> corners, float min_height, float height, /*BuildingSettings typeSettings,*/ MeshData data, Vector2 min, Vector2 size)
//       {
//           var vertsStartCount = _useTriangulationNet
//                   ? CreateRoofTriangulation(corners, height, data)
//                   : CreateRoofClass(corners, height, data);
//
//           foreach (var c in corners)
//           {
//               data.UV.Add(new Vector2((c.x - min.x), (c.z - min.y)));
//           }
//
//           // if (typeSettings.IsVolumetric)
//           // {
//           float d = 0f;
//           Vector3 v1;
//           Vector3 v2;
//           int ind = 0;
//           for (int i = 1; i < corners.Count; i++)
//           {
//               v1 = data.Vertices[vertsStartCount + i - 1];
//               v2 = data.Vertices[vertsStartCount + i];
//               ind = data.Vertices.Count;
//               data.Vertices.Add(v1);
//               data.Vertices.Add(v2);
//               data.Vertices.Add(new Vector3(v1.x, min_height, v1.z));
//               data.Vertices.Add(new Vector3(v2.x, min_height, v2.z));
//
//               d = (v2 - v1).magnitude;
//
//               data.UV.Add(new Vector2(0, 0));
//               data.UV.Add(new Vector2(d, 0));
//               data.UV.Add(new Vector2(0, height));
//               data.UV.Add(new Vector2(d, height));
//
//               data.Indices.Add(ind);
//               data.Indices.Add(ind + 2);
//               data.Indices.Add(ind + 1);
//
//               data.Indices.Add(ind + 1);
//               data.Indices.Add(ind + 2);
//               data.Indices.Add(ind + 3);
//           }
//
//           v1 = data.Vertices[vertsStartCount];
//           v2 = data.Vertices[vertsStartCount + corners.Count - 1];
//           ind = data.Vertices.Count;
//           data.Vertices.Add(v1);
//           data.Vertices.Add(v2);
//           data.Vertices.Add(new Vector3(v1.x, min_height, v1.z));
//           data.Vertices.Add(new Vector3(v2.x, min_height, v2.z));
//
//           d = (v2 - v1).magnitude;
//
//           data.UV.Add(new Vector2(0, 0));
//           data.UV.Add(new Vector2(d, 0));
//           data.UV.Add(new Vector2(0, height));
//           data.UV.Add(new Vector2(d, height));
//
//           data.Indices.Add(ind);
//           data.Indices.Add(ind + 1);
//           data.Indices.Add(ind + 2);
//
//           data.Indices.Add(ind + 1);
//           data.Indices.Add(ind + 3);
//           data.Indices.Add(ind + 2);
//           // }
//       }
//
//       private static int CreateRoofClass(List<Vector3> corners, float height, MeshData data)
//       {
//           var vertsStartCount = data.Vertices.Count;
//           var tris = new Triangulator(corners);
//           data.Vertices.AddRange(corners.Select(x => new Vector3(x.x, height, x.z)).ToList());
//           data.Indices.AddRange(tris.Triangulate().Select(x => vertsStartCount + x));
//           return vertsStartCount;
//       }
//
//       private int CreateRoofTriangulation(List<Vector3> corners, float height, MeshData data)
//       {
//           _mesh = new TriangleNet.Mesh();
//           var inp = new InputGeometry(corners.Count);
//           for (int i = 0; i < corners.Count; i++)
//           {
//               var v = corners[i];
//               inp.AddPoint(v.x, v.z);
//               inp.AddSegment(i, (i + 1) % corners.Count);
//           }
//           _mesh.Behavior.Algorithm = TriangulationAlgorithm.SweepLine;
//           _mesh.Behavior.Quality = true;
//           _mesh.Triangulate(inp);
//
//           var vertsStartCount = data.Vertices.Count;
//           data.Vertices.AddRange(corners.Select(x => new Vector3(x.x, height, x.z)).ToList());
//
//           foreach (var tri in _mesh.Triangles)
//           {
//               data.Indices.Add(vertsStartCount + tri.P1);
//               data.Indices.Add(vertsStartCount + tri.P0);
//               data.Indices.Add(vertsStartCount + tri.P2);
//           }
//           return vertsStartCount;
//       }
//
//       private void CreateGameObject(/*BuildingType kind,*/ MeshData data, GameObject main)
//       {
//           var go = new GameObject("VMG Buildings");
//           var mesh = go.AddComponent<MeshFilter>().mesh;
//           go.AddComponent<MeshRenderer>();
//           mesh.vertices = data.Vertices.ToArray();
//           mesh.triangles = data.Indices.ToArray();
//           mesh.SetUVs(0, data.UV);
//           mesh.RecalculateNormals();
//           //go.GetComponent<MeshRenderer>().material = FactorySettings.GetSettingsFor<BuildingSettings>(kind).Material;
//           go.transform.position += Vector3.up * 1;
//           go.transform.SetParent(main.transform, false);
//           var col = go.AddComponent<MeshCollider>();
//           col.convex = true;
//           col.isTrigger = true;
//
//           go.tag = "boardbuilding";
//           go.AddComponent<ObjectTapHandler>();
//
//       }
    }
}