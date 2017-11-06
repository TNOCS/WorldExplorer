using Assets.Scripts;
using Assets.Scripts.Classes;
using MapzenGo.Helpers;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MapzenGo.Models.Plugins
{
    public class TileLayerPlugin : Plugin
    {
        public List<Layer> tileLayers;

        public override void Create(Tile tile)
        {
            base.Create(tile);

            foreach (var tileLayer in tileLayers)
            {
                //AppState.Instance.Speech.Keywords.Add("Enable " + tileLayer.VoiceCommand, () =>
                // {
                //     if (tileLayer.Group())
                // });

                var go = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                go.name = "tilelayer-" + tileLayer.Title;
                go.SetParent(tile.transform, false);
                go.localScale = new Vector3((float)tile.Rect.Width, (float)tile.Rect.Width, 1);
                //go.rotation = Quaternion.AngleAxis(90, new Vector3(1, 0, 0));
                go.localRotation = Quaternion.Euler(90, 0,0);
                go.localPosition += new Vector3(0, tileLayer.Height, 0);
                var rend = go.GetComponent<Renderer>();

                // TODO: perhaps disable or remove for release. Currently only used in inspector for debugging.
                // Sets the values of the four Quad corners, and the lon+lat values of each tile.
                // Data is contained in on the tile object itself (TileLayerData), and is logged in the inspector.
                var CenterInMercator = GM.TileBounds(tile.TileTms, tile.Zoom).Center;
                var v0 = new Vector2d(tile.transform.position.x, tile.transform.position.z) + CenterInMercator;
                var v1 = GM.MetersToLatLon(v0);
                //Debug.Log(string.Format(@"""loc"":{{""lat"":{0},""lon"":{1}}}", v1.y, v1.x));

                //go.gameObject.AddComponent<TerrainHeightGenerator>();
                go.gameObject.AddComponent<BoardTapHandler>();

                var col = go.gameObject.GetComponent<MeshCollider>();
                col.sharedMesh = go.gameObject.GetComponent<MeshFilter>().mesh;
                col.convex = true;
                go.gameObject.tag = "board";
                go.gameObject.layer = 8;


                go.gameObject.AddComponent<TilePositionData>();                
                var tileData = go.gameObject.GetComponent<TilePositionData>();
                tileData.lat = v1.y;
                tileData.lon = v1.x;
               

                rend.material = tile.Material;
                

                // var url = string.Format("{0}/{1}/{2}/{3}.png", tileLayer.Url, tile.Zoom, tile.TileTms.x, tile.TileTms.y);
                var url = tileLayer.Url.Replace("{z}", tile.Zoom.ToString()).Replace("{x}", tile.TileTms.x.ToString()).Replace("{y}", tile.TileTms.y.ToString());
                //Debug.Log(url);
                ObservableWWW.GetWWW(url).Subscribe(
                    success =>
                    {
                        if (rend)
                        {
                            rend.material.mainTexture = new Texture2D(512, 512, TextureFormat.DXT5, false);
                            success.LoadImageIntoTexture((Texture2D)rend.material.mainTexture);
                        }
                    },
                    error =>
                    {
                        Debug.Log(error);
                    }
                );
            }
        }
    }
}
