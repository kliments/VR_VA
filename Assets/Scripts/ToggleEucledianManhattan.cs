﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEucledianManhattan : MonoBehaviour {
    public SpriteRenderer sprite;
    public Sprite euclideanSpr, mannhattanSpr;
    private DBScanAlgorithm distance;
    private bool eucledian;
	// Use this for initialization
	void Start () {
        distance = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        eucledian = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Toggle()
    {
        //reset DBSCAN algorithm
        distance.ResetMe();
        if (eucledian)
        {
            eucledian = false;
            distance.euclDist = true;
            sprite.sprite = euclideanSpr;
        }
        else
        {
            eucledian = true;
            distance.euclDist = false;
            sprite.sprite = mannhattanSpr;
        }
    }
}
