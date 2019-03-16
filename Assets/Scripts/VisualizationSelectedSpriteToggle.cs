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
        spriteObj.SetActive(true);
        visualizationButtonSpriteObj.SetActive(true);
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        visualizationButtonSpriteObj.SetActive(false);
    }
}
