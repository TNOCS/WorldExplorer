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
        Debug.Log("yo");
        UIManager.Instance.CallFunctions(keyword);
        Debug.Log("Speech Recognized, calling " + keyword);
    }
  
    public void SetLocation(string keyword)
    {
        UIManager.Instance.CallFunctions(keyword);
        Debug.Log("Speech Recognized, calling " + keyword);
    }
}
