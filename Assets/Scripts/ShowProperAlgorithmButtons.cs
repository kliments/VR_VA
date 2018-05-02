using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowProperAlgorithmButtons : MonoBehaviour {
    public GameObject kMeansParent, dbScanParent, responsiveMenu, algorithmsParent;
	// Use this for initialization
	void Start () {
        responsiveMenu = GameObject.Find("ResponsiveMenu");
        foreach(Transform child in responsiveMenu.transform)
        {
            if (child.gameObject.name == "AlgorithmsParent") algorithmsParent = child.gameObject;
            else if (child.gameObject.name == "KMeansParent") kMeansParent = child.gameObject;
            else if (child.gameObject.name == "DBSCANParent") dbScanParent = child.gameObject;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ButtonPressed()
    {
        algorithmsParent.SetActive(false);
        if (this.name == "K-Means")
        {
            kMeansParent.SetActive(true);
        }
        else
        {
            dbScanParent.SetActive(true);
        }
    }
}
