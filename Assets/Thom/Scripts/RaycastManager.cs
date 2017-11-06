using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaycastManager : Singleton<RaycastManager>
{
    [SerializeField]
    private float uiScalefactor = 1.35f;
    private bool enlarged = false;
    private bool animationPlaying = false;
    private Quaternion originalRotation;
    private GameObject rotatingObject;
    public GameObject enlargedObject;
    public AudioClip gazeFeedback;
    public AudioSource audioSource;


    public Animation MoveAnimation;
    private Animation RotateAnimation;
    private Animation TiltAnimation;
    private Animation ResizeAnimation;

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

            if (rayCastFocus.name == "DragInteractable" || rayCastFocus.name == "RotateInteractable" || rayCastFocus.name == "TiltInteractable" || rayCastFocus.name == "ResizeInteractable")
            {
                PlayAnimation(rayCastFocus);
            }
            else
            {
                if (animationPlaying)
                {
                    StopAnimations();
                }
            }

            // Enlarges elements that are being gazed and plays a low key sounds.
            if (rayCastFocus.tag == "inventoryobject" || rayCastFocus.tag == "uibutton")
            {
                // If it's an inventory item, also start rotating.
                if (rayCastFocus.tag == "inventoryobject")
                {
                    var originalRotation = rayCastFocus.transform.rotation;
                    if (rotatingObject == null)
                    {
                        rotatingObject = rayCastFocus;
                    }

                    if (rotatingObject != rayCastFocus)
                    {
                        rotatingObject.transform.rotation = originalRotation;
                        rotatingObject = rayCastFocus;
                    }
                    rayCastFocus.transform.Rotate(Vector3.up * Time.deltaTime * 50);
                }

                if (rayCastFocus.tag == "uibutton")
                {
                    if (rayCastFocus.gameObject.name == "InventoryTxt")
                    {
                        uiScalefactor = 1.2f;
                    }
                    else if (rayCastFocus.gameObject.name.Contains("Interactable"))
                    {
                        uiScalefactor = 1.2f;
                    }
                    else
                    {
                        uiScalefactor = 1.35f;
                    }
                }               

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
            // Stop all animations
            if (animationPlaying)
            {
                StopAnimations();
            }
        }
    }

    public void PlayAnimation(GameObject rayCastFocus)
    {
        if (MoveAnimation == null)
        {
            MoveAnimation = GameObject.Find("MoveAnimation").GetComponent<Animation>();
        }
        if (RotateAnimation == null)
        {
            RotateAnimation = GameObject.Find("RotateAnimation").GetComponent<Animation>();
        }
        if (TiltAnimation == null)
        {
            TiltAnimation = GameObject.Find("TiltAnimation").GetComponent<Animation>();
        }
        if (ResizeAnimation == null)
        {
            ResizeAnimation = GameObject.Find("ResizeAnimation").GetComponent<Animation>();
        }


        if (rayCastFocus.name == "DragInteractable")
        {
            RotateAnimation.Stop();
            TiltAnimation.Stop();
            ResizeAnimation.Stop();
            MoveAnimation.Play();
        }
        if (rayCastFocus.name == "RotateInteractable")
        {
            MoveAnimation.Stop();
            TiltAnimation.Stop();
            ResizeAnimation.Stop();
            RotateAnimation.Play();
        }
        if (rayCastFocus.name == "TiltInteractable")
        {
            MoveAnimation.Stop();
            RotateAnimation.Stop();
            ResizeAnimation.Stop();
            TiltAnimation.Play();
        }
        if (rayCastFocus.name == "ResizeInteractable")
        {
            MoveAnimation.Stop();
            RotateAnimation.Stop();
            TiltAnimation.Stop();
            ResizeAnimation.Play();
        }
        animationPlaying = true;
    }

    private void StopAnimations()
    {
        MoveAnimation.Stop();
        RotateAnimation.Stop();
        TiltAnimation.Stop();
        ResizeAnimation.Stop();
        animationPlaying = false;
    }


    // TODO: replace with IFocusable interface (?)
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

