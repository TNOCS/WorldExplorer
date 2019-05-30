using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR;
using UnityEngine.XR;

[System.Serializable]
public class QrCodeUrlEvent : UnityEvent<string>
{
}


/// <summary>
/// /
/// </summary>
public class QrCodes : MonoBehaviour
{
    public GameObject m_FeedbackObject;

    private CancellationTokenSource m_CancelToken;


    
    public TextMesh m_StatusText;

    [Header("Triggers when QR code is found")]
    public QrCodeUrlEvent m_QrCodeUrlEvent;

    [Header("Default when developing in unity")]
    public string m_DevelopmentUrl = "http://localhost/api/configuration/default.json";

    public void Awake()
    {
        DisableFeedback();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("QR CODE start");
        if (m_StatusText != null) m_StatusText.text = "starting";
        
    }

    private void DisableFeedback()
    {
        m_FeedbackObject?.SetActive(false);
        
    }

     

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScanQrCode()
    {
        Debug.Log("Start scanning for QR code");

        m_FeedbackObject?.SetActive(true);
#if UNITY_EDITOR
        Debug.Log("Not running in hololens, fake QR code.....");
        if (m_StatusText != null) m_StatusText.text = "development mode, using url: " + m_DevelopmentUrl;
        StartCoroutine(DelayedHide());

        // In unity return fixed url
        FireQrCodeFound(m_DevelopmentUrl);
        return;
#endif
#if !UNITY_EDITOR
        if (m_CancelToken != null) return; // already running
        m_CancelToken = new CancellationTokenSource();
        if (m_StatusText != null) m_StatusText.text = "look at QR code to start application";
        StartBackgroundScanningThread();
#endif
    }

    private void StartBackgroundScanningThread()
    {
        Debug.Log("Start scanning in background for QR code.");

#if !UNITY_EDITOR
        MediaFrameQrProcessing.Wrappers.ZXingQrCodeScanner.ScanFirstCameraForQrCode(
        result =>
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() => { HandleQrCodeRecognized(result); }, false);
        },
        m_CancelToken.Token);
#endif
    }

    private void HandleQrCodeRecognized(string pQrCode)
    {

        if (m_CancelToken == null) return;
                Debug.Log("QR code recognized: " + pQrCode ?? "<null>");
        if (pQrCode == default(string))  // Timeout
        {
            
        }
        else
        {
            if (pQrCode.ToUpper().StartsWith("HTTP://"))
            {
                if (m_CancelToken != null) m_CancelToken.Cancel(false);
                if (m_StatusText != null) m_StatusText.text = $"loading '{pQrCode}'";
                FireQrCodeFound(pQrCode);
                StartCoroutine(DelayedHide());
                m_CancelToken = null;
            }
            else if (m_StatusText) m_StatusText.text = $"No valid URL: '{pQrCode}'.";
        }
    }

    IEnumerator DelayedHide()
    {
        
        yield return new WaitForSeconds(5);
        DisableFeedback();
    }


    private void FireQrCodeFound(string pUrl)
    {
        gameObject.GetComponent<AudioSource>()?.Play();
        m_QrCodeUrlEvent?.Invoke(pUrl);
    }
}
