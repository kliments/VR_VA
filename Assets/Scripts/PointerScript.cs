using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if(other.gameObject.GetComponent<UniversalButtonScript>() != null)
        {
            other.gameObject.GetComponent<UniversalButtonScript>().isHover = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponent<UniversalButtonScript>() != null)
        {
            other.gameObject.GetComponent<UniversalButtonScript>().isHover = false;
        }
    }
}
