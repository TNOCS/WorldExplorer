using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SymbolTargetHandler : MonoBehaviour
{

    [SerializeField]
    public GameObject gui;
    [SerializeField]
    private GameObject cursor;
    private bool selected = false;
    private Material selectedMat;
    private Material oldMat;
    private Renderer coneRender;
    // Use this for initialization
    void Start()
    {
        selectedMat = (Material)Resources.Load("Materials/cone-Color-J04", typeof(Material));
        coneRender = transform.FindChild("cone/Cone with Right Triangle/Component").gameObject.GetComponent<Renderer>();
        oldMat = coneRender.material;
        cursor = GameObject.Find("Cursor");
    }
    void OnSelect()
    {
        selected = !selected;

        // If the user is in placing mode, display the spatial mapping mesh.
        if (selected)
        {
            gui.SetActive(true);
            coneRender.material = selectedMat;
        }
        // If the user is not in placing mode, hide the spatial mapping mesh.
        else
        {
            coneRender.material = oldMat;
            gui.SetActive(false);
        }
    }
    public void Show()
    {
        if (!selected)
            gui.SetActive(true);
    }
    public void Hide()
    {
        if (!selected)
            gui.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            // Do a raycast into the world that will only hit the Spatial Mapping mesh.

            // Move this object's parent object to
            // where the raycast hit the Spatial Mapping mesh.
            this.transform.position = new Vector3(cursor.transform.position.x, this.transform.position.y, cursor.transform.position.z);
            
        }
    }
}
