using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cursor : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private bool t = false;
    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
    }
    public void setPosition(GameObject pos)
    {
        this.transform.position=  pos.transform.position ;
        // Rotate the cursor to hug the surface of the hologram.
        this.transform.rotation =
            Quaternion.FromToRotation(pos.transform.position, Camera.main.transform.forward);
        t = true;
        meshRenderer.enabled = t;
    }
    // Update is called once per frame
    void Update()
    {
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

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
        }
        else
        {

          //  System.Collections.Generic.List<RaycastResult> hitGuiInfo = new System.Collections.Generic.List<RaycastResult>();
          //  PointerEventData pointer = new PointerEventData(EventSystem.current);
            //EventSystem.current.RaycastAll(pointer, hitGuiInfo);
           // GraphicRaycaster gr = 
            if (t)//(hitGuiInfo != null&& hitGuiInfo.Count>0)
            {
                // If the raycast hit a hologram...
              
                // Display the cursor mesh.
               
            }
            else
                // If the raycast did not hit a hologram, hide the cursor mesh.
                meshRenderer.enabled = false;
        }
    }
}