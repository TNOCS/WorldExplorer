using Assets.Scripts;
using Assets.Scripts.Classes;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using MapzenGo.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugExplorer : Singleton<DebugExplorer> {

    public bool debugMode;
    public string currentView;
    private int overlayCounter;

    public GameObject UI, UI1, debugCoordsText;
    public Vector3 devicePosition;
    public Vector3 gazeDirection;
    private void Awake()
    {
       

    }
    // Use this for initialization
    void Start() {        
        debugCoordsText = GameObject.Find("DebugCursorCoords");
        UI = GameObject.Find("UI");
        
        if (debugMode)
        {
            Debug.Log("REMINDER: DEBUG MODE IS ON. SESSIONMANAGER DISABLED");
        }

    }

    private void DisableExternalConnections()
    {
        //Disable Assetbundles.

    }

    void SetOverlay()
    {

    }

    // Update is called once per frame
    void Update() {
      /*  devicePosition = Camera.main.transform.position;
        gazeDirection = Camera.main.transform.forward;
        UI.transform.position = new Vector3(devicePosition.x, devicePosition.y -1f, devicePosition.z +2);
        UI.transform.rotation = Quaternion.LookRotation(gazeDirection);

        */

        // var view = appState.Config.ActiveView;
      
         /*var cursorCoords = SessionManager.Instance.me.CursorLocationToVector2d();
         var cursorCoordsv2 = Extensions.ToVector2(cursorCoords);

         var latv2 = cursorCoordsv2.x;
         var lonv2 = cursorCoordsv2.y;

         var text = debugCoordsText.GetComponent<TextMesh>();

         text.text = latv2.ToString() + " \n " + lonv2.ToString() + " \n ";
         */
   

        if (Input.GetKeyDown(KeyCode.Z))
        {
            //boardInteraction.Zoom(0);
        }


        if (Input.GetKeyDown(KeyCode.X))
        {
            var board = GameObject.Find("terrain");
            board.transform.position = new Vector3(board.transform.position.x, board.transform.position.y, board.transform.position.z + 3);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            var cursorLatLon = SessionManager.Instance.me.CursorLocationToVector2d();
            Debug.Log(cursorLatLon);
            Debug.Log(AppState.Instance.Config.ActiveView.Lat + " | " + AppState.Instance.Config.ActiveView.Lon);

        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.currentMode = "DeleteBtn";
            CursorManagerCustom.Instance.SetCursorIcon("DeleteBtn");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            UIManager.Instance.currentMode = "MoveBtn";
            CursorManagerCustom.Instance.SetCursorIcon("MoveBtn");
        }
    }
        } 
