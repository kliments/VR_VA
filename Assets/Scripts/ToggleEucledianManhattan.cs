using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEucledianManhattan : MonoBehaviour {
    public Text text;
    private DBScanAlgorithm distance;
    private bool eucledian;
	// Use this for initialization
	void Start () {
        distance = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        eucledian = false;
        text.text = "Manhattan";
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
            text.text = "Eucledian";
        }
        else
        {
            eucledian = true;
            distance.euclDist = false;
            text.text = "Manhattan";
        }
    }
}
