using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticLoadDataVisDenclue : MonoBehaviour {
    public GameObject data, viz, denclue;
    public int i;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadData()
    {
        /*data.transform.GetChild(i).gameObject.GetComponent<datasetChangerScript>().isSelected = true;
        data.transform.GetChild(i).gameObject.GetComponent<UniversalButtonScript>().loadDataset = true;*/
        data.transform.GetChild(i).gameObject.GetComponent<datasetChangerScript>().startTargetedAction();
    }
    void LoadVis()
    {
        /*viz.GetComponent<VisualizationChangerScript>().isSelected = true;
        viz.GetComponent<UniversalButtonScript>().loadVis = true;*/
        viz.GetComponent<VisualizationChangerScript>().startSelectedAction();
    }
    void LoadDenclue()
    {
        denclue.GetComponent<UniversalButtonScript>().denclue.StartAlgorithm();
    }

    private void OnEnable()
    {
        Invoke("LoadData", 1f);
        Invoke("LoadVis", 1.1f);
        Invoke("LoadDenclue", 1.2f);
    }
}
