using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseDecrease : MonoBehaviour {

    public GameObject kMeans;
    public GameObject dbScan;
    public Text text;
    public float difference;
	// Use this for initialization
	void Start () {
        difference = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void IncreaseNrSpheres()
    {
        kMeans.GetComponent<KMeansAlgorithm>().ResetMe();
        if(kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres <= 20)
        {
            kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres++;
        }
        text.text = "K: " + kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres.ToString();
    }

    public void DecreaseNrSpheres()
    {
        kMeans.GetComponent<KMeansAlgorithm>().ResetMe();
        if (kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres > 2)
        {
            kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres--;
        }
        text.text = "K: " + kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres.ToString();
    }

    public void IncreaseEpsilon()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if (dbScan.GetComponent<DBScanAlgorithm>().epsilon <= 0.2f)
        {
            dbScan.GetComponent<DBScanAlgorithm>().epsilon += 0.01f;
        }
        text.text = "eps: " + dbScan.GetComponent<DBScanAlgorithm>().epsilon.ToString();
    }

    public void DecreaseEpsilon()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if(dbScan.GetComponent<DBScanAlgorithm>().epsilon > 0.001)
        {
            dbScan.GetComponent<DBScanAlgorithm>().epsilon -= 0.01f;
        }
        text.text = "eps: " + dbScan.GetComponent<DBScanAlgorithm>().epsilon.ToString();
    }

    public void IncreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if(dbScan.GetComponent<DBScanAlgorithm>().minPts <= 20)
        {
            dbScan.GetComponent<DBScanAlgorithm>().minPts++;
        }
        text.text = "minPts: " + dbScan.GetComponent<DBScanAlgorithm>().minPts;
    }

    public void DecreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if(dbScan.GetComponent<DBScanAlgorithm>().minPts > 2)
        {
            dbScan.GetComponent<DBScanAlgorithm>().minPts--;
        }
        text.text = "minPts: " + dbScan.GetComponent<DBScanAlgorithm>().minPts;
    }

}
