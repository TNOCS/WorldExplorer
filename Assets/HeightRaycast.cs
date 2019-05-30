using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightRaycast : MonoBehaviour
{
    public float x;

    public float z;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //int layerMask = 10;
     
        Vector3 pos = new Vector3(transform.TransformPoint(x, 0, 0).x, 10000, transform.TransformPoint(0, 0, z).z);
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(pos, -Vector3.up, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(pos, -Vector3.up * (hit.distance), Color.yellow);
            //m.transform.position = pos + (-Vector3.up * hit.distance);
            //Debug.Log("Height =" + m.transform.position.y);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.up * 1000, Color.red);

        }
    }
}
