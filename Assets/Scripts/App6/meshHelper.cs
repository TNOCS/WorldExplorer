using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using System.Linq;



public class MeshHelpers
{





    public static void CreateMesh(List<Vector3> corners, float height, List<Vector3> verts, List<int> indices)

    {

        var tris = new Triangulator(corners);

        var vertsStartCount = verts.Count;

        verts.AddRange(corners.Select(x => new Vector3(x.x, height, x.z)).ToList());

        indices.AddRange(tris.Triangulate().Select(x => vertsStartCount + x));



        // if (typeSettings.IsVolumetric)

        {



            Vector3 v1;

            Vector3 v2;

            int ind = 0;

            for (int i = 1; i < corners.Count; i++)

            {

                v1 = verts[vertsStartCount + i - 1];

                v2 = verts[vertsStartCount + i];

                ind = verts.Count;

                verts.Add(v1);

                verts.Add(v2);

                verts.Add(new Vector3(v1.x, 0, v1.z));

                verts.Add(new Vector3(v2.x, 0, v2.z));



                indices.Add(ind);

                indices.Add(ind + 2);

                indices.Add(ind + 1);



                indices.Add(ind + 1);

                indices.Add(ind + 2);

                indices.Add(ind + 3);

            }



            v1 = verts[vertsStartCount];

            v2 = verts[vertsStartCount + corners.Count - 1];

            ind = verts.Count;

            verts.Add(v1);

            verts.Add(v2);

            verts.Add(new Vector3(v1.x, 0, v1.z));

            verts.Add(new Vector3(v2.x, 0, v2.z));



            indices.Add(ind);

            indices.Add(ind + 1);

            indices.Add(ind + 2);



            indices.Add(ind + 1);

            indices.Add(ind + 3);

            indices.Add(ind + 2);

        }

    }



    public static GameObject CreateGameObject(string name, List<Vector3> vertices, List<int> indices, GameObject main, Material material)

    {

        var go = new GameObject(name);

        AddMeshToGameObject(go, vertices, indices, material);

        //go.transform.position += Vector3.up * Order;

        go.transform.SetParent(main.transform, false);



        return go;

    }



    public static void AddMeshToGameObject(GameObject go, List<Vector3> vertices, List<int> indices, Material material)

    {

        var mesh = go.AddComponent<MeshFilter>().mesh;

        go.AddComponent<MeshRenderer>();

        mesh.vertices = vertices.ToArray();

        mesh.triangles = indices.ToArray();

        mesh.RecalculateNormals();

        go.GetComponent<MeshRenderer>().material = material;// _settings.GetSettingsFor(kind).Material;



    }



}