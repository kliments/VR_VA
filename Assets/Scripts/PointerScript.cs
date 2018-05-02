using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerScript : MonoBehaviour {
    public bool pointerCollides;
    public GameObject collider;
	// Use this for initialization
	void Start () {
        pointerCollides = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<UniversalButtonScript>() != null)
        {
            other.gameObject.GetComponent<UniversalButtonScript>().isHover = true;
            pointerCollides = true;
            collider = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponent<UniversalButtonScript>() != null)
        {
            other.gameObject.GetComponent<UniversalButtonScript>().isHover = false;
            pointerCollides = false;
        }
    }
}
