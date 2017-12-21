using Assets.Scripts;
using MapzenGo.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

public class GetHeightData : MonoBehaviour
{
  //  string[] arrayValuesString = new string[65 * 65];
  //  int[,] arrayValues = new int[65, 65];
  //  Vector3[] vertices = new Vector3[65 * 65];
  //  int counter = 0;
  //  int index = 0;
  //
  //  // To normalize the extreme heightnumbers
  //  private float scaleFactor = 100f;
  //  Mesh mesh;
  //
  //  // Use this for initialization
  //  void Start()
  //  {
  //      MeshFilter mf = GetComponent<MeshFilter>();
  //
  //      // Only works for squared meshes (like tiles).
  //      transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.y);
  //
  //      // Generates filename based on zoom and tile values.
  //      var zoom = GetComponentInParent<Tile>().Zoom;
  //      var x = GetComponentInParent<Tile>().TileTms.x;
  //      var y = GetComponentInParent<Tile>().TileTms.y;
  //      y = Math.Pow(2, zoom) - y - 1;
  //      var fileName = zoom.ToString() + "/" + x.ToString() + "/" + y.ToString();
  //      StartCoroutine(ConvertFile(fileName));
  //  }
  //
  //  public IEnumerator ConvertFile(string fileName)
  //  {
  //      //var url = @"http://127.0.0.1:8080/1655628.terrain";
  //      var url = "http://" + AppState.Instance.Config.HeightServer + "/" + fileName + ".terrain";
  //      var sb = new StringBuilder();
  //      string line;
  //
  //      //WORKS ONLY IN EDITOR: //using (Stream stream = GetStreamFromUrl(@"http://134.221.20.240:3999/tiles/" + fileName + ".terrain"))
  //      using (var www = UnityWebRequest.Get(url))
  //      {
  //          yield return www.Send();
  //          if (www.isNetworkError || www.isHttpError)
  //          {
  //              Debug.Log(www.error + " (Possible no height data available at " + url + ")");
  //             // BoardInteraction.Instance.terrainHeightsAvailable = false;
  //          }
  //          else
  //          {
  //              //TODO : save to zipfilepath
  //        //      string zipfilePath = Application.temporaryCachePath + "/args.zip";
  //        //      string exportLocation = Application.temporaryCachePath + "/dir";
  //        //      ZipUtil.Unzip(zipfilePath, exportLocation);
  //
  //             // BoardInteraction.Instance.terrainHeightsAvailable = true;
  //              byte[] results = www.downloadHandler.data;
  //              using (var stream = new MemoryStream(results))
  //              using (var binaryStream = new BinaryReader(stream))
  //              {
  //                  var i = 0;
  //                  var j = 0;
  //                  var pos = 0;
  //
  //                  // Hardcoded as .length doesn't work on WebRequest.Create 
  //                  // TODO: Dont hardcode.
  //                  var streamLength = 65 * 65 * 2;
  //                  //var streamLength = (int)binaryStream.BaseStream.Length;
  //                  var minStreamLength = 65 * 65 * 2;
  //
  //                  if (streamLength < minStreamLength)
  //                  {
  //                      throw new Exception("File length is too short: length is " + streamLength);
  //                  }
  //
  //                  var length = Math.Min(minStreamLength, streamLength); // Limit to the 65x65 terrain info, skip rest
  //                  while (pos < length)
  //                  {
  //                      var height = binaryStream.ReadInt16();
  //                      sb.AppendLine(string.Format("Index ({0:D2}, {1}): {2}", i, j++, height / 5 - 1000)); // convert to height
  //                      if (j >= 65)
  //                      {
  //                          j = 0;
  //                          i++;
  //                      }
  //                      pos += 2;
  //                  }
  //
  //                  // Fills string array with 1 full line per entry: 1 height value for each vertice.
  //                  StringReader strReader = new StringReader(sb.ToString());
  //                  for (int k = 0; k < arrayValuesString.Length; k++)
  //                  {
  //                      line = strReader.ReadLine();
  //                      arrayValuesString[k] = line;
  //                      vertices[k] = new Vector3(k, float.Parse(GetString(line)), k);
  //                  }
  //              }
  //             GenerateMesh();
  //          }
  //      }
  //  }
  //
  //  //  private static Stream GetStreamFromUrl(string url)
  //  //  {
  //  //      Debug.Log(url);
  //  //      byte[] imageData = null;
  //  //
  //  //
  //  //      using (var wc = new System.Net.WebClient())
  //  //          imageData = wc.DownloadData(url);
  //  //
  //  //      return new MemoryStream(imageData);
  //  //  }
  //
  //  // public void ReadFile(string fileName)
  //  // {
  //  //     string line;
  //  //
  //  //     StreamReader file = new StreamReader(WebRequest.Create(@"http://127.0.0.1:8080/" + fileName + ".txt").GetResponse().GetResponseStream());
  //  //     //StreamReader file = new StreamReader(WebRequest.Create(@"http://134.221.20.240:3999/tiles/" + fileName + ".txt").GetResponse().GetResponseStream());
  //  //     //StreamReader file = new StreamReader(@"http://127.0.0.1:8080/" + fileName + ".txt");
  //  //  //   Debug.Log(@"http://127.0.0.1:8080/" + fileName + ".txt");
  //  //
  //  //     // Fills string array with 1 full line per entry
  //  //     for (int i = 0; i < arrayValuesString.Length; i++)
  //  //     {
  //  //         line = file.ReadLine();
  //  //         arrayValuesString[i] = line;
  //  //         vertices[i] = new Vector3(i, float.Parse(GetString(line)), i);
  //  //     }
  //  // }
  //  //
  //
  //  public string GetString(string input)
  //  {
  //      int lastIndex = input.LastIndexOf(' ');
  //      var output = input.Substring(lastIndex + 1);
  //      return output;
  //  }
  //
  //  public void GenerateMesh()
  //  {
  //      mesh = GetComponent<MeshFilter>().mesh;
  //      Vector2[] uvs = new Vector2[vertices.Length];
  //
  //      for (int x = 0; x < 65; x++)
  //      {
  //          for (int z = 0; z < 65; z++)
  //          {
  //              // Sets each spot in the vertices index (0,0 to 64,64) with the heightnumbers.
  //              //vertices[index++] = new Vector3(x, z, int.Parse(GetString(arrayValuesString[counter])) / scaleFactor);
  //              vertices[index] = new Vector3(x, z, -int.Parse(GetString(arrayValuesString[counter])) / scaleFactor);
  //              uvs[index++] = new Vector2(x / 64.0f, z / 64.0f);
  //              counter++;
  //          }
  //      }
  //
  //      // Creates two triangles for each quad
  //      int[] triangles = new int[64 * 64 * 2 * 3]; // 64x64 quads, 2 triangles per quad, 3 vertices per triangle
  //      index = 0;
  //      for (int x = 0; x < 64; x++)
  //      {
  //          for (int z = 0; z < 64; z++)
  //          {
  //              // Triangle 1 of quad
  //              triangles[index++] = x + z * 65;
  //              triangles[index++] = x + 1 + z * 65; // Swap with line below to change triangle front face
  //              triangles[index++] = x + 1 + (z + 1) * 65;
  //              // Triangle 2 of quad
  //              triangles[index++] = x + z * 65;
  //              triangles[index++] = x + 1 + (z + 1) * 65; // Swap with line below to change triangle front face
  //              triangles[index++] = x + (z + 1) * 65;
  //          }
  //      }
  //
  //      mesh.vertices = vertices;
  //      mesh.triangles = triangles;
  //      mesh.uv = uvs;
  //      mesh.RecalculateBounds();
  //      mesh.RecalculateNormals();
  //      mesh.RecalculateTangents();
  //
  //      var newCol = gameObject.AddComponent<MeshCollider>();
  //      newCol.sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
  //  }
}
