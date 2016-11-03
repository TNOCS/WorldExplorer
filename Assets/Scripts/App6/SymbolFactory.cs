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
using MapzenGo.Helpers.VectorD;
using UnityEngine.UI;
public class SymbolFactory : MonoBehaviour
{






    [System.Serializable]

    public class Geometry
    {
        public string type { get; set; }
        public JSONObject coordinates { get; set; }
        public List<Vector3> vectors { get; set; }
    }



    [System.Serializable]

    public class Feature

    {

        public string type { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public Geometry geometry { get; set; }
        public Vector2d tilePoint { get; set; }
        // lat lon
        public JSONObject cor { get; set; }
    }



    public class GeoJson

    {

        public string type { get; set; }

        public List<Feature> features { get; set; }

        public Vector3 center { get; set; }



    }
    /// <summary>
    /// JSON imput string from service
    /// </summary>
    [SerializeField]
    public string geojson;
    private GeoJson geoJson = new GeoJson();

    public string baseUrl;
    #region tilemangerproperties
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
    #endregion
    //parent object (layer)
    protected Transform symbolHost;
    protected GameObject _symbolInfo;
    protected Vector2d CenterTms; //tms tile coordinate
    protected Vector2d CenterInMercator; //this is like distance (meters) in mercator 
    private Vector3 center;
    void Awake()
    {
        _symbolInfo = Resources.Load("_symbolInfo") as GameObject;
    }
    protected List<Vector2d> SymbolTiles;
    /// <summary>
    /// Build the symbol layer
    /// </summary>
    public void AddSymbols()
    {

        string encodedString = geojson;

        var geoJson = loadGeoJson(encodedString);


        SymbolTiles = new List<Vector2d>();
        // setText(geoJson.features.Count + " features");
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

        if (geoJson.features != null)
        {
            for (int i = -Range; i <= Range; i++)
            {
                for (int j = -Range; j <= Range; j++)
                {
                    ///Make an Dicionary of displayed titels as to check later if a symbol should be rendered.
                    var v = new Vector2d(CenterTms.x + i, CenterTms.y + j);
                    if (SymbolTiles.Contains(v))
                        continue;
                    var rect = GM.TileBounds(v, zoom);
                    SymbolTiles.Add(v);
                }
            }

            //fill newly created tiles with symbols if present
            foreach (Feature c in geoJson.features)
            {
                StartCoroutine(createSymbols(c, baseUrl));
            }
        }

    }

    /// <summary>
    /// Loads the image data from the web and creates a symbol
    /// </summary>
    /// <param name="c">the GEOJSON point</param>
    /// <param name="baseUrl"> link to the baselocation of the images</param>
    /// <returns></returns>
    IEnumerator createSymbols(Feature c, string baseUrl)
    {
        yield return null;
        if (c.properties["symbol"] == null)
            yield return null;
        string web = baseUrl + c.properties["symbol"].ToString().Replace(@"""", "");
        WWW www = new WWW(web);
        yield return www;

        // Check if the point is  in the displayed tile area if so continue
        if (SymbolTiles.Contains(c.tilePoint))
        {


            if (_symbolInfo)
            {
                string symbolname = "symbol-" + c.properties["id"];
                var target = new GameObject("Symbol-target");


                var symbol = new GameObject("Symbol");
                symbol.name = symbolname;
                var dotMerc = GM.LatLonToMeters(c.cor[1].f, c.cor[0].f);
                var localMercPos = (dotMerc - CenterInMercator);
                symbol.transform.position = new Vector3((float)localMercPos.x, (float)localMercPos.y);

                // var target = new GameObject("symbol-Target");
                target.transform.position = localMercPos.ToVector3();
                target.transform.SetParent(transform, false);
                var sprite = symbol.AddComponent<SpriteRenderer>();
                sprite.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
                var symbolCom = symbol.AddComponent<Symbol>();
                symbolCom.Stick(target.transform);

                symbol.transform.SetParent(target.transform, true);
                symbol.transform.localScale = new Vector3(10, 10);

                if (c !=null && c.properties.ContainsKey("stats") && c.properties["stats"] != null)
                {
                    
                    var info = (GameObject)Instantiate(_symbolInfo);
                    var canvas = info.GetComponent<Canvas>();

                    canvas.worldCamera = Camera.main;
                    info.transform.SetParent(target.transform, false);
                    canvas.transform.localScale = new Vector3(50, 100);
                    canvas.transform.localPosition = new Vector3(25, 70, 0);
                    var bar = canvas.transform.FindChild("bar");
                    bar.localScale = new Vector3(100, 100);
                    var BarFill = bar.FindChild("Bar-Background").FindChild("Bar-Fill").gameObject.GetComponentInChildren<Image>().fillAmount= 90;//calculate fill stat value


                    // Image voor balk:
                    //var ICO = bar.transform.FindChild("ICO").gameObject.GetComponent<Image>();
                    //  ICO.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));

                }


            }
        }


    }
    Vector2d parseTile(JSONObject coords)
    {
        Vector2d result = new Vector2d();

        if (coords == null) return result;



        var lat = float.Parse(coords.list[1].ToString());

        var lon = float.Parse(coords.list[0].ToString());

        var mp = GM.LatLonToMeters(new Vector2d(lat, lon));

        mp = GM.MetersToTile(mp, zoom);
        result = mp;
        return result;
    }
    /// <summary>
    /// Parse GEO JSON text to Object
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
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

                    // f.geometry.vectors = parsePolygon(f.geometry.coordinates.list[0]);

                    break;

                case "Polygon":

                    // f.geometry.vectors = parsePolygon(f.geometry.coordinates);

                    break;
                case "Point":
                    f.tilePoint = parseTile(f.geometry.coordinates);
                    f.cor = f.geometry.coordinates;
                    break;

            }

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




    void Update()
    {



    }

}