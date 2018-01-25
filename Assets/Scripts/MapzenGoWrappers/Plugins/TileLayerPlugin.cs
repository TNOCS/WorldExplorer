using Assets.Scripts;
using Assets.Scripts.Classes;
using MapzenGo.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace MapzenGo.Models.Plugins
{
    public class TileLayerPlugin : Plugin
    {
        /// <summary>
        /// Handles the spawning of tiles, as well as the terrain heights if the user has enabled them.
        /// </summary>

        public List<Layer> tileLayers;
        string objectJSONString;

        public override void Create(Tile tile)
        {
            base.Create(tile);
            foreach (var tileLayer in tileLayers)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                go.name = "tilelayer-" + tileLayer.Title;
                go.SetParent(tile.transform, false);
                go.localRotation = Quaternion.Euler(90, 0, 0);

                var rend = go.GetComponent<Renderer>();
                var CenterInMercator = GM.TileBounds(tile.TileTms, tile.Zoom).Center;
                var v0 = new Vector2d(tile.transform.position.x, tile.transform.position.z) + CenterInMercator;
                var v1 = GM.MetersToLatLon(v0);

                go.gameObject.AddComponent<BoardTapHandler>();

                // Generates filename and url for terrain heights based on zoom and tile values.
                var zoom = tile.Zoom;
                var x = tile.TileTms.x;
                var y = tile.TileTms.y;
                y = Math.Pow(2, zoom) - y - 1;  // Slippy to TMS.

                var fileName = zoom.ToString() + "/" + x.ToString() + "/" + y.ToString();
                var terrainUrl = "http://" + AppState.Instance.Config.HeightServer + "/" + fileName + ".terrain";

                // Sets terrain heights for each tile.
                if (BoardInteraction.Instance.terrainHeights && AppState.Instance.Config.ActiveView.Name == "Compound")
                {
                    StartCoroutine(CheckIfHeightsAvailable(terrainUrl, tile, tileLayer, go));
                }
                else
                {
                    go.localPosition += new Vector3(0, tileLayer.Height, 0);
                    go.localScale = new Vector3((float)tile.Rect.Width, (float)tile.Rect.Width, 1);
                }

                go.gameObject.tag = "board";
                go.gameObject.layer = 8;
                rend.material = tile.Material;

                var url = tileLayer.Url.Replace("{z}", tile.Zoom.ToString()).Replace("{x}", tile.TileTms.x.ToString()).Replace("{y}", tile.TileTms.y.ToString());
                ObservableWWW.GetWWW(url).Subscribe(
                    success =>
                    {
                        if (rend)
                        {
                            rend.material.mainTexture = new Texture2D(512, 512, TextureFormat.DXT5, false);
                            success.LoadImageIntoTexture((Texture2D)rend.material.mainTexture);

                            // Adds a shader that allows the mesh to be viewed from all sides. Only necessary when terrain is leveled.
                            if (BoardInteraction.Instance.terrainHeights)
                            {
                                // This plugin gets destroyed upon reload, so the reference of the shader has to be saved somewhere else.
                                if (BoardInteraction.Instance.twoSidedShader == null)
                                {
                                    //  BoardInteraction.Instance.twoSidedShader = Resources.Load("Shaders/FastConfigurable/Shaders/FastConfigurable2Sided", typeof(Shader)) as Shader;
                                }
                                //go.gameObject.GetComponent<Renderer>().material.shader = BoardInteraction.Instance.twoSidedShader;
                            }

                        }
                    },
                    error =>
                    {
                        Debug.Log(error + " loading url: " + url);
                    }
                );

            }
        }

        #region TerrainHeights
        private IEnumerator CheckIfHeightsAvailable(string url, Tile tile, Layer tileLayer, Transform go)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error + " (Possible no height data available at " + url + ")");
                    go.localPosition += new Vector3(0, tileLayer.Height, 0);
                    go.localScale = new Vector3((float)tile.Rect.Width, (float)tile.Rect.Width, 1);
                }
                else
                {
                    StartCoroutine(ConvertFile(url, go));
                    SetTransform(go, tile);
                }
            }
        }

        public IEnumerator ConvertFile(string url, Transform go)
        {
            var sb = new StringBuilder();
            string line;
            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.Send();
                {
                    byte[] results = www.downloadHandler.data;
                    using (var stream = new MemoryStream(results))
                    using (var binaryStream = new BinaryReader(stream))
                    {
                        var i = 0;
                        var j = 0;
                        var pos = 0;

                        var length = Math.Min(65 * 65 * 2, (int)binaryStream.BaseStream.Length); // Limit to the 65x65 terrain info, skip rest
                        while (pos < length)
                        {
                            var height = binaryStream.ReadInt16();
                            sb.AppendLine(string.Format("Index ({0:D2}, {1}): {2}", i, j++, height / 5 - 1000)); // convert to height
                            if (j >= 65)
                            {
                                j = 0;
                                i++;
                            }
                            pos += 2;
                        }
                        // Fills string array with 1 full line per entry: 1 height value for each vertice.
                        StringReader strReader = new StringReader(sb.ToString());
                        string[] arrayValuesString = new string[65 * 65];
                        Vector3[] vertices = new Vector3[65 * 65];

                        for (int k = 0; k < arrayValuesString.Length; k++)
                        {
                            line = strReader.ReadLine();
                            arrayValuesString[k] = line;
                            vertices[k] = new Vector3(k, float.Parse(GetString(line)), k);
                        }
                        GenerateMesh(go, arrayValuesString, vertices);
                    }
                }
            }
        }

        public string GetString(string input)
        {
            int lastIndex = input.LastIndexOf(' ');
            var output = input.Substring(lastIndex + 1);
            return output;
        }

        public void GenerateMesh(Transform go, string[] arrayValuesString, Vector3[] vertices)
        {
            int counter = 0;
            int index = 0;
            Mesh mesh;
            var scaleFactor = 1.5f;
            mesh = go.gameObject.GetComponent<MeshFilter>().mesh;
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int x = 0; x < 65; x++)
            {
                for (int z = 0; z < 65; z++)
                {
                    // Sets each spot in the vertices index (0,0 to 64,64) with the heightnumbers.
                    //vertices[index++] = new Vector3(x, z, int.Parse(GetString(arrayValuesString[counter])) / scaleFactor);

                    // Careful: enabling this log freezes Unity for about a minute.
                    //Debug.Log("Tile: " + go.parent.name + " " + index + " X: " + x + " z: " + z + " height: " + (int.Parse(GetString(arrayValuesString[counter]).ToString()) + "counter: " + counter));
                    vertices[index] = new Vector3(x, z, int.Parse(GetString(arrayValuesString[counter])) / scaleFactor);
                    uvs[index++] = new Vector2(x / 64.0f, z / 64.0f);
                    counter++;
                }
            }

            // Creates two triangles for each quad
            int[] triangles = new int[64 * 64 * 2 * 3]; // 64x64 quads, 2 triangles per quad, 3 vertices per triangle
            index = 0;
            for (int x = 0; x < 64; x++)
            {
                for (int z = 0; z < 64; z++)
                {
                    // Triangle 1 of quad
                    triangles[index++] = x + z * 65;
                    triangles[index++] = x + 1 + z * 65; // Swap with line below to change triangle front face
                    triangles[index++] = x + 1 + (z + 1) * 65;
                    // Triangle 2 of quad
                    triangles[index++] = x + z * 65;
                    triangles[index++] = x + 1 + (z + 1) * 65; // Swap with line below to change triangle front face
                    triangles[index++] = x + (z + 1) * 65;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            var newCol = go.gameObject.AddComponent<MeshCollider>();
            newCol.sharedMesh = go.gameObject.GetComponent<MeshFilter>().mesh;
        }

        public void SetTransform(Transform go, Tile tile)
        {
            // Only works for squared meshes (like tiles).
            go.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.y);

            // Fix to scale and position tiles correctly.
            // The tile and tilelayer scaling and positioning breaks when generating a new mesh instead of using the quad primitive. 
            // Either fix it like this or find a way to keep the original Quad mesh scaling and positioning for the new mesh with correct vertex aligning.
            var scaleFactor = 19.4f;
            var positionFactor = -600;
            var heightFactor = 700;
            switch (tile.Zoom)
            {
                case 15:
                    go.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    go.transform.localPosition = new Vector3(positionFactor, 700, positionFactor);
                    break;
                case 16:
                    go.localScale = new Vector3(scaleFactor / 2, scaleFactor / 2, scaleFactor / 3);
                    go.transform.localPosition = new Vector3(positionFactor / 2, 351, positionFactor / 2);
                    break;
                case 17:
                    go.localScale = new Vector3(scaleFactor / 4, scaleFactor / 4, scaleFactor / 6);
                    go.transform.localPosition = new Vector3(positionFactor / 4, 182, positionFactor / 4);
                    break;
                case 18:
                    go.localScale = new Vector3(scaleFactor / 8, scaleFactor / 8, scaleFactor / 12);
                    go.transform.localPosition = new Vector3(positionFactor / 8, 93, positionFactor / 8);
                    break;
                case 19:
                    go.localScale = new Vector3(scaleFactor / 16, scaleFactor / 16, scaleFactor / 24);
                    go.transform.localPosition = new Vector3(positionFactor / 16, heightFactor / 45, positionFactor / 16);
                    break;
            }
        }
        #endregion
    }
}