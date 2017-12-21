using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaycastManager : SingletonCustom<RaycastManager>
{
    [SerializeField]
    private float uiScalefactor = 1.25f;
    private bool enlarged = false;
    private bool animationPlaying = false;
    private Quaternion originalRotation;
    private GameObject rotatingObject;
    public GameObject enlargedObject;
    public AudioClip gazeFeedback;
    public AudioSource audioSource;

    private void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[0];
        gazeFeedback = Resources.Load<AudioClip>("Audio/lowclick");
    }  
   
    public void Update()
    {
        // Enlarges elements being gazed at.
        EmphasizeGazeTargets();
    } 

    private void EmphasizeGazeTargets()
    {
        if (Raycast().collider != null)
        {
           var rayCastFocus = Raycast().collider.gameObject;

            // Enlarges elements that are being gazed and plays a low key sounds.
            if (rayCastFocus.tag == "inventoryobject" || rayCastFocus.tag == "uibutton")
            {
                // If it's an inventory item, also start rotating.
                // ROTATION DISABLED UNTIL TNO MODELS HAVE CORRET PIVOT POINT.
               // if (rayCastFocus.tag == "inventoryobject")
               //{
               //   var originalRotation = rayCastFocus.transform.rotation;
               //   if (rotatingObject == null)
               //   {
               //       rotatingObject = rayCastFocus;
               //   }
               //
               //   if (rotatingObject != rayCastFocus)
               //   {
               //       rotatingObject.transform.rotation = originalRotation;
               //       rotatingObject = rayCastFocus;
               //   }
               //   rayCastFocus.transform.Rotate(Vector3.up * Time.deltaTime * 50);
               //  }            

                // Enlarges
                if (!enlarged)
                {
                    rayCastFocus.transform.localScale = rayCastFocus.transform.localScale * uiScalefactor;
                    audioSource.PlayOneShot(gazeFeedback, 0.1f);
                    enlargedObject = rayCastFocus;
                    enlarged = true;
                }
                // 'Unlarges'
                if (enlarged && rayCastFocus != enlargedObject)
                {
                    enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                    enlargedObject = null;
                    enlarged = false;
                }
            }
            else
            {
                // Scales elements back down
                if (enlargedObject != null)
                {
                    enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                    enlargedObject = null;
                    enlarged = false;
                }                
            }
        }
        else
        {
            // Stop rotating objects
            if (rotatingObject != null)
            {
                rotatingObject.transform.rotation = originalRotation;
                rotatingObject = null;
            }
            // Scales elements back down
            if (enlargedObject != null)
            {
                enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                enlargedObject = null;
                enlarged = false;
            }
        }
    }
    
    public RaycastHit Raycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, Physics.DefaultRaycastLayers))
        {
            return hit;
        }
        return hit;
    }
}

