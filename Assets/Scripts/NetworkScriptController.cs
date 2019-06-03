using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkScriptController : MonoBehaviour {
    /*
     * Script for parsing commands between players
     * for calling script methods
     */
    public object[] param;
    public static NetworkScriptController commandSender;
    public int index, algorithmStep;
    //Determines whether script is called from master, or is client
    public bool master = false;

    public GameObject responsiveMenu, visualizationsParent, kMeansController, dbscanController, denclueController, silhouetteCoefficient;
    public DataChangerScript dataChanger;
    public VizChangerScript vizChanger;
    // Use this for initialization
    void Start() {
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

}
