using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetSelectedSpriteToggle : MonoBehaviour {

    public GameObject spriteObj, datasetButtonSpriteObj;
    public Sprite sprite,datasetButtonSprite;
    // Use this for initialization
    void Start()
    {
        spriteObj = GameObject.Find("DatasetVizSprite");
        datasetButtonSpriteObj = GameObject.Find("ResponsiveMenu").transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSprite()
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = sprite;
        datasetButtonSpriteObj.GetComponent<SpriteRenderer>().sprite = datasetButtonSprite;
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = null;
    }
}
