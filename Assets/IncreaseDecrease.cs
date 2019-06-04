using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class IncreaseDecrease : NetworkBehaviour {

    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbScan;
    public DenclueAlgorithm denclue;
    public Text text;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void IncreaseNrSpheres()
    {
        kMeans.ResetMe();
        if(kMeans.nrOfSpheres < 20)
        {
            kMeans.nrOfSpheres++;
        }
        text.text = "K: " + kMeans.nrOfSpheres.ToString();
        kMeans.pseudoCodeText.SetActive(true);
    }

    public void DecreaseNrSpheres()
    {
        kMeans.ResetMe();
        if (kMeans.nrOfSpheres > 2)
        {
            kMeans.nrOfSpheres--;
        }
        text.text = "K: " + kMeans.nrOfSpheres.ToString();
        kMeans.pseudoCodeText.SetActive(true);
    }

    public void IncreaseEpsilon()
    {
        dbScan.ResetMe();
        if (dbScan.epsilon < 0.2f)
        {
            dbScan.epsilon += 0.01f;
            dbScan.epsilon = (float)(System.Math.Round((double)dbScan.epsilon, 2));
        }
        text.text = "eps: " + dbScan.epsilon.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }

    public void DecreaseEpsilon()
    {
        dbScan.ResetMe();
        if(dbScan.epsilon > 0.001)
        {
            dbScan.epsilon -= 0.01f;
            dbScan.epsilon = (float)(System.Math.Round((double)dbScan.epsilon, 2));
        }
        text.text = "eps: " + dbScan.epsilon.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }


    public void IncreaseMinPts()
    {
        dbScan.ResetMe();
        if (dbScan.minPts < 20)
        {
            dbScan.minPts++;
        }
        text.text = "minPts: " + dbScan.minPts.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }

    public void DecreaseMinPts()
    {
        dbScan.ResetMe();
        if (dbScan.minPts > 2)
        {
            dbScan.minPts--;
        }
        text.text = "minPts: " + dbScan.minPts.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }

    public void IncreaseInfluence()
    {
        denclue.ResetMe();
        if(denclue.halfLengthOfNeighbourhood < 10)
        {
            denclue.halfLengthOfNeighbourhood++;
            denclue.CmdStartDenclue();
        }
        text.text = "ε: " + denclue.halfLengthOfNeighbourhood.ToString();
    }

    public void DecreaseInfluence()
    {
        if (denclue.halfLengthOfNeighbourhood > 4)
        {
            denclue.ResetMe();
            denclue.halfLengthOfNeighbourhood--;
            denclue.CmdStartDenclue();
        }
        text.text = "ε: " + denclue.halfLengthOfNeighbourhood.ToString();
    }

    public void IncreaseThreshold()
    {
        if (denclue.threshold < 1.1f)
        {
            denclue.threshold+=0.01f;
            Vector3 tempPos = denclue.thresholdPlane.transform.position;
            tempPos.y += 0.01f;
            denclue.thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.threshold,2).ToString();
    }

    public void DecreaseThreshold()
    {
        if (denclue.threshold > 0.02f)
        {
            denclue.threshold-= 0.01f;
            Vector3 tempPos = denclue.thresholdPlane.transform.position;
            tempPos.y -= 0.01f;
            denclue.thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.threshold,2).ToString();
    }
}
