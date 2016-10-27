using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using System.Linq;

using System;

using UnityEngine.Windows.Speech;
using Assets.Scripts.App6;
using MapzenGo.Helpers;
using MapzenGo.Models;
using Assets.Scripts.MapzenGoWrappers;

public class SymbolFactory : MonoBehaviour
{



    [SerializeField]
    public string geojson;



    [System.Serializable]

    public class Geometry

    {

        public string type { get; set; }

        //      public double[] coordinates { get; set; }

        public JSONObject coordinates { get; set; }

        public List<Vector3> vectors { get; set; }

        // List<List<List<List<double>>>> 

    }



    [System.Serializable]

    public class Feature

    {

        public string type { get; set; }

        public Dictionary<string, object> properties { get; set; }

        public Geometry geometry { get; set; }
        public Vector2d tilePoint { get; set; }
        public JSONObject cor { get; set; }

    }



    public class GeoJson

    {

        public string type { get; set; }

        public List<Feature> features { get; set; }

        public Vector3 center { get; set; }



    }



    private GeoJson geoJson = new GeoJson();



    [SerializeField]
    public float Latitude = 39.921864f;
    [SerializeField]
    public float Longitude = 32.818442f;
    [SerializeField]
    public int zoom;
    [SerializeField]
    public int Range;
    [SerializeField]
    public float TileSize = 100;

    protected Transform symbolHost;
    //  protected Dictionary<Vector2d, Tile> Tiles;
    protected Vector2d CenterTms; //tms tile coordinate
    protected Vector2d CenterInMercator; //this is like distance (meters) in mercator 
    private GeoJson loadGeoJson(string text)

    {

        JSONObject geojson = new JSONObject(text);

        geoJson.features = new List<Feature>();

        var features = geojson["features"];

        for (var fid = 0; fid < features.Count; fid++)

        {

            JSONObject feature = features[fid];

            var f = new Feature();

            f.geometry = new Geometry();

            f.geometry.type = feature["geometry"]["type"].ToString().Replace("\"", "");

            f.geometry.coordinates = feature["geometry"]["coordinates"];



            switch (f.geometry.type)

            {

                case "MultiPolygon":

                    f.geometry.vectors = parsePolygon(f.geometry.coordinates.list[0]);

                    break;

                case "Polygon":

                    f.geometry.vectors = parsePolygon(f.geometry.coordinates);

                    break;
                case "Point":
                    f.tilePoint = parseTile(f.geometry.coordinates);
                    f.cor = f.geometry.coordinates;
                    break;

            }

            //switch (f.geometry.type)

            //{

            //    case "MultiPolygon":



            //        break;

            //}

            f.properties = new Dictionary<string, object>();

            foreach (var s in feature["properties"].keys)

            {

                f.properties[s] = feature["properties"][s];

            }

            geoJson.features.Add(f);

        }
        if (geoJson.features[0].geometry.vectors != null)
            geoJson.center = geoJson.features[0].geometry.vectors[0];
        else
            geoJson.center = new Vector3((float)geoJson.features[0].tilePoint.x, 0, (float)geoJson.features[0].tilePoint.y);


        return geoJson;

    }
    private Vector3 center;
    protected Dictionary<Vector2d, Tile> SymbolTiles;
    public void AddSymbols()

    {

        string encodedString = geojson; // "{\"field1\": 0.5,\"field2\": \"sampletext\",\"field3\": [1,2,3]}";

        var geoJson = loadGeoJson(encodedString);


        SymbolTiles = new Dictionary<Vector2d, Tile>();
        // setText(geoJson.features.Count + " features");
        symbolHost = new GameObject("symbolTiles").transform;
        symbolHost.SetParent(transform, false);

        var v2 = GM.LatLonToMeters(Latitude, Longitude);
        var tile = GM.MetersToTile(v2, zoom);

        //center of main symbolholder object
        CenterTms = tile;
        //bouding box
        CenterInMercator = GM.TileBounds(CenterTms, zoom).Center;

        CreateSymbolTiles(CenterTms, CenterInMercator);



        // set symbol holder scale
        var rect = GM.TileBounds(CenterTms, zoom);
        transform.localScale = Vector3.one * (float)(TileSize / rect.Width);
        center = rect.Center.ToVector3();

    }


