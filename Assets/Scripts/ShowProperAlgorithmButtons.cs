using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if (kMeans == null) kMeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        if (dbScan == null) dbScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        if (denclue == null) denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        if (this.name == "K-Means")
        {
            kMeansParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(kMeansParent);
            backButton.previousMenus.Add(kMeansParent);
            kMeans.pseudoCodeText.SetActive(true);
            dbScan.pseudoCodeText.SetActive(false);
            pseudoTextChanger.SwitchText(0);
            if(kMeans.GetComponent<IncreaseDecrease>().text == null) kMeans.GetComponent<IncreaseDecrease>().text = kMeansParent.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
        }
        else if(this.name == "DBSCAN")
        {
            dbScanParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbScanParent);
            backButton.previousMenus.Add(dbScanParent);
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(true);
            pseudoTextChanger.SwitchText(1);
            if(dbScan.GetComponents<IncreaseDecrease>()[0].text == null) dbScan.GetComponents<IncreaseDecrease>()[0].text = dbScanParent.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
            if(dbScan.GetComponents<IncreaseDecrease>()[1].text == null) dbScan.GetComponents<IncreaseDecrease>()[1].text = dbScanParent.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
        }
        else
        {
            denclueParent.SetActive(true);
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            backButton.previousMenus.Add(denclueParent);
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(false);
            pseudoTextChanger.SwitchText(2);
            if(denclue.GetComponents<IncreaseDecrease>()[0].text == null) denclue.GetComponents<IncreaseDecrease>()[0].text = denclueParent.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
            if(denclue.GetComponents<IncreaseDecrease>()[1].text == null) denclue.GetComponents<IncreaseDecrease>()[1].text = denclueParent.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
        }
        algorithmsParent.SetActive(false);
    }
}
