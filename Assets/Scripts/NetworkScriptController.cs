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
    public int index, algorithmStep;
    //Determines whether script is called from master, or is client
    public bool master = false;

    public GameObject responsiveMenu, visualizationsParent, kMeansController, dbscanController, denclueController, silhouetteCoefficient;
    public DataChangerScript dataChanger;
    public VizChangerScript vizChanger;
    // Use this for initialization
    void Start() {
        photonView = GetComponent<PhotonView>();
        SetParents();
    }

    // Update is called once per frame
    void Update() {

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
        photonView.RPC("LoadDatasetController", PhotonTargets.AllViaServer, index);
    }
    public void LoadVisualization()
    {
        photonView.RPC("LoadVisualizationController", PhotonTargets.AllViaServer, index);
    }

    public void KMeansAlgorithm()
    {
        photonView.RPC("RunKMeans", PhotonTargets.AllViaServer);
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
                if (child.name == "VisualizationsParent")
                {
                    visualizationsParent = child.gameObject;
                }
            }
        }

        if (kMeansController == null) kMeansController = GameObject.Find("KMeansAlgorithmController");
        if (dbscanController == null) dbscanController = GameObject.Find("DBScanAlgorithmController");
        if (denclueController == null) denclueController = GameObject.Find("DenclueAlgorithmController");
        if (silhouetteCoefficient == null) silhouetteCoefficient = GameObject.Find("SilhouetteCoefficient");
        if (dataChanger == null) dataChanger = (DataChangerScript)FindObjectOfType(typeof(DataChangerScript));
        if (vizChanger == null) vizChanger = (VizChangerScript)FindObjectOfType(typeof(VizChangerScript));
        if (responsiveMenu == null || visualizationsParent == null) Invoke("SetParents", 1);
    }
    #region PunRPC methods
    [PunRPC]
    private void LoadDatasetController(int dataIndex)
    {
        dataChanger.LoadDataset(dataIndex);
    }
    [PunRPC]
    private void LoadVisualizationController(int vizIndex)
    {
        vizChanger.ChangeVisualization(vizIndex);
    }
    [PunRPC]
    private void RunKMeans()
    {
        /*
         * Calls KMeansAlgorithm on all clients including this one
         * As KMeans works, first step is random generation of spheres,
         * therefore, in 1 step, spheres are randomly generated on this client, and position is updated to other clients
         * Next steps is normal calling of KMeansAlgorithm script
         */
        if(algorithmStep == 0)
        {
            //execute this code, if this is the main script called
            if (master)
            {
                kMeansController.GetComponent<KMeansAlgorithm>().StartAlgorithm();
                foreach(var player in PhotonNetwork.playerList)
                {
                    if (player.ID == PhotonNetwork.player.ID)
                    {
                        continue;
                    }
                    for(int i=0; i<kMeansController.GetComponent<KMeansAlgorithm>().spheresStartPositions.Count; i++)
                    {
                        //player.networkController().kMeansController.GetComponent<KMeansAlgorithm>().spheresStartPositions[i] = kMeansController.GetComponent<KMeansAlgorithm>().spheresStartPositions[i];
                    }
                }
            }
            //execute if this script is client
            else
            {
                kMeansController.GetComponent<KMeansAlgorithm>().StartAlgorithm();
            }
            algorithmStep++;
            master = false;
        }
        else kMeansController.GetComponent<KMeansAlgorithm>().StartAlgorithm();
    }
    #endregion

}
