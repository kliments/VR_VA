using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseDecrease : MonoBehaviour {

    public GameObject kMeans;
    public GameObject dbScan;
    public GameObject denclue;
    public Text text;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void IncreaseNrSpheres()
    {
        kMeans.GetComponent<KMeansAlgorithm>().ResetMe();
        if(kMeans.GetComponent<KMeansAlgorithm>().nrOfSpheres < 20)
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
        if (dbScan.GetComponent<DBScanAlgorithm>().epsilon < 0.2f)
        {
            dbScan.GetComponent<DBScanAlgorithm>().epsilon += 0.01f;
            dbScan.GetComponent<DBScanAlgorithm>().epsilon = (float)(System.Math.Round((double)dbScan.GetComponent<DBScanAlgorithm>().epsilon, 2));
        }
        text.text = "eps: " + dbScan.GetComponent<DBScanAlgorithm>().epsilon.ToString();
    }

    public void DecreaseEpsilon()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if(dbScan.GetComponent<DBScanAlgorithm>().epsilon > 0.001)
        {
            dbScan.GetComponent<DBScanAlgorithm>().epsilon -= 0.01f;
            dbScan.GetComponent<DBScanAlgorithm>().epsilon = (float)(System.Math.Round((double)dbScan.GetComponent<DBScanAlgorithm>().epsilon, 2));
        }
        text.text = "eps: " + dbScan.GetComponent<DBScanAlgorithm>().epsilon.ToString();
    }


    public void IncreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if (dbScan.GetComponent<DBScanAlgorithm>().minPts < 20)
        {
            dbScan.GetComponent<DBScanAlgorithm>().minPts++;
        }
        text.text = "minPts: " + dbScan.GetComponent<DBScanAlgorithm>().minPts.ToString();
    }

    public void DecreaseMinPts()
    {
        dbScan.GetComponent<DBScanAlgorithm>().ResetMe();
        if (dbScan.GetComponent<DBScanAlgorithm>().minPts > 2)
        {
            dbScan.GetComponent<DBScanAlgorithm>().minPts--;
        }
        text.text = "minPts: " + dbScan.GetComponent<DBScanAlgorithm>().minPts.ToString();
    }

    public void IncreaseInfluence()
    {
        denclue.GetComponent<DenclueAlgorithm>().ResetMe();
        if(denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood < 10)
        {
            denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood++;
            denclue.GetComponent<DenclueAlgorithm>().StartAlgorithm();
        }
        text.text = "ε: " + denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood.ToString();
    }

    public void DecreaseInfluence()
    {
        if (denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood > 4)
        {
            denclue.GetComponent<DenclueAlgorithm>().ResetMe();
            denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood--;
            denclue.GetComponent<DenclueAlgorithm>().StartAlgorithm();
        }
        text.text = "ε: " + denclue.GetComponent<DenclueAlgorithm>().halfLengthOfNeighbourhood.ToString();
    }

    public void IncreaseThreshold()
    {
        if (denclue.GetComponent<DenclueAlgorithm>().threshold < 1.1f)
        {
            denclue.GetComponent<DenclueAlgorithm>().threshold+=0.01f;
            Vector3 tempPos = denclue.GetComponent<DenclueAlgorithm>().thresholdPlane.transform.position;
            tempPos.y += 0.01f;
            denclue.GetComponent<DenclueAlgorithm>().thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.GetComponent<DenclueAlgorithm>().threshold,2).ToString();
    }

    public void DecreaseThreshold()
    {
        if (denclue.GetComponent<DenclueAlgorithm>().threshold > 0.02f)
        {
            denclue.GetComponent<DenclueAlgorithm>().threshold-= 0.01f;
            Vector3 tempPos = denclue.GetComponent<DenclueAlgorithm>().thresholdPlane.transform.position;
            tempPos.y -= 0.01f;
            denclue.GetComponent<DenclueAlgorithm>().thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.GetComponent<DenclueAlgorithm>().threshold,2).ToString();
    }
}
