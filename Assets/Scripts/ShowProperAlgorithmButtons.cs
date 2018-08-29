using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowProperAlgorithmButtons : MonoBehaviour {
    public GameObject kMeansParent, dbScanParent, denclueParent, responsiveMenu, algorithmsParent, menusParent;
    private BackButtonMenu backButton;
	// Use this for initialization
	void Start () {
        responsiveMenu = GameObject.Find("ResponsiveMenu");
        menusParent = GameObject.Find("MenusParent");
        foreach(Transform child in responsiveMenu.transform)
        {
            if (child.gameObject.name == "AlgorithmsParent") algorithmsParent = child.gameObject;
            else if (child.gameObject.name == "KMeansParent") kMeansParent = child.gameObject;
            else if (child.gameObject.name == "DBSCANParent") dbScanParent = child.gameObject;
            else if (child.gameObject.name == "DENCLUEParent") denclueParent = child.gameObject;
        }
        backButton = (BackButtonMenu)FindObjectOfType(typeof(BackButtonMenu));
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
            menusParent.GetComponent<CoverflowScript>().AssignValues(kMeansParent);
            backButton.previousMenus.Add(kMeansParent);
        }
        else if(this.name == "DBSCAN")
        {
            dbScanParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbScanParent);
            backButton.previousMenus.Add(dbScanParent);
        }
        else
        {
            denclueParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            backButton.previousMenus.Add(denclueParent);
        }
    }
}
