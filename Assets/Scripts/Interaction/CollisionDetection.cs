using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public enum ClipType
    {
        HideMesh,
        SliceMesh,
        DoNothing
    }

    public ClipType Clipping;


    public Material IntersectFillMaterial;
    /// <summary>
    /// Continously checks if any object would stick out of the board, and disables them accordingly.
    /// </summary>

    List<GameObject> gos = new List<GameObject>();
       
    void Update()
    {
        if (Clipping == ClipType.DoNothing) return;
        // Disable any placed or loaded-in object that sticks out of the board box.
        Collider[] collidingObjects = Physics.OverlapBox(transform.position, transform.localScale, transform.rotation);
        foreach (Collider col in collidingObjects)
        {
            if (col.gameObject.tag == "boardobject" || col.gameObject.name.Contains("Buildings") || ChildOfTiles(col.gameObject))
            {
                switch(Clipping)
                {
                    case ClipType.HideMesh:
                        col.gameObject.SetActive(false);
                        break;
                    case ClipType.SliceMesh:
                        if (!gos.Contains(col.gameObject))
                        {
                            gos.Add(col.gameObject);
                            // https://github.com/DavidArayan/EzySlice
                            var res = col.gameObject.Slice(transform.position + transform.forward * 0.1f /* add small offset*/, transform.forward,
                                new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), IntersectFillMaterial);
                            if (res != null)
                            {
                                col.gameObject.GetComponent<MeshFilter>().sharedMesh = res.upperHull;
                                // index 0 is one side, index 1 is other side

                                //res[0].transform.SetParent(col.gameObject.transform, false);
                                //res[1].transform.SetParent(col.gameObject.transform, false);

                            }

                        }
                        break;
                }
                

                //if (!pieces[1].GetComponent<Rigidbody>())
                  //  pieces[1].AddComponent<Rigidbody>();

                ///Destroy(pieces[1], 1);
                //col.gameObject.SetActive(false);
            }
        }
    }

    public bool ChildOfTiles(GameObject pGo)
    {
        while (pGo.transform.parent != null)
        {
            pGo = pGo.transform.parent.gameObject;
            if (pGo.name == "Tiles")
            {
                return true;
            }
        }
        return false;
    }
}
