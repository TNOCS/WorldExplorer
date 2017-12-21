using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VMGBuildingsFactory : SingletonCustom<VMGBuildingsFactory>
{
    public List<VMGBuilding> VMGBuildingList = new List<VMGBuilding>();

    void Start()
    {
        //var objectUrl = "http://" + AppState.Instance.Config.ObjectServer + "/" + fileName + ".geojson";
        //var objectUrl = "http://134.221.20.240:8777/vogd_01_building_a_wgs84.min/14/8723/5389.geojson";
        var objectUrl = "http://www.thomvdm.com/testJSONbuildings.json";
        StartCoroutine(GetJSON(objectUrl));
    }

    [Serializable]
    public class Wrapper { public VMGBuildingProperties[] data; }
    public static VMGBuildingProperties[] ParseObjects(string json)
    {
        json = "{ \"data\":" + json + "}";
        Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
        return wrapper.data;
    }

    [Serializable]
    public class VMGBuildingProperties
    {
        public double[][] coordinates;
        public int type;
        public ObjectsTags tags;
    }

    [Serializable]
    public class ObjectsTags
    {
        public string c; // Name / id.
        public string t; // Type. Unused (for now).
        public string s; // subType. Unused (for now).
        public double h; // Height.
        public double b; // Angle of orientation
    }

    private IEnumerator GetJSON(string url)
    {
        using (var www = UnityWebRequest.Get(url))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error + " (Error retrieving objects from " + url + ")");
            }
            else
            {
                var buildingJSONString = www.downloadHandler.text;
                //var jsonString = "{\"Items\":" + json + 
                AddJSONToList(buildingJSONString);
            }
        }
    }

    private void AddJSONToList(string buildingJSONString)
    {
        var json = JSON.Parse(buildingJSONString);
        foreach (JSONNode o in json.Children)
        {
          // List<Vector2> latLonList = new List<Vector2>();
          //
          // // Grabs the X any Y value of each coordinate and adds them as a Vector2 to the list;
          // for (int i = 0; i < o["geometry"][0].Count; i++)
          // {
          //     latLonList.Add(new Vector2(o["geometry"][0][i][0], o["geometry"][0][i][1]));
          // }
          //
          // //int type = o["type"];
          //
          // JSONNode tags = o["tags"];
          // string id = tags["c"]; // ID / Name.
          // float height = tags["h"];
          // float aoo = tags["b"]; // Angle of Orientation.
          //
          // VMGBuilding obj = new VMGBuilding(id, latLonList, height, aoo);
          // VMGBuildingList.Add(obj);
        }

        foreach(VMGBuilding vmgBuilding in VMGBuildingList)
        {
            foreach (Vector2 v2 in vmgBuilding.latLonList)
            {
                //Debug.Log(v2.x + " " + v2.y);
            }
        }
    }
}
