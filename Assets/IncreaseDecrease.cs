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
	void Start ()
    {
        kMeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        dbScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    public void CmdIncreaseNrSpheres()
    {
        if (!isServer) return;
        kMeans.ResetMe();
        if(kMeans.nrOfSpheres < 20)
        {
            kMeans.nrOfSpheres++;
        }
        text.text = "K: " + kMeans.nrOfSpheres.ToString();
        kMeans.pseudoCodeText.SetActive(true);
        RpcResetNrSpheres();
    }

    [Command]
    public void CmdDecreaseNrSpheres()
    {
        if (!isServer) return;
        kMeans.ResetMe();
        if (kMeans.nrOfSpheres > 2)
        {
            kMeans.nrOfSpheres--;
        }
        text.text = "K: " + kMeans.nrOfSpheres.ToString();
        kMeans.pseudoCodeText.SetActive(true);
        RpcResetNrSpheres();
    }

    [ClientRpc]
    public void RpcResetNrSpheres()
    {
        if (hasAuthority) return;
        kMeans.ResetMe(); text.text = "K: " + kMeans.nrOfSpheres.ToString();
        kMeans.pseudoCodeText.SetActive(true);
    }

    [Command]
    public void CmdIncreaseEpsilon()
    {
        if (!isServer) return;
        dbScan.ResetMe();
        if (dbScan.epsilon < 0.2f)
        {
            dbScan.epsilon += 0.01f;
            dbScan.epsilon = (float)(System.Math.Round((double)dbScan.epsilon, 2));
        }
        text.text = "eps: " + dbScan.epsilon.ToString();
        dbScan.pseudoCodeText.SetActive(true);
        RpcResetEpsilon();
    }
    
    [Command]
    public void CmdDecreaseEpsilon()
    {
        if (!isServer) return;
        dbScan.ResetMe();
        if(dbScan.epsilon > 0.001)
        {
            dbScan.epsilon -= 0.01f;
            dbScan.epsilon = (float)(System.Math.Round((double)dbScan.epsilon, 2));
        }
        text.text = "eps: " + dbScan.epsilon.ToString();
        dbScan.pseudoCodeText.SetActive(true);
        RpcResetEpsilon();
    }

    [ClientRpc]
    public void RpcResetEpsilon()
    {
        if (hasAuthority) return;
        dbScan.ResetMe();
        text.text = "eps: " + dbScan.epsilon.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }

    [Command]
    public void CmdIncreaseMinPts()
    {
        if (!isServer) return;
        dbScan.ResetMe();
        if (dbScan.minPts < 20)
        {
            dbScan.minPts++;
        }
        text.text = "minPts: " + dbScan.minPts.ToString();
        dbScan.pseudoCodeText.SetActive(true);
        RpcResetMinPts();
    }

    [Command]
    public void CmdDecreaseMinPts()
    {
        if (!isServer) return;
        dbScan.ResetMe();
        if (dbScan.minPts > 2)
        {
            dbScan.minPts--;
        }
        text.text = "minPts: " + dbScan.minPts.ToString();
        dbScan.pseudoCodeText.SetActive(true);
        RpcResetMinPts();
    }

    [ClientRpc]
    public void RpcResetMinPts()
    {
        if (hasAuthority) return;
        dbScan.ResetMe();
        text.text = "minPts: " + dbScan.minPts.ToString();
        dbScan.pseudoCodeText.SetActive(true);
    }

    [Command]
    public void CmdIncreaseInfluence()
    {
        if (!isServer) return;
        denclue.ResetMe();
        if(denclue.halfLengthOfNeighbourhood < 10)
        {
            denclue.halfLengthOfNeighbourhood++;
            denclue.CmdStartDenclue();
        }
        text.text = "ε: " + denclue.halfLengthOfNeighbourhood.ToString();
        RpcResetInfluence();
    }

    [Command]
    public void CmdDecreaseInfluence()
    {
        if (!isServer) return;
        if (denclue.halfLengthOfNeighbourhood > 4)
        {
            denclue.ResetMe();
            denclue.halfLengthOfNeighbourhood--;
            denclue.CmdStartDenclue();
        }
        text.text = "ε: " + denclue.halfLengthOfNeighbourhood.ToString();
        RpcResetInfluence();
    }

    [ClientRpc]
    public void RpcResetInfluence()
    {
        if (hasAuthority) return;
        denclue.ResetMe();
        text.text = "ε: " + denclue.halfLengthOfNeighbourhood.ToString();
    }

    [Command]
    public void CmdIncreaseThreshold()
    {
        if (!isServer) return;
        if (denclue.threshold < 1.1f)
        {
            denclue.threshold+=0.01f;
            Vector3 tempPos = denclue.thresholdPlane.transform.position;
            tempPos.y += 0.01f;
            denclue.thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.threshold,2).ToString();
        RpcResetThreshold();
    }

    [Command]
    public void CmdDecreaseThreshold()
    {
        if (!isServer) return;
        if (denclue.threshold > 0.02f)
        {
            denclue.threshold-= 0.01f;
            Vector3 tempPos = denclue.thresholdPlane.transform.position;
            tempPos.y -= 0.01f;
            denclue.thresholdPlane.transform.position = tempPos;
        }
        text.text = "ξ: " + decimal.Round((decimal)denclue.threshold,2).ToString();
        RpcResetThreshold();
    }

    [ClientRpc]
    public void RpcResetThreshold()
    {
        if (hasAuthority) return;
        text.text = "ξ: " + decimal.Round((decimal)denclue.threshold, 2).ToString();
    }
}
