using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowProperAlgorithmButtons : MonoBehaviour {
    public GameObject kMeansParent, dbScanParent, denclueParent, responsiveMenu, algorithmsParent, menusParent;
    private BackButtonMenu backButton;
	// Use this for initialization
	void Start () {
        responsiveMenu = gameObject;
        while(responsiveMenu.name != "ResponsiveMenu")
        {
            responsiveMenu = responsiveMenu.transform.parent.gameObject;
        }
        menusParent = gameObject;
        while (menusParent.name != "MenusParent")
        {
            menusParent = menusParent.transform.parent.gameObject;
        }
        foreach(Transform child in responsiveMenu.transform)
        {
            if (child.name == "AlgorithmsParent") algorithmsParent = child.gameObject;
            else if (child.name == "KMeansParent") kMeansParent = child.gameObject;
            else if (child.name == "DBSCANParent") dbScanParent = child.gameObject;
            else if (child.name == "DENCLUEParent") denclueParent = child.gameObject;
        }
        backButton = menusParent.GetComponent<BackButtonMenu>();
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
            kMeansParent.GetComponentInChildren<KMeansAlgorithm>().pseudoCodeText.SetActive(true);
            dbScanParent.GetComponentInChildren<DBScanAlgorithm>().pseudoCodeText.SetActive(false);
        }
        else if(this.name == "DBSCAN")
        {
            dbScanParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbScanParent);
            backButton.previousMenus.Add(dbScanParent);
            kMeansParent.GetComponentInChildren<KMeansAlgorithm>().pseudoCodeText.SetActive(false);
            dbScanParent.GetComponentInChildren<DBScanAlgorithm>().pseudoCodeText.SetActive(true);
        }
        else
        {
            denclueParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            backButton.previousMenus.Add(denclueParent);
        }
    }
}
