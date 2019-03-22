using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkScriptController : MonoBehaviour {
    /*
     * Script for parsing commands between players
     * for calling script methods
     */
    public object[] param;
    public PhotonView photonView;
    public static NetworkScriptController commandSender;
    public int index;

    public GameObject responsiveMenu, datasetsParent, visualizationsParent, kMeansParent, dbScanParent, denclueParent, silhouetteCoefficient;
    // Use this for initialization
    void Start () {
        photonView = GetComponent<PhotonView>();
        SetParents();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        if (commandSender == null) commandSender = this;
    }
    private void OnDisable()
    {
        if (commandSender == this) commandSender = null;
    }

    public void LoadDataset()
    {
        photonView.RPC("LoadDatasetController", PhotonTargets.AllBuffered,index);
    }
    public void LoadVisualization()
    {
        photonView.RPC("LoadVisualizationController", PhotonTargets.AllBuffered, index);
    }

    private void SetParents()
    {
        if (responsiveMenu == null)
        {
            responsiveMenu = GameObject.Find("ResponsiveMenu");
        }
        else
        {
            foreach (Transform child in responsiveMenu.transform)
            {
                if (child.name == "DatasetParent")
                {
                    datasetsParent = child.gameObject;
                }
                else if (child.name == "VisualizationsParent")
                {
                    visualizationsParent = child.gameObject;
                }
            }
        }

        if(kMeansParent == null) kMeansParent = GameObject.Find("KMeansAlgorithmController");
        if(dbScanParent == null) dbScanParent = GameObject.Find("DBScanAlgorithmController");
        if(denclueParent == null) denclueParent = GameObject.Find("DenclueAlgorithmController");
        if(silhouetteCoefficient == null) silhouetteCoefficient = GameObject.Find("SilhouetteCoefficient");
        if (responsiveMenu == null || datasetsParent == null || visualizationsParent == null) Invoke("SetParents", 1);
    }
    #region PunRPC methods
    [PunRPC]
    private void LoadDatasetController(int dataIndex)
    {
        datasetsParent.transform.GetChild(dataIndex).GetComponent<datasetChangerScript>().startTargetedAction();
    }
    [PunRPC]
    private void LoadVisualizationController(int vizIndex)
    {
        visualizationsParent.transform.GetChild(vizIndex).GetComponent<VisualizationChangerScript>().startSelectedAction();
    }
    #endregion

}
