using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.W))
        {
            transform.position += transform.forward * 0.1f;
        }
        else if(Input.GetKeyDown(KeyCode.S))
        {
            transform.position += -transform.forward * 0.1f;
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            transform.position += -transform.right * 0.1f;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position += transform.right * 0.1f;
        }
    }
}