    private void CreateSymbolTiles(Vector2d CenterTms, Vector2d CenterInMercator)
    {

        string baseUrl = "http://gamelab.tno.nl/Missieprep/";
        if (geoJson.features != null)
        {
            for (int i = -Range; i <= Range; i++)
            {
                for (int j = -Range; j <= Range; j++)
                {
                    var v = new Vector2d(CenterTms.x + i, CenterTms.y + j);
                    if (SymbolTiles.ContainsKey(v))
                        continue;
                    var rect = GM.TileBounds(v, zoom);
                    var tile = new GameObject("symboltile-" + v.x + "-" + v.y).AddComponent<Tile>();
                  
                    tile.Zoom = zoom;
                    tile.TileTms = v;
                    tile.TileCenter = rect.Center;
                    // tile.Material = MapMaterial;
                    tile.Rect = GM.TileBounds(v, zoom);

                    SymbolTiles.Add(v, tile);
                    tile.transform.position = (rect.Center - CenterInMercator).ToVector3();
                    tile.transform.SetParent(symbolHost, false);
                  

                }
            }
            foreach (Feature c in geoJson.features)
                   {


                    StartCoroutine(Sprite1(c, baseUrl,CenterTms,CenterInMercator));


                 }
        }

    }


    IEnumerator Sprite1(Feature c, string baseUrl, Vector2d tms, Vector2d centerInMercator)
    {
        if (c.properties["symbol"] == null)
            yield return null;
        string web = baseUrl + c.properties["symbol"].ToString().Replace(@"""", "");
        WWW www = new WWW(web);
        yield return www;


        if (SymbolTiles.ContainsKey(c.tilePoint))
        {
            var tile = SymbolTiles[c.tilePoint];

        

            var go = new GameObject("Symbol");
         //   var rect = GM.TileBounds(c.tilePoint, zoom);
            var dotMerc = GM.LatLonToMeters(c.cor[1].f, c.cor[0].f);
            var localMercPos = dotMerc - tile.Rect.Center;
            go.transform.position = new Vector3((float)localMercPos.x, (float)localMercPos.y);

            var target = new GameObject("symbol-Target");
            target.transform.position = localMercPos.ToVector3();
            target.transform.SetParent(tile.transform, false);

            var symbol = go.AddComponent<Symbol>();
            go.name = "symbol-" + c.properties["id"];
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));


            ///  var localMercPos = dotMerc - tile.Rect.Center;
            ///   go.transform.position = new Vector3((float)localMercPos.x, (float)localMercPos.y);

            symbol.Stick(target.transform);
            symbol.transform.SetParent(target.transform, true);

        }
    }

    List<Vector3> parseMultiPolygon(JSONObject coords)

    {





        List<Vector3> result = new List<Vector3> { };

        if (coords == null) return result;

        result.AddRange(parsePolygon(coords.list[0]));



        return result;

    }

    List<Vector3> parsePolygon(JSONObject coords)

    {

        List<Vector3> result = new List<Vector3> { };

        if (coords == null) return result;

        var points = coords.list[0];

        foreach (var p in points.list.Take(points.list.Count - 1))

        {

            var lat = float.Parse(p.list[1].ToString());

            var lon = float.Parse(p.list[0].ToString());

            var mp = GM.LatLonToMeters(new Vector2d(lat, lon));
           
            result.Add(new Vector3((float)mp.x, 0, (float)mp.y));

        }



        return result;

    }

    Vector2d parseTile(JSONObject coords)
    {
        Vector2d result = new Vector2d();

        if (coords == null) return result;



        var lat = float.Parse(coords.list[1].ToString());

        var lon = float.Parse(coords.list[0].ToString());

        var mp = GM.LatLonToMeters(new Vector2d(lat, lon));

        mp = GM.MetersToTile(mp, zoom);
        result = mp;//new Vector3((float)mp.x, 0, (float)mp.y);
        return result;
    }





    // Update is called once per frame

    void Update()
    {



    }

}