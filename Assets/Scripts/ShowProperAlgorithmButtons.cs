using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowProperAlgorithmButtons : MonoBehaviour {
    public GameObject kMeansParent, dbScanParent, denclueParent, responsiveMenu, algorithmsParent, menusParent;
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbScan;
    public DenclueAlgorithm denclue;
    private BackButtonMenu backButton;
    private SwitchPseudoCodeText pseudoTextChanger;
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
        pseudoTextChanger = (SwitchPseudoCodeText)FindObjectOfType(typeof(SwitchPseudoCodeText));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void ButtonPressed()
    {
        if (this.name == "K-Means")
        {
            kMeansParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(kMeansParent);
            backButton.previousMenus.Add(kMeansParent);
            kMeans.pseudoCodeText.SetActive(true);
            dbScan.pseudoCodeText.SetActive(false);
            pseudoTextChanger.SwitchText(0);
        }
        else if(this.name == "DBSCAN")
        {
            dbScanParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbScanParent);
            backButton.previousMenus.Add(dbScanParent);
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(true);
            pseudoTextChanger.SwitchText(1);
        }
        else
        {
            denclueParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            backButton.previousMenus.Add(denclueParent);
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(false);
            pseudoTextChanger.SwitchText(2);
        }
        algorithmsParent.SetActive(false);
    }
}
