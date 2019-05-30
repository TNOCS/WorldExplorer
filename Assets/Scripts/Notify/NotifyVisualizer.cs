using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotifyVisualizer : MonoBehaviour
{ 
    public GameObject m_Dialog;
    public Text m_Message;
    private IEnumerator coroutine;


    private bool m_IsDisplayingNotification = false;

    void Start()
    {
        m_Dialog?.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Dialog == null) return; // No GUI
        if (m_IsDisplayingNotification) return;
        var notify = NotifyManager.Instance.GetNotifyMessage();
        if (notify != null)
        {
            coroutine = DisplayDialog(notify);
            StartCoroutine(coroutine);
        }
    }

    IEnumerator DisplayDialog(NotifyManager.Notify pNotify)
    {
       
        m_IsDisplayingNotification = true;
        m_Dialog?.SetActive(true);
        
        Debug.Log("Display notification: " + pNotify.m_MessageText);
        if (m_Message != null) m_Message.text = pNotify.m_MessageText;
        yield return new WaitForSeconds(pNotify.m_DisplaySeconds);
        m_IsDisplayingNotification = false;
        m_Dialog?.SetActive(false);
        yield return null;
    }


}
