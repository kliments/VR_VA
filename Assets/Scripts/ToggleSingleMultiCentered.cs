﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSingleMultiCentered : MonoBehaviour {
    public SpriteRenderer sprite;
    public Sprite singleCenSpr, multiCenSpr;
    private DenclueAlgorithm denclue;
    private bool multiCentered;
    // Use this for initialization
    void Start()
    {
        denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        multiCentered = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Toggle()
    {
        if (multiCentered)
        {
            multiCentered = false;
            denclue.multiCentered = true;
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            denclue.multiCentered = false;
            sprite.sprite = singleCenSpr;
        }
        denclue.StartDenclue();
    }
}
