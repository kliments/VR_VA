using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDecrease : MonoBehaviour {

    public GameObject kMeans;
    public GameObject dbScan;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void IncreaseNrSpheres()
    {
        kMeans.GetComponent<KMeansAlgorithm>().ResetMe();
        kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres++;
    }

    public void DecreaseNrSpheres()
    {
        kMeans.GetComponent<KMeansAlgorithm>().ResetMe();
        kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres--;
    }

    public void IncreaseEpsilon()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        dbScan.GetComponent<DBScanAlgorithm>().epsilon += 0.01f;
    }

    public void DecreaseEpsilon()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        dbScan.GetComponent<DBScanAlgorithm>().epsilon -= 0.01f;
    }

    public void IncreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        dbScan.GetComponent<DBScanAlgorithm>().minPts++;
    }

    public void DecreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        dbScan.GetComponent<DBScanAlgorithm>().minPts--;
    }

}
