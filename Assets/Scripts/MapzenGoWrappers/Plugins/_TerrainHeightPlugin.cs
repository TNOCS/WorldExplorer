using System.Collections.Generic;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    public class _TerrainHeightPlugin : TilePlugin
    {
        public enum TileServices
        {
            Default,
            Luchtfoto,
            Osm,
            Satellite,
            Terrain,
            Toner,
            Watercolor
        }

        // Sample Terrarium URL: https://tile.mapzen.com/mapzen/terrain/v1/terrarium/11/1518/858.png?api_key=mapzen-9ePDqgc

        public TileServices TileService = TileServices.Default;
        [SerializeField]        
        protected string _key = "mapzen-9ePDqgc";
        private string[] TileServiceUrls = new string[] {
            "http://b.tile.openstreetmap.org/",
            "https://geodata1.nationaalgeoregister.nl/luchtfoto/wmts/luchtfoto_png/nltilingschema/",
            "http://b.tile.openstreetmap.org/",
            "http://b.tile.openstreetmap.us/usgs_large_scale/",
            "http://tile.stamen.com/terrain-background/",
            "http://a.tile.stamen.com/toner/",
            "https://stamen-tiles.a.ssl.fastly.net/watercolor/"
        };

        public override void GeoJsonDataLoaded(Tile tile)
        {
            
        }


        public override void TileCreated(Tile tile)
        {
          
            var zoomtmp = 11;
            var tmsx = 1518;
            var tmsy = 858;
            var url = "https://www.thomvdm.com/sampleheightmap.jpg";
            //var url = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/" + zoomtmp + "/" + tile.TileTms.x + "/" + tile.TileTms.y + ".png?api_key=" + _key;

            // var url = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/" + tile.Zoom + "/" + tile.TileTms.x + "/" +
            //tile.TileTms.y + ".png?api_key=" + _key;

           /* HKL  ObservableWWW.GetWWW(url).Subscribe(
                success =>
                {
                    CreateMesh(tile, success);
                },
                error =>
                {
                    Debug.Log(url + " - " + error);
                }); */
        }

        private void CreateMesh(Tile tile, WWW terrarium)
        {
            //var url = TileServiceUrls[(int)TileService] + tile.Zoom + "/" + tile.TileTms.x + "/" + tile.TileTms.y + ".png";
            var url = "https://www.thomvdm.com/sampleheightmap.jpg";
            const int sampleCount = 3;
            var tex = new Texture2D(256, 256);
            terrarium.LoadImageIntoTexture(tex);

            /* HKL ObservableWWW.GetWWW(url).Subscribe(
                success =>
                {
                    var go = new GameObject("TerrainHeight");
                    var mesh = go.AddComponent<MeshFilter>().mesh;
                    var rend = go.AddComponent<MeshRenderer>();
                    var verts = new List<Vector3>();

                    // When sampleCount == 3, compute 9 points: the four corners, the center, and the four midpoints along the side
                    // vertices are (all at the appropriate y = height):
                    // 0) minX, minY 
                    // 1) minX, halfY 
                    // 2) minX, maxY
                    // 3) halfX, minY 
                    // 4) halfX, halfY 
                    // 5) halfX, maxY
                    // 6) maxX, minY 
                    // 7) maxX, halfY 
                    // 8) maxX, maxY
                    for (float x = 0; x < sampleCount; x++)
                    {
                        for (float y = 0; y < sampleCount; y++)
                        {
                            // Lerp: Linearly interpolates between left and right corner.
                            var xx = Mathf.Lerp((float)tile.Rect.Min.x, (float)(tile.Rect.Min.x + tile.Rect.Size.x), x / (sampleCount - 1));
                            var yy = Mathf.Lerp((float)tile.Rect.Min.y, (float)(tile.Rect.Min.y + tile.Rect.Size.y), y / (sampleCount - 1));
                            // Clamp: Clamps value between min and max and returns value.
                            var px = (int)Mathf.Clamp((x / (sampleCount - 1) * 256), 0, 255);           // 0, 128, 255
                            var py = (int)Mathf.Clamp((256 - (y / (sampleCount - 1) * 256)), 0, 255);   // 255, 128, 0
                            // Compute relative vector with respect to the origin
                            verts.Add(new Vector3(
                                (float)(xx - tile.Rect.Center.x),
                                GetTerrariumHeight(tex.GetPixel(px, py)),
                                (float)(yy - tile.Rect.Center.y)));
                        }
                    }

                    mesh.SetVertices(verts);
                    // Create a mesh
                    mesh.triangles = new int[] { 0, 3, 4, 0, 4, 1, 1, 4, 5, 1, 5, 2, 3, 6, 7, 3, 7, 4, 4, 7, 8, 4, 8, 5 };
                    mesh.SetUVs(0, new List<Vector2>()
                    {
                        new Vector2(0, 1),
                        new Vector2(0, 0.5f),
                        new Vector2(0, 0),
                        new Vector2(0.5f, 1),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0),
                        new Vector2(1, 1),
                        new Vector2(1, 0.5f),
                        new Vector2(1, 0),
                    });
                    mesh.RecalculateNormals();
                    go.transform.SetParent(tile.transform, false);

                    rend.material.mainTexture = new Texture2D(512, 512, TextureFormat.DXT5, false);
                    rend.material.color = new Color(1f, 1f, 1f, 1f);
                    success.LoadImageIntoTexture((Texture2D)rend.material.mainTexture);
                },
                error =>
                {
                    Debug.Log(error);
                });
                */
        }

        private float GetTerrariumHeight(Color c)
        {
            return (c.r * 65536 + c.g * 256 + c.b) - 32768;
            //var h = (c.r * 256 * 256 + c.g * 256 + c.b) - 32768;
            //Debug.Log(string.Format("Height: {0}", h));
            //return h;
        }
    }
}