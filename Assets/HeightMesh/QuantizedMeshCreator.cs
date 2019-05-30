using System;
using System.IO;
using UnityEngine;

public class QuantizedMeshCreator
{

    public const float MinimumHeight = -10; /* lowest point in Netherlands*/
    public const float MaximumHeight = 350; /* heighest point in Netherlands */

    public static Mesh CreateMesh(Stream pStream, String pName)
    {
        try
        {

            var decoder = new QuantizedMeshFormatDecoder(pStream, false);
            Vector3[] terrainVertices = new Vector3[decoder.VertexList.Length];
            for (int index = 0; index < terrainVertices.Length; index++)
            {
                float heightInMeter = decoder.GetHeightInMeter(index);
                float normalizedHeight = Mathf.InverseLerp(MinimumHeight, MaximumHeight, heightInMeter);

                terrainVertices[index] = new Vector3(
                    (float)QuantizedMeshFormatDecoder.Normalize(decoder.VertexList[index].u),
                                normalizedHeight,
                                (float)QuantizedMeshFormatDecoder.Normalize(decoder.VertexList[index].v));
            }

            int[] terrainTriangles = new int[decoder.TriangleIndices.Length];
            for (int index = 0; index < terrainTriangles.Length / 3; index++)
            {
                // Inverse traingle direction (frontface / backface)
                int offset = (index * 3);
                terrainTriangles[offset + 0] = (int)decoder.TriangleIndices[offset + 0];
                terrainTriangles[offset + 1] = (int)decoder.TriangleIndices[offset + 2];
                terrainTriangles[offset + 2] = (int)decoder.TriangleIndices[offset + 1];
            }

            Vector2[] terrainUV = new Vector2[decoder.VertexList.Length];
            for (int index = 0; index < terrainVertices.Length; index++)
            {
                terrainUV[index] = new Vector2(
                    (float)QuantizedMeshFormatDecoder.Normalize(decoder.VertexList[index].u),
                    (float)QuantizedMeshFormatDecoder.Normalize(decoder.VertexList[index].v));
            }

            // Replace quad mesh with own mesh

            Mesh terreinMesh = new Mesh()
            {
                name = pName,
                vertices = terrainVertices,
                triangles = terrainTriangles,
                uv = terrainUV
            };


            terreinMesh.RecalculateBounds();
            terreinMesh.RecalculateNormals(); // Could also be extracted from terrain file; but this is easer

            return terreinMesh;

        }
        catch (Exception ex)
        {
            Debug.LogError("Corrupt mesh stream, use default quad; error: " + ex.Message);
            Mesh terreinMesh = CreateEmptyQuad(pName + "_corruptstream");
            return terreinMesh;
        }
    }

    public static Mesh CreateEmptyQuad(String pName)
    {
        Vector3[] vertices = new Vector3[4]
    {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1)
    };
        Vector3[] normals = new Vector3[4]
    {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
    };
        Vector2[] uv = new Vector2[4]
            {

        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };
        int[] tri = new int[6] { 0, 2, 1, 2, 3, 1 };

        var mesh = new Mesh()
        {
            name = pName,
            vertices = vertices,
            triangles = tri,
            normals = normals,
            uv = uv
        };
        return mesh;
    }
}
