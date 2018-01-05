using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{

    public void SetUIMode(string keyword)
    {
        UIInteraction.Instance.SwitchMode(keyword);
    }

    public void SetEditMode(string keyword)
    {
        UIManager.Instance.CallFunctions(keyword);
    }

    public void SetLocation(string keyword)
    {
        UIManager.Instance.CallFunctions(keyword);
    }

    // Video mode disables cursors and POIs.
    public void ToggleVideoMode(string keyword)
    {
        DebugExplorer.Instance.ToggleVideoMode();
    }
}
