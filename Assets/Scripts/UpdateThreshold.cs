using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateThreshold : MonoBehaviour {
    public GameObject activateController;
    public Transform threshold;
    public PhotonView photonView;
	// Use this for initialization
	void Start () {
        activateController = GameObject.Find("[CameraRig]").transform.GetChild(2).gameObject;
        threshold = GameObject.Find("ThresholdPlane").transform;
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if (!activateController.activeSelf) activateController.SetActive(true);
        }
		if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            IncreaseThreshold();
            photonView.RPC("IncreaseThreshold", PhotonTargets.AllBuffered);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DecreaseThreshold();
            photonView.RPC("DecreaseThreshold", PhotonTargets.AllBuffered);
        }
        else if(Input.GetKeyDown(KeyCode.O))
        {
            photonView.RPC("LoadDatasetController", PhotonTargets.AllViaServer, Random.Range(0,20));
        }
        else if(Input.GetKeyDown(KeyCode.P))
        {
            photonView.RPC("LoadVisualizationController", PhotonTargets.AllViaServer, Random.Range(0, 4));
        }
        else if(Input.GetKeyDown(KeyCode.K))
        {
            //NetworkScriptController.commandSender.master = true;
            photonView.RPC("RunKMeans", PhotonTargets.AllViaServer);
        }
    }

    [PunRPC]
    void IncreaseThreshold()
    {
        threshold.position += threshold.up * 0.1f;
    }
    [PunRPC]
    void DecreaseThreshold()
    {
        threshold.position += -threshold.up * 0.1f;
    }
}
