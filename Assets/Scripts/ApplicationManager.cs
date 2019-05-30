using Assets.Scripts;
using Assets.Scripts.Plugins;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// Since singleton can't be referenced in unity editor, create entry point with this
public class ApplicationManager : MonoBehaviour
{
    public InitialPlacement m_InitialPlacement;
    public QrCodes m_QrCode;
    public GameObject m_Origin;
    
    void Start()
    {
        Debug.Log("Application Manager started");
        CursorManagerCustom.Instance.InitCursor();
        UIManager.Instance.currentMode = "MoveBtn";
        
#if !UNITY_EDITOR && UNITY_WSA
        // In hololens use tap to place
        m_InitialPlacement?.StartPlacement();
#else
        TablePlaced();
#endif
        GameObject cursorFabOther = Resources.Load("Prefabs/BasicCursorOther") as GameObject;
        UIManager.Instance.InitUI();
        AppState.Instance.Camera = gameObject;
        AppState.Instance.Cursor = GameObject.Find("Cursor");
        SessionManager.Instance.cursorPrefab = cursorFabOther;

        //LoadConfiguration("http://localhost:8888/api/Configuration/default.json");
    }
  
    void Update()
    {
        
    }


    public void TablePlaced()
    {
        //GameObject.Find("dummy").transform.position = m_Origin.transform.position;
        m_QrCode?.ScanQrCode(); // This will trigger LoadConfiguration when qr code is found
    }

    public void LoadConfiguration(string pUrl)
    {
        Debug.Log($"Load config (thread id {Thread.CurrentThread.ManagedThreadId})");
        AppState.Instance.LoadConfigAsync(pUrl);


    }
}
