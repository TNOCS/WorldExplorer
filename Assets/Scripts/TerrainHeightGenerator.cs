using Assets.Scripts;
using MapzenGo.Models;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainHeightGenerator : MonoBehaviour
{

    [SerializeField]
    private int zoomLevel = 5;
    [SerializeField]
    private int resolution = 32;
    [SerializeField]
    private bool useNormalMap = true;

    [SerializeField]
    private Texture2D elevationTexture;
    [SerializeField]
    private Texture2D normalTexture;

    [SerializeField]
    private double tileTmsX;
    [SerializeField]
    private double tileTmsY;

    [SerializeField]
    private string url = "http://127.0.0.1:8080";
    
    private void Start()
    {
        // Only works for squared meshes (like tiles)
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.y);
   //
   // var zoom = AppState.Instance.Config.ActiveView.Zoom;
   // tileTmsX = GetComponentInParent<Tile>().TileTms.x;
   // tileTmsY = GetComponentInParent<Tile>().TileTms.y;

        //elevationTexture = Resources.Load("Heightmaps/elevation") as Texture2D;
       //   elevationTexture = Resources.Load("Heightmaps/" + zoom + "/" + tileTmsX + "-" + tileTmsY) as Texture2D;
        //  Debug.Log("Loading texture " + elevationTexture + " from " + "Heightmaps/" + zoom + "/" + tileTmsX + "-" + tileTmsY);
        //
          //normalTexture = Resources.Load("Heightmaps/normals") as Texture2D;

        //var tileUrl = url + "/" + zoom + "/" + tileTmsX + "/" + tileTmsY;
     // var tileUrl = "www.thomvdm.com/converted.txt";
     // ObservableWWW.GetWWW(tileUrl).Subscribe(
     //        success =>
     //        {
     //            Debug.Log("Success retrieving tile from " + tileUrl);
     //            CreateMesh();
     //
     //        },
     //        error =>
     //        {
     //            Debug.Log(tileUrl + " - " + error);
     //        });
     //
        // When this GameObject is created, create an elevation mesh.
        CreateMesh();
    }

    public void SetUseNormalMap(bool use)
    {
        // Update the mesh to use or disable the normal map.
        useNormalMap = use;
        CreateMesh();
    }

    public void SetMeshResolution(float resolution)
    {
        // Update the mesh to the nearest power-of-two resolution.
        int r = (int)Math.Pow(2.0f, Math.Floor(Math.Log(resolution, 2.0f)));
        if (this.resolution != r)
        {
            this.resolution = r;
            CreateMesh();
        }
    }

    // Generate a uniform grid of vertices, elevate them according to a texture, and apply a normal map.
    private void CreateMesh()
    {
        // Get the mesh for this GameObject.
        var mesh = GetComponent<MeshFilter>().mesh;

        // Generate a vertex grid for the mesh. The offset vector makes the final mesh centered on 0 in X and Z.
        GenerateElevationGrid(mesh, resolution, new Vector3(-0.5f, 0.0f, -0.5f));

        // Elevate the vertices of the mesh.
        ApplyElevation(mesh);

        // Apply or remove the normal map from our material, based on the option set in the editor.
        var material = GetComponent<MeshRenderer>().material;
        if (useNormalMap)
        {
            ApplyNormalTexture(material);
        }
        else
        {
            RemoveNormalTexture(material);
            // When the normal map is not used, use approximate normals calculated at vertices.
            mesh.RecalculateNormals();
        }
    }

    // Runs once per frame.
    public void Update()
    {
        // When the mouse button is held, use the mouse position to rotate the model.
        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up, -Input.GetAxis("Mouse X") * 10.0f);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            CreateMesh();
        }
    }

    // Generate a uniform 2D grid of vertices for the mesh, with resolution+1 vertices on each side.
    // The vertices span 0.0-1.0 in X and Z by default. The offset vector is added to every vertex position.
    public void GenerateElevationGrid(Mesh mesh, int resolution, Vector3 offset)
    {
        // Create fresh, empty lists for all of the vertex values we need to set.
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var colors = new List<Color>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();

        // Iterate over the rows and columns of a grid in X and Z.
        int index = 0;
        for (int col = 0; col <= resolution; col++)
        {
            float y = (float)col / resolution;
            for (int row = 0; row <= resolution; row++)
            {
                float x = (float)row / resolution;

                // Add the values for a new vertex.
                vertices.Add(new Vector3(x, 0, y) + offset);
                colors.Add(Color.white);
                uvs.Add(new Vector2(x, y));

                // We add the 'up' vector as the normal for every vertex so that when we apply our normal map,
                // it will be effectively interpreted as 'object space' normals rather than 'tangent space'.
                normals.Add(Vector3.up);

                // Similar to the normals, we add the 'right' vector as the tangent for every vertex so that
                // our added normal map will be treated like 'object space'. The 4th value determines the sign
                // of cross product used to calculate the binormal, for us it happens to be -1.
                tangents.Add(new Vector4(1, 0, 0, -1));

                // Add indices for form triangles between this vertex and its neighbors left and down, unless
                // we're at the end of a column or row.
                if (row < resolution && col < resolution)
                {
                    indices.Add(index);
                    indices.Add(index + resolution + 1);
                    indices.Add(index + 1);

                    indices.Add(index + 1);
                    indices.Add(index + resolution + 1);
                    indices.Add(index + resolution + 2);
                }

                index++;
            }
        }

        // Clear the previous values from the mesh and set our newly made values.
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
    }

    public void ApplyElevation(Mesh mesh)
    {
        // Iterate over the vertex positions and UVs of the mesh.
        var vertices = mesh.vertices;
        var uvs = mesh.uv;
        for (int i = 0; i < vertices.Length; i++)
        {
            // At each vertex, sample the elevation texture at the corresponding UV coordinate.
            int x = Convert.ToInt32(uvs[i].x * elevationTexture.width);
            int y = Convert.ToInt32(uvs[i].y * elevationTexture.height);
            Color color = elevationTexture.GetPixel(x, y);
            

            // Convert the resulting color value to an elevation in meters.
            float elevation = ColorToElevation(color);
            Debug.Log(elevation);
            // Use the tile size in meters at the given zoom level to determine the relative
            // scale of elevation values in the mesh.
            const double earthCircumferenceMeters = 6378137.0 * Math.PI * 2.0;
            double tileSize = earthCircumferenceMeters / (1 << zoomLevel);
            double height = elevation / tileSize;
            vertices[i].y = (float)height;
            Debug.Log(height);
        }
        // Assign the new vertex positions to the mesh.
        mesh.vertices = vertices;
    }

    public void ApplyNormalTexture(Material material)
    {
        // https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
        material.EnableKeyword("_NORMALMAP");
        material.SetTexture("_BumpMap", normalTexture);
    }

    public void RemoveNormalTexture(Material material)
    {
        material.DisableKeyword("_NORMALMAP");
    }

    public static float ColorToElevation(Color color)
    {
        // Convert from color channel values in 0.0-1.0 range to elevation in meters:
        // https://mapzen.com/documentation/terrain-tiles/formats/#terrarium
        return (color.r * 256.0f * 256.0f + color.g * 256.0f + color.b) - 32768.0f;
    }
}
