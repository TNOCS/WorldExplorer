using System;
using UnityEngine;
using System.Collections;
class ABTest : MonoBehaviour
{
    private string BundleURL = "http://www.thomvdm.com/assets/eindhoven";
    //public string BundleURL = "134.221.20.240:3999";
    private string AssetName = "flyingpins";

    IEnumerator Start()
    {        
        // Download the file from the URL. It will not be saved in the Cache
        using (WWW www = new WWW(BundleURL))
        {
            yield return www;
            Debug.Log(www.assetBundle);
            Debug.Log(www.url);
            /*if (www.error != null)
                throw new Exception("WWW download had an error:" + www.error);*/
            AssetBundle bundle = www.assetBundle;
            if (AssetName == "")
                Instantiate(bundle.mainAsset);
            else
                Debug.Log("Loading " + AssetName);
                Instantiate(bundle.LoadAsset(AssetName));
            // Unload the AssetBundles compressed contents to conserve memory
            bundle.Unload(false);

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
    }
}
