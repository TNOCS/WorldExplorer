using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;
using MapzenGo.Models;
using System.Collections.Generic;

public class AppState : Singleton<AppState>
{
    protected AppState() {
    } // guarantee this will be always a singleton only - can't use the constructor!
    
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
    public Layer()
    {
        Enabled = false;
        Height = 1.5F;
        UseTransparency = true;
    }

    public void FromJson(JSONObject json)
    {
        Title = json.GetString("Title");
        Url = json.GetString("Url");
        Type = json.GetString("Type");
        Enabled = json.GetBoolean("Enabled");
        VoiceCommand = json.GetString("VoiceCommand");
        UseTransparency = json.GetBoolean("UseTransparency");
        Height = json.GetFloat("Height");
    }

    public string Title { get; set; }
    public string Type { get; set; }
    /// <summary>
    /// Voice command to turn the layer on/off
    /// </summary>
    public string VoiceCommand { get; set; }
    /// <summary>
    /// Source URL 
    /// </summary>
    public string Url { get; set; }
    public bool Enabled { get; set; }
    /// <summary>
    /// Some layers are transparent (default true), and the transparency needs to be set.
    /// </summary>
    public bool UseTransparency { get; set; }
    /// <summary>
    /// Rendering height of the layer
    /// </summary>
    public float Height { get; set; }
}


public class ViewState
{
    public void FromJson(JSONObject json)
    {
        Lat = json.GetFloat("Lat");
        Lon = json.GetFloat("Lon");
        Zoom = json.GetInt("Zoom");
        Scale = json.GetInt("Scale");
        Range = json.GetInt("Range");
        TileSize = json.GetInt("TileSize");
    }

    public float Lat { get; set; }
    public float Lon { get; set; }
    public int Zoom { get; set; }
    public int Scale { get; set; }
    public int Range { get; set; }
    public int TileSize { get; set; }

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

    public static int GetInt(this JSONObject json, string key, int defaultValue = 0)
    {
        if (json && json.HasField(key) && json[key].IsNumber) return (int)json[key].n;
        return defaultValue;
    }

    public static float GetFloat(this JSONObject json, string key, float defaultValue = 0f)
    {
        if (json && json.HasField(key) && json[key].IsNumber) return (float)json[key].n;
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
