using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR.WSA.Input;

public class Cursor : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private bool t = false;


    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    private GameObject prevTarget;
    GestureRecognizer recognizer;
    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        ;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                FocusedObject.BroadcastMessage("OnSelect");
            }
        };
        recognizer.StartCapturingGestures();
    }

    private void OnTriggerEnter(Collider other)
    {

    }
    public void setPosition(GameObject pos)
    {
        this.transform.position = pos.transform.position;
        // Rotate the cursor to hug the surface of the hologram.
        this.transform.rotation =
            Quaternion.FromToRotation(pos.transform.position, Camera.main.transform.forward);

        meshRenderer.enabled = t;
    }
    // Update is called once per frame
    void Update()
    {
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        GameObject oldFocusObject = FocusedObject;
        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram...

            // Display the cursor mesh.
            meshRenderer.enabled = true;
            // Move the cursor to the point where the raycast hit.
            this.transform.position = hitInfo.point;
            // Rotate the cursor to hug the surface of the hologram.
            this.transform.rotation =
                Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
           


            
                if (hitInfo.transform.gameObject.tag == "symbol")
                {

                    if (prevTarget == null)
                        hitInfo.transform.gameObject.GetComponent<SymbolTargetHandler>().ToggleGUI();
                    else
                        if (prevTarget != hitInfo.transform.gameObject)
                    {
                        prevTarget.GetComponent<SymbolTargetHandler>().ToggleGUI();
                        hitInfo.transform.gameObject.GetComponent<SymbolTargetHandler>().ToggleGUI();
                    }
                    prevTarget = hitInfo.transform.gameObject;
                    if (Input.GetMouseButtonDown(0))
                    {
                    FocusedObject = prevTarget;
                    FocusedObject.BroadcastMessage("OnSelect");
                }
                }
                else
                {
                    if (prevTarget != null)
                    {
                        prevTarget.GetComponent<SymbolTargetHandler>().ToggleGUI();
                        prevTarget = null;
                    }
                }
            

        }
        else
        {

            //System.Collections.Generic.List<RaycastResult> hitGuiInfo = new System.Collections.Generic.List<RaycastResult>();
            //PointerEventData pointer = new PointerEventData(EventSystem.current);
            //EventSystem.current.RaycastAll(pointer, hitGuiInfo);
            //// GraphicRaycaster gr = 
            //t = true;
            //meshRenderer.enabled = true;

            //if (hitGuiInfo.Count > 0)
            //{
            //    foreach (var go in hitGuiInfo)
            //    {
            //        Debug.Log(go.gameObject.name, go.gameObject);
            //    }
            //}

            //    else
            // If the raycast did not hit a hologram, hide the cursor mesh.
            meshRenderer.enabled = false;
        }
        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            oldFocusObject.BroadcastMessage("OnSelect");
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }
}