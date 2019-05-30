using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using MapzenGo.Models;
using MapzenGo.Models.Factories;
using UnityEngine;
using System;

/// <summary>
/// Generate 3D models: Currently, only assets from asset bundles are supported.
/// </summary>
public class ModelFactory : Factory
{
    public string BundleURL;
    /// <summary>
    /// Specify the version number so you will download a new version.
    /// See http://answers.unity3d.com/questions/157563/how-do-you-set-an-assetbundles-version-number.html
    /// </summary>
    public int version = 9;
    public override string XmlTag { get { return "assets"; } }
    public float scale = 1F;
    private HashSet<string> _active = new HashSet<string>();

    public override void Start()
    {
        base.Start();
        Query = (geo) => geo["geometry"]["type"].str == "Point" && geo["properties"].HasField("asset");
    }

    public override void GeoJsonDataLoaded(Tile tile)
    {
        if (!(tile.Data.HasField(XmlTag) && tile.Data[XmlTag].HasField("features")))
            return;

        // string temp = tile.Data[XmlTag].ToString();
        var featureList = tile.Data[XmlTag]["features"].list;
        if (featureList != null && featureList.Count > 0)
        {
            foreach (var entity in featureList.Where(x => Query(x)).SelectMany(geo => Create(tile, geo))) { }
        }
    }

    System.Collections.IEnumerator DownloadAndCache(Tile tile, JSONObject geo, string assetName)
    {
        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

        var bundleURL = geo["properties"].HasField("assetbundle") ? geo["properties"]["assetbundle"].str : BundleURL;

        Debug.Log(bundleURL);
        // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        using (WWW www = WWW.LoadFromCacheOrDownload(bundleURL, version))
        {
            yield return www;
            if (www.error != null)
                throw new Exception("WWW download had an error:" + www.error);
            AssetBundle bundle = www.assetBundle;
            Debug.Log(bundle);
            if (assetName == "")
                Instantiate(bundle.mainAsset);
            else
            {
                Debug.Log(assetName);
                var c2 = geo["geometry"]["coordinates"];
                var dotMerc2 = GM.LatLonToMeters(c2[1].f, c2[0].f);
                var pos = dotMerc2 - tile.Rect.Center;
                var go = Instantiate(bundle.LoadAsset(assetName), new Vector3((float)pos.x, 0F, (float)pos.y), Quaternion.identity) as GameObject;
                go.transform.SetParent(tile.transform, false);
            }
            // Unload the AssetBundles compressed contents to conserve memory
            bundle.Unload(false);
        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
    }


    protected override IEnumerable<MonoBehaviour> Create(Tile tile, JSONObject geo)
    {
        var asset = geo["properties"]["asset"].str;

        if (!_active.Contains(asset))
        {
            _active.Add(asset);
            //Debug.Log("Loading asset: " + asset);
            tile.Destroyed += (s, e) => { _active.Remove(asset); };
            StartCoroutine(LoadAsset(tile, geo, asset));
        }

        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="geo"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    private IEnumerator LoadAsset(Tile tile, JSONObject geo, string assetName)
    {
        var bundleURL = geo["properties"].HasField("assetbundle") ? geo["properties"]["assetbundle"].str : BundleURL;
        var bundle = AssetBundleManager.getAssetBundle(bundleURL, version);
        if (bundle)
            yield return StartCoroutine(InstantiateAsset(bundle, tile, geo, assetName));
        else
            yield return StartCoroutine(DownloadAssetBundle(tile, geo, assetName, bundleURL, version));
    }

    private IEnumerator DownloadAssetBundle(Tile tile, JSONObject geo, string assetName, string url, int version)
    {
        yield return StartCoroutine(AssetBundleManager.downloadAssetBundle(url, version));
        AssetBundle bundle = null;
        while (bundle == null)
        {
            bundle = AssetBundleManager.getAssetBundle(url, version);
            if (!bundle) yield return new WaitForSeconds(0.1F);
        }
        yield return InstantiateAsset(bundle, tile, geo, assetName);
    }

    private IEnumerator InstantiateAsset(AssetBundle bundle, Tile tile, JSONObject geo, string assetName)
    {
        //Debug.Log("Instantiate " + assetName);
        var c2 = geo["geometry"]["coordinates"];
        var dotMerc2 = GM.LatLonToMeters(c2[1].f, c2[0].f);
        var pos = dotMerc2 - tile.Rect.Center;
        var terrainHeight = TerrainHeight.GetTerrainHeight(tile.gameObject, (float)pos.x, (float)pos.y);
        var go = Instantiate(bundle.LoadAsset(assetName), new Vector3((float)pos.x, terrainHeight, (float)pos.y), Quaternion.identity) as GameObject;
        if (go == null) throw new Exception("Error instantating object " + assetName);
        go.transform.localScale = new Vector3(scale, scale, scale);
        go.transform.SetParent(tile.transform, false);
        go.tag = "boardobject";
        var col = go.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
        yield return null;
    }

    void OnDisable()
    {
        //AssetBundleManager.Unload(url, version);
    }

}
