using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledDisabled : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDisable()
    {
        Debug.Log("PrintOnDisable: script was disabled");
    }

    void OnEnable()
    {
        Debug.Log("PrintOnEnable: script was enabled");
    }

}
