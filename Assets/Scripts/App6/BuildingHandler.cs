using UnityEngine;

using System.Collections;



public class BuildingHandler : MonoBehaviour
{



    public bool Active { get; set; }

    private Color _defaultColor;

    private MeshRenderer _renderer;



    // Use this for initialization

    void Start()
    {

        _renderer = gameObject.GetComponent<MeshRenderer>();

        _defaultColor = _renderer.material.color;



    }



    public void Tap()

    {

        Debug.Log("Building tapped " + gameObject.name);

    }



    public void Gaze(bool active)

    {

        Active = active;

        _renderer.material.color = (active) ? Color.red : _defaultColor;

    }



    // Update is called once per frame

    void Update()
    {



    }

}