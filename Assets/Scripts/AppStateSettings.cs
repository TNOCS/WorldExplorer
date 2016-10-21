using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models;
using System.Collections.Generic;

public class AppState : Singleton<AppState>
{
    protected AppState() { } // guarantee this will be always a singleton only - can't use the constructor!
    
    public Vector3 Center { get; set; }
    public CachedTileManager TileManager { get; set; }

    public AppConfig Config { get; set; }
    public ViewState State { get; set; }

    public void LoadConfig()
    {
        var targetFile = Resources.Load<TextAsset>("config");
        var test = new JSONObject(targetFile.text);

        Config = new AppConfig();
        Config.FromJson(test);
    }
}


public class Layer
{
    public void FromJson(JSONObject json)
    {
        Title = json.GetString("Title");
        Type = json.GetString("Type");
        Enabled = json.GetBoolean("Enabled");
    }

    public string Title { get; set; }
    public string Type { get; set; }
    public bool Enabled { get; set; }
}


public class ViewState
{
    public void FromJson(JSONObject json)
    {
        Lat = json.GetDouble("Lat");
        Lon = json.GetDouble("Lon");
        Zoom = json.GetDouble("Zoom");
        Scale = json.GetDouble("Scale");
        Range = json.GetDouble("Range");
    }

    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Zoom { get; set; }
    public double Scale { get; set; }
    public double Range { get; set; }

    // center
    // scale
    // zoom
    // range
}

public class AppConfig
{

    public AppConfig()
    {
        //this.Layers = new List<Layer>();
    }

    public void FromJson(JSONObject json)
    {
        TileServer = json.GetString("TileServer");
        MqttServer = json.GetString("MqttServer");
        MqttPort = json.GetString("MqttPort");

        Layers = new List<Layer>();
        var ll = json["Layers"];
        for (var l = 0; l < ll.Count; l++)
        {
            var layer = new Layer();
            layer.FromJson(ll[l]);
            Layers.Add(layer);
        }

        InitalView = new ViewState();
        InitalView.FromJson(json["InitalView"]);
    }

    public List<Layer> Layers { get; set; }
    public string TileServer { get; set; }
    public string MqttServer { get; set; }
    public string MqttPort { get; set; }
    public ViewState InitalView { get; set; }
}

public static class JsonHelpers
{
    public static double GetDouble(this JSONObject json, string key, double defaultValue = 0)
    {
        if (json && json.HasField(key) && json[key].IsNumber) return json[key].n;
        return defaultValue;
    }

    public static string GetString(this JSONObject json, string key, string defaultValue = "")
    {
        if (json && json.HasField(key) && json[key].IsString) return json[key].str;
        return defaultValue;
    }

    public static bool GetBoolean(this JSONObject json, string key, bool defaultValue = false)
    {
        if (json && json.HasField(key) && json[key].IsBool) return json[key].b;
        return defaultValue;
    }

}
