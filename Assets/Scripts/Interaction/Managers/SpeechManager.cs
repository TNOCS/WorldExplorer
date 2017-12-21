using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour {

    public void SetUIMode(string keyword)
    {
        UIInteraction.Instance.SwitchMode(keyword);
        Debug.Log("Speech Recognized, calling " + keyword);
    }

    public void SetEditMode(string keyword)
    { 
        UIManager.Instance.CallFunctions(keyword);
        Debug.Log("Speech Recognized, calling " + keyword);
    }
  
    public void SetLocation(string keyword)
    {
        UIManager.Instance.CallFunctions(keyword);
        Debug.Log("Speech Recognized, calling " + keyword);
    }

    public void ToggleVideoMode(string keyword)
    {
        DebugExplorer.Instance.ToggleVideoMode();
    }
}
