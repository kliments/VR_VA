using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SwitchPseudoCodeText : NetworkBehaviour {
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbScan;
    public DenclueAlgorithm denclue;

    // Use this for initialization
    void Start () {
        kMeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        dbScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SwitchText(int i)
    {
        CmdSwitchText(i);
    }

    [Command]
    private void CmdSwitchText(int i)
    {
        if (!isServer) return;
        if(i==0)
        {
            kMeans.pseudoCodeText.SetActive(true);
            dbScan.pseudoCodeText.SetActive(false);
        }
        else if(i==1)
        {
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(true);
        }
        else
        {
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(false);
        }
        RpcSwitchText(i);
    }

    [ClientRpc]
    private void RpcSwitchText(int i)
    {
        if (hasAuthority) return;
        if (i == 0)
        {
            kMeans.pseudoCodeText.SetActive(true);
            dbScan.pseudoCodeText.SetActive(false);
        }
        else if (i == 1)
        {
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(true);
        }
        else
        {
            kMeans.pseudoCodeText.SetActive(false);
            dbScan.pseudoCodeText.SetActive(false);
        }
    }
}
