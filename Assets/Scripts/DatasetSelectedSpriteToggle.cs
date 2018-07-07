using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetSelectedSpriteToggle : MonoBehaviour {

    public GameObject spriteObj, datasetSelectedSprite;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSprite()
    {
        if(spriteObj==null || datasetSelectedSprite==null)
        {
            //checkmark that dataset has been selected
            spriteObj = GameObject.Find("PrimaryMenuParent").transform.GetChild(0).transform.GetChild(0).gameObject;
            //sprite notification that disappears after 3 seconds
            datasetSelectedSprite = GameObject.Find("MenusParent").transform.GetChild(5).gameObject;
        }
        spriteObj.SetActive(true);
        datasetSelectedSprite.SetActive(true);
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        datasetSelectedSprite.SetActive(false);
    }
}
