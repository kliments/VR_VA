using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetSpriteChanger : MonoBehaviour {

    public Sprite actualSprite;
    public GameObject spriteObj;
	// Use this for initialization
	void Start ()
    {
        spriteObj = GameObject.Find("PCAProjectionSprite");
    }
	
	// Update is called once per frame
	void Update () {
		if(GetComponent<GeneralCoverflowProperties>().isHovered)
        {
            spriteObj.GetComponent<SpriteRenderer>().sprite = actualSprite;
        }
	}
}
