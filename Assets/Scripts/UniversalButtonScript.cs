using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalButtonScript : MonoBehaviour
{
    public GameObject primaryMenu, primaryParent, datasetParent, vizParent, algorithmParent, kmeansParent, dbscanParent, denclueParent, ground, kMeansController, dbScanController, denclueController;
    public ResponsiveMenuScript controller;
    public float difference;
    public SilhouetteCoefficient coef;
    public int indexID;

    private Transform responsiveMenu;
    private MeshRenderer meshRenderer;
    private IncreaseDecrease increaseDecreseObj;
    private PointerEventListener ptEvtLsnr;
    private BackButtonMenu menusParent;
    private SwapBetweenMenus swapScript;
    private int index = 0;
    //for debugging
    public bool loadDataset, loadVis, startKmeans, startDBScan, startDenclue, silhouetteCoef;

    // Use this for initialization
    void Start()
    {
        FindParentsAndObjects();
        meshRenderer = GetComponent<MeshRenderer>();
        loadDataset = loadVis = startDenclue = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (loadDataset)
        {
            loadDataset = false;
            index = 0;
            foreach (Transform child in transform.parent)
            {
                if (child == transform) break;
                index++;
            }
            DataChangerScript.dataChanger.CmdLoadDataset(indexID);
        }

        if (loadVis)
        {
            loadVis = false;
            index = 0;
            foreach (Transform child in transform.parent)
            {
                if (child == transform) break;
                index++;
            }
            VizChangerScript.vizChanger.CmdChangeVisualization(indexID);
        }
        if (startKmeans)
        {
            startKmeans = false;
            kMeansController.GetComponent<KMeansAlgorithm>().CmdStartAlgorithm();

        }
        if (startDBScan)
        {
            startDBScan = false;
            dbScanController.GetComponent<DBScanAlgorithm>().CmdStartDBSCAN();
        }
        if (startDenclue)
        {
            startDenclue = false;
            denclueController.GetComponent<DenclueAlgorithm>().CmdStartDenclue();
        }
        if (silhouetteCoef)
        {
            silhouetteCoef = false;
            coef.Calculate();
        }
    }


    void FindParentsAndObjects()
    {
        if (transform.parent.gameObject.name == "ControlsMenu" || transform.parent.gameObject.name == "KMeansControlsMenu")
        {
            responsiveMenu = transform.parent.transform.parent.GetChild(1);
        }
        else
        {
            responsiveMenu = transform;
            while (responsiveMenu.name != "ResponsiveMenu")
            {
                responsiveMenu = responsiveMenu.parent;
            }
        }
        foreach (Transform child in responsiveMenu)
        {
            if (child.name == "PrimaryMenuParent")
            {
                primaryParent = child.gameObject;
            }
            else if (child.name == "DatasetParent")
            {
                datasetParent = child.gameObject;
            }
            else if (child.name == "VisualizationsParent")
            {
                vizParent = child.gameObject;
            }
            else if (child.name == "AlgorithmsParent")
            {
                algorithmParent = child.gameObject;
            }
            else if (child.name == "KMeansParent")
            {
                kmeansParent = child.gameObject;
            }
            else if (child.name == "DBSCANParent")
            {
                dbscanParent = child.gameObject;
            }
            else if (child.name == "DENCLUEParent")
            {
                denclueParent = child.gameObject;
            }
        }

        controller = (ResponsiveMenuScript)FindObjectOfType(typeof(ResponsiveMenuScript));
        primaryMenu = GameObject.Find("MenusParent");
        ptEvtLsnr = (PointerEventListener)FindObjectOfType(typeof(PointerEventListener));
        menusParent = (BackButtonMenu)FindObjectOfType(typeof(BackButtonMenu));
        swapScript = (SwapBetweenMenus)FindObjectOfType(typeof(SwapBetweenMenus));
        ground = GameObject.Find("Ground");
        coef = (SilhouetteCoefficient)FindObjectOfType(typeof(SilhouetteCoefficient));
        kMeansController = GameObject.Find("KMeansAlgorithmController(Clone)");
        dbScanController = GameObject.Find("DBScanAlgorithmController(Clone)");
        denclueController = GameObject.Find("DenclueAlgorithmController(Clone)");
    }
    
    public void Press()
    {
        RespectiveButtonRespectiveFunction();
    }

    void RespectiveButtonRespectiveFunction()
    {
        //show the datasets buttons
        if (this.name == "Datasets")
        {
            GetComponent<ShowOrHideDatasets>().ButtonPressed();
            if (menusParent.previousMenus[menusParent.previousMenus.Count - 1] != datasetParent)
            {
                menusParent.previousMenus.Add(datasetParent);
                swapScript.dontShowControlsMenu = true;
                menusParent.GetComponent<CoverflowScript>().AssignValues(datasetParent);
            }
        }
        //show the visualizations buttons
        else if (this.name == "Visualizations")
        {
            GetComponent<ShowOrHideVisualizations>().ButtonPressed();
            if (menusParent.previousMenus[menusParent.previousMenus.Count - 1] != vizParent)
            {
                menusParent.previousMenus.Add(vizParent);
                swapScript.dontShowControlsMenu = true;
                menusParent.GetComponent<CoverflowScript>().AssignValues(vizParent);
            }
        }
        //show the algorithms buttons
        else if (this.name == "Algorithms")
        {
            GetComponent<ShowOrHideAlgorithms>().ButtonPressed();
            if (menusParent.previousMenus[menusParent.previousMenus.Count - 1] != algorithmParent)
            {
                menusParent.previousMenus.Add(algorithmParent);
                swapScript.dontShowControlsMenu = true;
                menusParent.GetComponent<CoverflowScript>().AssignValues(algorithmParent);
            }
        }
        //toggle between showing or hiding the mirror
        else if (this.name == "ShowHideMirror")
        {
            GetComponent<ShowHideMirror>().ToggleMirror();
        }
        //reset the whole scene
        else if (this.name == "Reset")
        {
            SceneManager.LoadScene("MainScene");
        }
        //change to proper dataset
        else if (transform.parent == datasetParent.transform)
        {
            //GetComponent<datasetChangerScript>().startTargetedAction();
            index = 0;
            foreach (Transform child in transform.parent)
            {
                if (child == transform) break;
                index++;
            }
            DataChangerScript.dataChanger.CmdLoadDataset(indexID);
            menusParent.GetComponent<BackButtonMenu>().GoBackInMenu();
            GetComponent<DatasetSelectedSpriteToggle>().ShowSprite();
        }
        //change to proper visualization
        else if (transform.parent == vizParent.transform)
        {
            index = 0;
            foreach (Transform child in transform.parent)
            {
                if (child == transform) break;
                index++;
            }
            VizChangerScript.vizChanger.CmdChangeVisualization(indexID);

            menusParent.GetComponent<BackButtonMenu>().GoBackInMenu();
            GetComponent<VisualizationSelectedSpriteToggle>().ShowSprite();
        }
        //show the proper algorithm buttons
        else if (transform.parent == algorithmParent.transform)
        {
            GetComponent<ShowProperAlgorithmButtons>().ButtonPressed();
        }
        //K-means buttons functionalities
        else if (transform.parent == kmeansParent.transform)
        {
            if (kMeansController == null) kMeansController = GameObject.Find("KMeansAlgorithmController(Clone)");
            if (this.name == "NrOfSpheres")
            {
                //increaseDecreseObj = GetComponent<IncreaseDecrease>();
                increaseDecreseObj = kMeansController.GetComponent<IncreaseDecrease>();
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {
                    if (IsInvoking("DecreaseNrSpheres")) CancelInvoke("DecreaseNrSpheres");
                    if (!IsInvoking("IncreaseNrSpheres")) InvokeRepeating("IncreaseNrSpheres", 0, 0.5f);
                }
                else if (difference < 0f)
                {
                    if (IsInvoking("IncreaseNrSpheres")) CancelInvoke("IncreaseNrSpheres");
                    if (!IsInvoking("DecreaseNrSpheres")) InvokeRepeating("DecreaseNrSpheres", 0, 0.5f);
                }
            }
            else if (this.name == "Step Backward")
            {
                kMeansController.GetComponent<KMeansAlgorithm>().CmdPreviousStep();
            }
            else if (this.name == "K-Means Step Forward")
            {
                kMeansController.GetComponent<KMeansAlgorithm>().CmdStartAlgorithm();
            }
            else if (this.name == "Play")
            {
                kMeansController.GetComponent<PlayScript>().CmdTogglePlayPause();
            }
            else if (this.name == "SilhouetteCoef")
            {
                coef.Calculate();
            }
        }
        //DBSCAN buttons functionalities
        else if (transform.parent == dbscanParent.transform)
        {
            if (dbScanController == null) dbScanController = GameObject.Find("DBScanAlgorithmController(Clone)");
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbscanParent);
            if (this.name == "epsilon")
            {
                increaseDecreseObj = dbScanController.GetComponents<IncreaseDecrease>()[0];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {
                    if (IsInvoking("DecreaseEpsilon")) CancelInvoke("DecreaseEpsilon");
                    if (!IsInvoking("IncreaseEpsilon")) InvokeRepeating("IncreaseEpsilon", 0, 0.5f);
                }
                else if (difference < 0f)
                {
                    if (IsInvoking("IncreaseEpsilon")) CancelInvoke("IncreaseEpsilon");
                    if (!IsInvoking("DecreaseEpsilon")) InvokeRepeating("DecreaseEpsilon", 0, 0.5f);
                }
            }
            else if (this.name == "minPts")
            {
                increaseDecreseObj = dbScanController.GetComponents<IncreaseDecrease>()[1];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseMinPts")) CancelInvoke("DecreaseMinPts");
                    if (!IsInvoking("IncreaseMinPts")) InvokeRepeating("IncreaseMinPts", 0, 0.5f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseMinPts")) CancelInvoke("IncreaseMinPts");
                    if (!IsInvoking("DecreaseMinPts")) InvokeRepeating("DecreaseMinPts", 0, 0.5f);
                }
            }
            else if (this.name == "EucledianManhattan")
            {
                dbScanController.GetComponent<ToggleEucledianManhattan>().CmdToggle();
            }
            else if (this.name == "StepBackward")
            {
                dbScanController.GetComponent<DBScanAlgorithm>().CmdDBBackwards();
            }
            else if (this.name == "DBScan Step Forward")
            {
                dbScanController.GetComponent<DBScanAlgorithm>().CmdStartDBSCAN();
            }
            else if (this.name == "Play")
            {
                dbScanController.GetComponent<DBScanPlay>().CmdTogglePlayPause();
            }
            else if (this.name == "SilhouetteCoef")
            {
                coef.Calculate();
            }
        }

        //DENCLUE buttons functionalities
        else if (transform.parent == denclueParent.transform)
        {
            if (denclueController == null) denclueController = GameObject.Find("DenclueAlgorithmController(Clone)");
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            if (this.name == "influence")
            {
                increaseDecreseObj = denclueController.GetComponents<IncreaseDecrease>()[0];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseInfluence")) CancelInvoke("DecreaseInfluence");
                    if (!IsInvoking("IncreaseInfluence")) InvokeRepeating("IncreaseInfluence", 0, 0.5f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseInfluence")) CancelInvoke("IncreaseInfluence");
                    if (!IsInvoking("DecreaseInfluence")) InvokeRepeating("DecreaseInfluence", 0, 0.5f);
                }
            }
            else if (this.name == "threshold")
            {
                increaseDecreseObj = denclueController.GetComponents<IncreaseDecrease>()[1];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseThreshold")) CancelInvoke("DecreaseThreshold");
                    if (!IsInvoking("IncreaseThreshold")) InvokeRepeating("IncreaseThreshold", 0, 0.5f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseThreshold")) CancelInvoke("IncreaseThreshold");
                    if (!IsInvoking("DecreaseThreshold")) InvokeRepeating("DecreaseThreshold", 0, 0.5f);
                }
            }
            else if (this.name == "SquareGaussian")
            {
                denclueController.GetComponent<SquareGaussian>().CmdToggleSquareGaussian();
            }
            else if (this.name == "DencluePlay")
            {
                denclueController.GetComponent<DenclueAlgorithm>().CmdStartDenclue();
            }
            else if (this.name == "SingleMultiCentered")
            {
                denclueController.GetComponent<ToggleSingleMultiCentered>().CmdToggle();
            }
        }


        //Controls Menu functionalities
        else if (transform.parent.gameObject.name == "ControlsMenu" || transform.parent.gameObject.name == "KMeansControlsMenu")
        {
            if (this.name == "Move")
            {
                ptEvtLsnr.setMoveMode();
            }
            else if (this.name == "Scale")
            {
                ptEvtLsnr.setScalingMode();
            }
            else if (this.name == "Rotate")
            {
                ptEvtLsnr.setRotationMode();
            }
            else if (this.name == "GoToSphere")
            {
                ptEvtLsnr.setSelectDataMode();
            }
            else if (this.name == "GoToGround")
            {
                ground.GetComponent<SetToGround>().rigPosReset = true;
            }
        }

    }

    void IncreaseNrSpheres()
    {
        increaseDecreseObj.CmdIncreaseNrSpheres();
    }

    void DecreaseNrSpheres()
    {
        increaseDecreseObj.CmdDecreaseNrSpheres();
    }

    void IncreaseEpsilon()
    {
        increaseDecreseObj.CmdIncreaseEpsilon();
    }

    void DecreaseEpsilon()
    {
        increaseDecreseObj.CmdDecreaseEpsilon();
    }

    void IncreaseMinPts()
    {
        increaseDecreseObj.CmdIncreaseMinPts();
    }

    void DecreaseMinPts()
    {
        increaseDecreseObj.CmdDecreaseMinPts();
    }

    void IncreaseInfluence()
    {
        increaseDecreseObj.CmdIncreaseInfluence();
    }

    void DecreaseInfluence()
    {
        increaseDecreseObj.CmdDecreaseInfluence();
    }

    void IncreaseThreshold()
    {
        increaseDecreseObj.CmdIncreaseThreshold();
    }

    void DecreaseThreshold()
    {
        increaseDecreseObj.CmdDecreaseThreshold();
    }

    public void CancelAllCalls()
    {
        CancelInvoke();
    }
}