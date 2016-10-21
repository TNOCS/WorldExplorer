using System;
using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using UnityEngine;

namespace MapzenGo.Models
{
    public class DynamicTileManager : TileManager
    {
        //[SerializeField] private Rect _centerCollider;
        [SerializeField] private int _removeAfter;
        [SerializeField] private bool _keepCentralized;

        private static readonly AppState appState = AppState.Instance;

        public override void Start()
        {
            base.Start();

            _removeAfter = Math.Max(_removeAfter, Range * 2 + 1);
            //var rect = new Vector2(TileSize, TileSize);
            //_centerCollider = new Rect(Vector2.zero - rect / 2 , rect);
        }

        public override void Update()
        {
            base.Update();
            UpdateTiles();
        }

        private void UpdateTiles()
        {
            if (appState.Center.x != 0 || appState.Center.y != 0 || appState.Center.z != 0) 
            {
                //player movement in TMS tiles: Note the minus in front of the y.
                var tileDif = new Vector2(appState.Center.x, -appState.Center.y);
                Zoom += (int) appState.Center.z;
                //Debug.Log(tileDif);
                //move locals
                Centralize(tileDif);
                //create new tiles
                LoadTiles(CenterTms, CenterInMercator);
                UnloadTiles(CenterTms);
                // Reset movement changes
                appState.Center = Vector3.zero;
            }
        }

        /// <summary>
        /// Move everything to keep current tile at 0,0
        /// </summary>
        /// <param name="tileDif"></param>
        private void Centralize(Vector2 tileDif)
        {
            CenterTms += tileDif.ToVector2d();
            if (_keepCentralized)
            {
                foreach (var tile in Tiles.Values)
                {
                    tile.transform.position -= new Vector3(tileDif.x * TileSize, 0, -tileDif.y * TileSize);
                }

                CenterInMercator = GM.TileBounds(CenterTms, Zoom).Center;
                var difInUnity = new Vector3(tileDif.x * TileSize, 0, -tileDif.y * TileSize);
                Camera.main.transform.position -= difInUnity;
            }
            else
            {
                var difInUnity = new Vector2(tileDif.x*TileSize, -tileDif.y*TileSize);
            }
        }

        private void UnloadTiles(Vector2d currentTms)
        {
            var rem = new List<Vector2d>();
            foreach (var key in Tiles.Keys.Where(x => x.ManhattanTo(currentTms) > _removeAfter))
            {
                rem.Add(key);
                Destroy(Tiles[key].gameObject);
            }
            foreach (var v in rem)
            {
                Tiles.Remove(v);
            }
        }

        private Vector2 GetMovementVector()
        {
            //var dif = _player.transform.position.ToVector2xz();
            var tileDif = Vector2.zero;
            //if (dif.x < Math.Min(_centerCollider.xMin, _centerCollider.xMax))
            //    tileDif.x = -1;
            //else if (dif.x > Math.Max(_centerCollider.xMin, _centerCollider.xMax))
            //    tileDif.x = 1;

            //if (dif.y < Math.Min(_centerCollider.yMin, _centerCollider.yMax))
            //    tileDif.y = 1;
            //else if (dif.y > Math.Max(_centerCollider.yMin, _centerCollider.yMax))
            //    tileDif.y = -1; //invert axis  TMS vs unity
            return tileDif;
        }
    }
}
