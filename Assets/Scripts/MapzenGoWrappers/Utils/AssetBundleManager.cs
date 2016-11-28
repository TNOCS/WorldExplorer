using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

static public class AssetBundleManager
{
    // A dictionary to hold the AssetBundle references
    static private Dictionary<string, AssetBundleRef> dictAssetBundleRefs;
    static private List<string> downloadingKeyNames = new List<string>();

    static AssetBundleManager()
    {
        dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
    }
    // Class with the AssetBundle reference, url and version
    private class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
        public int version;
        public string url;
        public AssetBundleRef(string strUrlIn, int intVersionIn)
        {
            url = strUrlIn;
            version = intVersionIn;
        }
    };
    // Get an AssetBundle
    public static AssetBundle getAssetBundle(string url, int version)
    {
        string keyName = url + version.ToString();
        AssetBundleRef abRef;
        if (dictAssetBundleRefs.TryGetValue(keyName, out abRef))
            return abRef.assetBundle;
        else
            return null;
    }

    // Download an AssetBundle
    public static IEnumerator downloadAssetBundle(string url, int version)
    {
        string keyName = url + version.ToString();
        if (dictAssetBundleRefs.ContainsKey(keyName) || downloadingKeyNames.Contains(keyName))
            yield return null;
        else
        {
            downloadingKeyNames.Add(keyName);
            while (!Caching.ready)
                yield return null;

            using (WWW www = WWW.LoadFromCacheOrDownload(url, version))
            {
                yield return www;
                if (www.error != null)
                    throw new Exception("WWW download:" + www.error);
                AssetBundleRef abRef = new AssetBundleRef(url, version);
                abRef.assetBundle = www.assetBundle;
                dictAssetBundleRefs.Add(keyName, abRef);
                downloadingKeyNames.Remove(keyName);
            }
        }
    }
    // Unload an AssetBundle
    public static void Unload(string url, int version, bool allObjects)
    {
        string keyName = url + version.ToString();
        AssetBundleRef abRef;
        if (dictAssetBundleRefs.TryGetValue(keyName, out abRef))
        {
            abRef.assetBundle.Unload(allObjects);
            abRef.assetBundle = null;
            dictAssetBundleRefs.Remove(keyName);
        }
    }
}
