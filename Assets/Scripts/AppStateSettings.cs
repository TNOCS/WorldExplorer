using UnityEngine;
using System.Collections;
using MapzenGo.Models.Settings;

public class AppStateSettings {
    private static AppStateSettings instance;

    private AppStateSettings() { }

    public static AppStateSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppStateSettings();
            }
            return instance;
        }
    }

    public Vector3 Center { get; set; }
}
