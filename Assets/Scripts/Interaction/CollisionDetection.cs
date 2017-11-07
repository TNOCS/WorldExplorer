using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    void Update()
    {
        // Disabled any placed or loaded-in object that sticks out of the board box.
        Collider[] collidingObjects = Physics.OverlapBox(transform.position, transform.localScale, transform.rotation);
        foreach (Collider col in collidingObjects)
        {
            if (col.gameObject.tag == "boardobject" || col.gameObject.name.Contains("Buildings"))
            {
               col.gameObject.SetActive(false);
            }
        }
    }
}
