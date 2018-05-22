using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationSelectedSpriteToggle : MonoBehaviour {

    public GameObject spriteObj, visualizationButtonSpriteObj;
    public Sprite sprite, visualizationButtonSprite;
    // Use this for initialization
    void Start()
    {
        spriteObj = GameObject.Find("DatasetVizSprite");
        visualizationButtonSpriteObj = GameObject.Find("ResponsiveMenu").transform.GetChild(0).transform.GetChild(1).transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSprite()
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = sprite;
        visualizationButtonSpriteObj.GetComponent<SpriteRenderer>().sprite = visualizationButtonSprite;
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = null;
    }
}
