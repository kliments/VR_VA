using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationSelectedSpriteToggle : MonoBehaviour {

    public GameObject spriteObj, visualizationButtonSpriteObj;
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

        if (spriteObj == null || visualizationButtonSpriteObj == null)
        {
            //checkmark that dataset has been selected
            spriteObj = GameObject.Find("PrimaryMenuParent").transform.GetChild(0).transform.GetChild(0).gameObject;
            //sprite notification that disappears after 3 seconds
            visualizationButtonSpriteObj = GameObject.Find("MenusParent").transform.GetChild(6).gameObject;
        }
        spriteObj.SetActive(true);
        visualizationButtonSpriteObj.SetActive(true);
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        visualizationButtonSpriteObj.SetActive(false);
    }
}
