using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonCustom<InventoryManager>
{

    public List<GameObject> itemsInInventory = new List<GameObject>();

    void Start()
    {
        //  // Retrieves all prefabs from the Inventory folder in Resources and add them to a list
        //  Object[] retrievedInventoryItems = Resources.LoadAll("Prefabs/Inventory", typeof(GameObject));
        //  foreach (GameObject retrievedObject in retrievedInventoryItems)
        //  {            
        //      GameObject go = (GameObject)retrievedObject;
        //      Debug.Log("Retrieved prefab" + go);
        //      allInventoryItems.Add(go);
        //  }


    }

    // Sets item visibility based on the minimum zoom level of the item.
    public void SetItemVisibility()
    {
        // Retrieves all items positions and adds them to a list if this hasn't been done yet.
     // if (itemsInInventory.Count == 0)
     // {
     //     var inventoryItems = GameObject.Find("InventoryItems");
     //     for (int i = 0; i < inventoryItems.transform.childCount; i++)
     //     {
     //
     //         Debug.Log("Retrieving item" + i);
     //         var go = inventoryItems.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject;
     //         itemsInInventory.Add(go);
     //     }
     // }
            //  // Sets objects active or inactive based on zoom level.
            //  foreach (GameObject go in itemsInInventory)
            //  {
            //      if (go.GetComponent<InventoryObjectData>().minZoomLevel <= AppState.Instance.Config.ActiveView.Zoom)
            //      {
            //          go.SetActive(true);
            //      }
            //      else
            //      {
            //          go.SetActive(false);
            //      }
            //  }
        }
    }


//  [SerializeField]
//  private List<GameObject> allInventoryItems = new List<GameObject>();
//  [SerializeField]
//  private List<GameObject> visibleInventoryItems = new List<GameObject>();
//
//  private List<GameObject> itemPositions = new List<GameObject>();
//  private int visibleItems = 12;
//  [SerializeField]
//  private Dictionary<int, Vector3> positionDict = new Dictionary<int, Vector3>();


//positionDict[i] = inventoryItems.transform.GetChild(i).gameObject.transform.position;


// Instantiate and set each item to the correct position
// var count = 0;
// foreach (GameObject go in allInventoryItems)
// {
//     var obj = Instantiate(go) as GameObject;
//     var emptyGo = new GameObject();
//     obj.transform.SetParent(emptyGo.transform, false);
//     if (count < visibleItems)
//     {
//         emptyGo.transform.SetParent(inventoryItems.transform.GetChild(count), false);
//         //obj.transform.localScale = obj.transform.localScale;
//         Debug.Log("Setting Position of item" + count);
//         emptyGo.transform.position = positionDict[count];
//
//     }
//     else
//     {
//         Debug.Log("Set inactive " + obj);
//         obj.SetActive(false);
//     }
//     count++;
// }