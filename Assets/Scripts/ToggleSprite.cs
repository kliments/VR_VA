using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSprite : MonoBehaviour {
    public GameObject datasetParent, datasetVizSpriteObj;
    private SpriteRenderer dataVizSprite;
	// Use this for initialization
	void Start () {
        dataVizSprite = datasetVizSpriteObj.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if(!datasetParent.activeSelf || dataVizSprite.sprite!=null)
        {
            GetComponent<SpriteRenderer>().sprite = null;
        }
	}
}
