using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    /// <summary>
    /// Continously checks if any object would stick out of the board, and disables them accordingly.
    /// </summary>

    void Update()
    {
        // Disable any placed or loaded-in object that sticks out of the board box.
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
