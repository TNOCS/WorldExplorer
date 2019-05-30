using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Plugins;
using System.Collections;
//using WorldExplorerClient.interfaces;
//using Newtonsoft.Json;
//using eu.driver.model.worldexplorer;



public class Initialize : MonoBehaviour
{
    //private string configUrl = "http://thomvdm.com/hololensconfig.json_donotuse";
    // Use this for initialization
    /// <summary>
    /// Your own cursor
    /// </summary>
    // GameObject cursorFabSelf;
    /// <summary>
    /// Cursor for other users
    /// </summary>
    // GameObject cursorFabOther;
    //private GameObject cursor;
    //private GameObject HoloManagers;
    //private AppState appState;
    //private Font font;
    //private AudioClip fingerPressedSound;

    void Awake()
    {
        
        Debug.Log("Waking up...");
        // We need this so the MQTT thread can receive messages
        // var mtd = gameObject.AddComponent<UnityMainThreadDispatcher
        //cursorFabSelf = Resources.Load("Prefabs/BasicCursor") as GameObject;
        //cursorFabOther = Resources.Load("Prefabs/BasicCursorOther") as GameObject;
        //font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //fingerPressedSound = (AudioClip)Resources.Load("FingerPressed");

        //appState = AppState.Instance;
    }

    IEnumerator Start()
    {
        Debug.Break();
        Debug.Log("Initializing...");
        
        CursorManagerCustom.Instance.InitCursor();
        UIManager.Instance.currentMode = "MoveBtn";
     
        yield return  null;
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    public void SetOverlay()
    {
        AppState.Instance.Config.ActiveView = AppState.Instance.Config.Views[2].Clone();
        AppState.Instance.ResetMap();
         /* HKL_TEST
        SessionManager.Instance.UpdateView(appState.Config.ActiveView, null);
        */
    }
}
