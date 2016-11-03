using Assets.Scripts;
using Assets.Scripts.Classes;
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
                if (tileLayer.Type != "tilelayer" || tileLayer.Enabled == false) continue;
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                go.name = "tilelayer-" + tileLayer.Title;
                go.SetParent(tile.transform, true);
                go.localScale = new Vector3((float)tile.Rect.Width, (float)tile.Rect.Width, 1);
                go.rotation = Quaternion.AngleAxis(90, new Vector3(1, 0, 0));
                go.localPosition = Vector3.zero;
                go.localPosition += new Vector3(0, tileLayer.Height, 0);
                var rend = go.GetComponent<Renderer>();
                rend.material = tile.Material;

                var url = string.Format("{0}/{1}/{2}/{3}.png", tileLayer.Url, tile.Zoom, tile.TileTms.x, tile.TileTms.y);
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
