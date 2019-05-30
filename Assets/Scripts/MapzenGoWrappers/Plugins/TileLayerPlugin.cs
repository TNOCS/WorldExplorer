using Assets.Scripts;
using Assets.Scripts.Classes;
using MapzenGo.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    /// <summary>
    /// Renders the OSM, GOOGLE, etc. tiles
    /// </summary>
    public class TileLayerPlugin : TilePlugin
    {


        public const int TerrainLayer = 8; //LayerMask.GetMask("board");


        public ViewState ActiveView;
        readonly string objectJSONString;

        private readonly string mTileQuantizedHeightMeshUrlTemplate;

        public TileLayerPlugin()
        {
            try
            {
                Uri baseUri = new Uri(AppState.Instance.BaseUrlConfigurationServer);
                Uri terrainHeightUri = new Uri(baseUri, "api/QuantizedMesh/{zoom}/{x}/{y}.terrain");
                mTileQuantizedHeightMeshUrlTemplate = terrainHeightUri.ToString();
                Debug.LogError($"QuantizedMesh tile url template is '{mTileQuantizedHeightMeshUrlTemplate}'.");
            }
            catch (Exception)
            {
                mTileQuantizedHeightMeshUrlTemplate = "";
                Debug.LogError($"Failed to construct QuantizedMesh tile height url.");
            }
        }

        public override void GeoJsonDataLoaded(Tile tile)
        {
            // Do nothing
        }

        public override void TileCreated(Tile tile)
        {
            CreateTile(tile);

        }

        private async void CreateTile(Tile tile)
        {
            //if (Application.isEditor) return;

            if (ActiveView.RasterTileLayers.Count == 0) Debug.Log($"No tile layers found");
            foreach (var tileLayer in ActiveView.RasterTileLayers)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                go.name = "tilelayer-" + tileLayer.LayerId;
                go.SetParent(tile.transform, false);
                //go.localRotation = Quaternion.Euler(90, 0, 0);
                go.localRotation = Quaternion.Euler(0, 0, 0);

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



                // Sets terrain heights for each tile.
                if (BoardInteraction.Instance.terrainHeights && AppState.Instance.Config.ActiveView.Name == "Compound")
                {

                }
                else
                {
                    float offset = (float)tile.Rect.Width / 2.0f * -1;
                    //go.localPosition += new Vector3(0, tileLayer.Height, 0);
                    go.localPosition += new Vector3(offset, /* tileLayer.Height */ 0.0f, offset);
                    //go.localScale = new Vector3((float)tile.Rect.Width, (float)tile.Rect.Width, 1);
                    go.localScale = new Vector3((float)tile.Rect.Width, 2000 /* height terrain*/, (float)tile.Rect.Width);
                }

                string meshName = $"HeightMesh-{zoom}-{tile.TileTms.x}-{tile.TileTms.y}";
                var defaultSimpleMesh = QuantizedMeshCreator.CreateEmptyQuad(meshName);
                go.GetComponent<MeshFilter>().mesh = defaultSimpleMesh;
                go.GetComponent<MeshCollider>().sharedMesh = defaultSimpleMesh;
                go.gameObject.tag = "board";
                go.gameObject.layer = TerrainLayer;
                rend.material = tile.Material;
                Uri baseUri = new Uri(AppState.Instance.BaseUrlConfigurationServer);
                Uri uri = new Uri(baseUri, $"api/RasterTile/{tileLayer.LayerId}/{zoom}/{tile.TileTms.x}/{tile.TileTms.y}");
                // download tile texture
                try
                {
                    Debug.Log($"Downloading raster image for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} (url={uri.ToString()}).");
                    byte[] image = await NetworkUtil.DownloadDataAsync(CancellationToken.None, uri.ToString());
                    File.WriteAllBytes("c:\\temp\\hein.png", image);
                    Debug.Log($"Downloaded raster image for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y}");

                    if (rend)
                    {
                        rend.material.mainTexture = new Texture2D(512, 512, TextureFormat.DXT5, false);
                        rend.material.color = new Color(1f, 1f, 1f, 1f);
                        ((Texture2D)rend.material.mainTexture).LoadImage(image);

                    }
                    Debug.Log($"Background image for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} applied");
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to download {uri.ToString()} : {ex.Message}");
                }
                // download quantized mesh


                var quantizedMeshUrl = mTileQuantizedHeightMeshUrlTemplate
                    .Replace("{zoom}", tile.Zoom.ToString())
                    .Replace("{x}", tile.TileTms.x.ToString())
                    .Replace("{y}", tile.TileTms.y.ToString());
                // $"http://localhost:56367/api/QuantizedMesh/{zoom}/{x}/{y}.terrain";

                Debug.Log($"Download heightmap for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} url: {quantizedMeshUrl} ");

                try
                {
                    string tileID = $"{tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y}";
                    Mesh terrainMesh = AppState.Instance.TileManager.GetHeightMeshFromCache(tileID);
                    if (terrainMesh == null)
                    {
                        byte[] heightMesh = await NetworkUtil.DownloadDataAsync(CancellationToken.None, quantizedMeshUrl);

                        using (var stream = new MemoryStream(heightMesh))
                        {

                            terrainMesh = QuantizedMeshCreator.CreateMesh(stream, meshName);
                            // terrainMesh = QuantizedMeshCreator.CreateEmptyQuad(meshName);
                        }
                        AppState.Instance.TileManager.CacheMesh(tileID, terrainMesh);
                    }

                    if (go != null)
                    {
                        go.GetComponent<MeshFilter>().mesh = terrainMesh;
                        go.GetComponent<MeshCollider>().sharedMesh = terrainMesh;
                    }
                    Debug.Log($"Height mesh for tile {tile.Zoom}-{tile.TileTms.x}-{tile.TileTms.y} applied");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to download/apply height mesh '{quantizedMeshUrl}'.");
                }
                AppState.Instance.TileManager.HeightTileLoaded(tile);

            }
            #region TerrainHeights

            /*
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
            */
            #endregion
        }
    }
}