using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolTargetHandler : MonoBehaviour
{

    [SerializeField]
    public GameObject gui;
    private bool selected = false;
    // Use this for initialization
    void Start()
    {

    }
    void OnSelect()
    {
        selected = !selected;

        // If the user is in placing mode, display the spatial mapping mesh.
        if (selected)
        {
            gui.SetActive(true);

        }
        // If the user is not in placing mode, hide the spatial mapping mesh.
        else
        {
            gui.SetActive(false);
        }
    }
    public void ToggleGUI()
    {
        if(!selected)
        gui.SetActive(!gui.active);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
