using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticLoadDataVisDenclue : MonoBehaviour {
    public GameObject data, viz, denclue, dbscan, kmeans;
    public int i;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadData()
    {
        data.transform.GetChild(i).gameObject.GetComponent<datasetChangerScript>().startTargetedAction();
    }
    void LoadVis()
    {
        viz.GetComponent<VisualizationChangerScript>().startSelectedAction();
    }
    void LoadDenclue()
    {
        denclue.GetComponent<UniversalButtonScript>().denclue.StartDenclue();
    }
    void LoadDBscan()
    {
        dbscan.GetComponent<DBScanAlgorithm>().StartDBSCAN();
    }
    void LoadKmeans()
    {
        kmeans.GetComponent<KMeansAlgorithm>().StartAlgorithm();
    }

    private void OnEnable()
    {
        Invoke("LoadData", 1f);
        Invoke("LoadVis", 1.1f);
        //Invoke("LoadDenclue", 1.2f);
        //Invoke("LoadDBscan", 1.2f);
        //Invoke("LoadKmeans", 1.2f);
    }
}
