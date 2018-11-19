using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticLoadDataVisDenclue : MonoBehaviour {
    public GameObject data, viz, denclue;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadData()
    {
        data.transform.GetChild(0).gameObject.GetComponent<datasetChangerScript>().isSelected = true;
        data.transform.GetChild(0).gameObject.GetComponent<UniversalButtonScript>().loadDataset = true;
    }
    void LoadVis()
    {
        viz.GetComponent<VisualizationChangerScript>().isSelected = true;
        viz.GetComponent<UniversalButtonScript>().loadVis = true;
    }
    void LoadDenclue()
    {
        denclue.GetComponent<UniversalButtonScript>().startDenclue = true;
    }

    private void OnEnable()
    {
        Invoke("LoadData", 0.5f);
        Invoke("LoadVis", 0.6f);
        Invoke("LoadDenclue", 1.2f);
    }
}
