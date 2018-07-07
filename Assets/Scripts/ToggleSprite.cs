using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSprite : MonoBehaviour {
    public GameObject datasetParent;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(!datasetParent.activeSelf && GetComponent<SpriteRenderer>().sprite!=null)
        {
            GetComponent<SpriteRenderer>().sprite = null;
        }
	}
}
