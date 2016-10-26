using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using System.Linq;

using System;

using UnityEngine.Windows.Speech;
using Assets.Scripts.App6;

public class SymbolFactory : MonoBehaviour
{



    [SerializeField]
    public string geojson;



    public delegate void DoVoiceCommand(VoiceCommand c);



    public class VoiceCommand

    {

        public string Command { get; set; }

        public DoVoiceCommand Action { get; set; }

    }



    private List<VoiceCommand> voiceCommands;





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
        public Vector3 point { get; set; }

    }



    public class GeoJson

    {

        public string type { get; set; }

        public List<Feature> features { get; set; }

        public Vector3 center { get; set; }



    }



    private GeoJson geoJson = new GeoJson();



    private const int EarthRadius = 6378137;



    private const double OriginShift = 2 * Math.PI * EarthRadius / 2;



    public static Vector2d LatLonToMeters(Vector2d v)

    {

        return LatLonToMeters(v.x, v.y);

    }



    //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913

    public static Vector2d LatLonToMeters(double lat, double lon)

    {

        var p = new Vector2d();

        p.x = (lon * OriginShift / 180);

        p.y = (Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180));

        p.y = (p.y * OriginShift / 180);

        return new Vector2d(p.x, p.y);

    }



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
                    f.point = parsePoint(f.geometry.coordinates);
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
            geoJson.center = geoJson.features[0].point;


        return geoJson;

    }



    #region voicecommands
    void InitVoiceCommands()

    {



        voiceCommands = new List<VoiceCommand>();



        var go = gameObject.transform;

        foreach (Transform c in go)

        {

            var vcHide = new VoiceCommand();

            vcHide.Command = "Hide " + c.name;

            vcHide.Action = (VoiceCommand command) =>

            {

                c.gameObject.SetActive(false);

            };

            voiceCommands.Add(vcHide);



            var vcShow = new VoiceCommand();

            vcShow.Command = "Show " + c.name;

            vcShow.Action = (VoiceCommand command) =>

            {

                c.gameObject.SetActive(true);

            };

            voiceCommands.Add(vcShow);



        }



        KeywordRecognizer recognizer = new KeywordRecognizer(voiceCommands.Select(k => k.Command).ToArray());

        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;

        recognizer.Start();



    }



    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)

    {

        foreach (var vc in voiceCommands)

        {

            if (vc.Command == args.text)

            {

                vc.Action(vc);

            }

        }

    }

    #endregion

    public void AddSymbols()

    {

        string encodedString = geojson; // "{\"field1\": 0.5,\"field2\": \"sampletext\",\"field3\": [1,2,3]}";

        var geoJson = loadGeoJson(encodedString);



        // setText(geoJson.features.Count + " features");



        var main = new GameObject("Symbols");

        main.transform.SetParent(this.gameObject.transform, true);
        string baseUrl = "http://gamelab.tno.nl/Missieprep/";
        if (geoJson.features != null)
            foreach (Feature c in geoJson.features)
            {
               
                StartCoroutine(Sprite1(c, baseUrl) );


            }
        //   this.gameObject.transform.position = new Vector3(-geoJson.center.x * scale, -0.5f, -geoJson.center.z * scale);

        //      main.transform.Rotate(new Vector3(0.9f, 0, 0));
        //   this.gameObject.transform.localScale = new Vector3(scale, scale, scale);


    }

    IEnumerator Sprite1(Feature c,string baseUrl)
    {
        if (c.properties["symbol"] == null)
          yield  return null;
         string web = baseUrl + c.properties["symbol"].ToString().Replace(@"""","");
        WWW www = new WWW(web);
        yield return www;

        var go = new GameObject("Symbol");
        var symbol = go.AddComponent<Symbol>();
        go.name = "symbol-" + c.properties["id"];
        var sprite = go.AddComponent<SpriteRenderer>();
        sprite.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
        var dotMerc = c.point;
        ///  var localMercPos = dotMerc - tile.Rect.Center;
        ///   go.transform.position = new Vector3((float)localMercPos.x, (float)localMercPos.y);
        go.transform.position = dotMerc;
        symbol.Stick(this.transform);
    }




    //protected override IEnumerable<MonoBehaviour> Create(Tile tile, JSONObject geo)
    //{
    //    var kind = geo["properties"]["kind"].str.ConvertToPoiType();

    //    if (!FactorySettings.HasSettingsFor(kind))
    //        yield break;

    //    var typeSettings = FactorySettings.GetSettingsFor<PoiSettings>(kind);

    //    var go = new GameObject("Poi"); //Instantiate(_labelPrefab);
    //    var poi = go.AddComponent<Poi>();
    //    go.name = "poi-" + tile.name;
    //    //RJ added spriteRenderer
    //    var sprite = go.AddComponent<SpriteRenderer>();
    //    sprite.sprite = typeSettings.Sprite;
    //    //RJ DELETE? Sprite as 3d objects works better and Image doesn't work either?
    //    //poi.GetComponentInChildren<Image>().sprite = typeSettings.Sprite;


    //    //if (geo["properties"].HasField("name"))
    //    //    go.GetComponentInChildren<TextMesh>().text = geo["properties"]["name"].str;
    //    var c = geo["geometry"]["coordinates"];
    //    var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
    //    var localMercPos = dotMerc - tile.Rect.Center;
    //    go.transform.position = new Vector3((float)localMercPos.x, (float)localMercPos.y);
    //    var target = new GameObject("poiTarget");
    //    var targetScript = target.AddComponent<targetForPoi>();
    //    target.transform.position = localMercPos.ToVector3();
    //    target.transform.SetParent(tile.transform, false);
    //    poi.Stick(target.transform);
    //    poi.transform.SetParent(target.transform, true);

    //    SetProperties(geo, poi, typeSettings);
    //    targetScript.Name = (poi.Name != null) ? poi.Name : poi.name;
    //    targetScript.Kind = poi.Kind;
    //    targetScript.Properties = geo["properties"].ToString();
    //    yield return poi;
    //}

    //private static void SetProperties(JSONObject geo, Poi poi, PoiSettings typeSettings)
    //{
    //    poi.Id = geo["properties"]["id"].ToString();
    //    if (geo["properties"].HasField("name"))
    //        poi.Name = geo["properties"]["name"].str;
    //    poi.Type = geo["type"].str;
    //    poi.Kind = geo["properties"]["kind"].str;
    //    // poi.name = "poi";
    //}




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

            var mp = LatLonToMeters(new Vector2d(lat, lon));

            result.Add(new Vector3((float)mp.x, 0, (float)mp.y));

        }



        return result;

    }

    Vector3 parsePoint(JSONObject coords)
    {
        Vector3 result = new Vector3();

        if (coords == null) return result;



        var lat = float.Parse(coords.list[1].ToString());

        var lon = float.Parse(coords.list[0].ToString());

        var mp = LatLonToMeters(new Vector2d(lat, lon));

        result = new Vector3((float)mp.x, 0, (float)mp.y);

        return result;
    }





    // Update is called once per frame

    void Update()
    {



    }

}