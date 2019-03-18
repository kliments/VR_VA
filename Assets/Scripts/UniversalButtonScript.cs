using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalButtonScript : MonoBehaviour {
    public GameObject primaryMenu,primaryParent, datasetParent, vizParent, algorithmParent, kmeansParent, dbscanParent, denclueParent, ground, kMeansController,dbScanController, denclueController;
    public ResponsiveMenuScript controller;
    public float difference;
    private Transform responsiveMenu;
    private MeshRenderer meshRenderer;
    private IncreaseDecrease increaseDecreseObj;
    private PointerEventListener ptEvtLsnr;
    private BackButtonMenu menusParent;
    private SwapBetweenMenus swapScript;
    public DenclueAlgorithm denclue;
    public SilhouetteCoefficient coef;

    //for debugging
    public bool loadDataset, loadVis, startDenclue;

    // Use this for initialization
	void Start () {
        FindParents();
        meshRenderer = GetComponent<MeshRenderer>();
        controller = (ResponsiveMenuScript)FindObjectOfType(typeof(ResponsiveMenuScript));
        primaryMenu = GameObject.Find("MenusParent");
        ptEvtLsnr = (PointerEventListener)FindObjectOfType(typeof(PointerEventListener));
        menusParent = (BackButtonMenu)FindObjectOfType(typeof(BackButtonMenu));
        swapScript = (SwapBetweenMenus)FindObjectOfType(typeof(SwapBetweenMenus));
        ground = GameObject.Find("Ground");
        denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        loadDataset = loadVis = startDenclue = false;
        coef = (SilhouetteCoefficient)FindObjectOfType(typeof(SilhouetteCoefficient));

        kMeansController = GameObject.Find("KMeansAlgorithmController");
        dbScanController = GameObject.Find("DBScanAlgorithmController");
        denclueController = GameObject.Find("DenclueAlgorithmController");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (loadDataset)
        {
            loadDataset = false;
            if (transform.parent == datasetParent.transform && GetComponent<datasetChangerScript>().isSelected)
            {
                GetComponent<datasetChangerScript>().startTargetedAction();
            }
        }

        if (loadVis)
        {
            loadVis = false;
            if (transform.parent == vizParent.transform && GetComponent<VisualizationChangerScript>().isSelected)
            {
                GetComponent<VisualizationChangerScript>().startSelectedAction();
            }
        }

        if (startDenclue)
        {
            startDenclue = false;
            denclue.StartDenclue();
        }
    }
    

    void FindParents()
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
        foreach(Transform child in responsiveMenu)
        {
            if(child.name == "PrimaryMenuParent")
            {
                primaryParent = child.gameObject;
            }
            else if(child.name == "DatasetParent")
            {
                datasetParent = child.gameObject;
            }
            else if(child.name == "VisualizationsParent")
            {
                vizParent = child.gameObject;
            }
            else if(child.name == "AlgorithmsParent")
            {
                algorithmParent = child.gameObject;
            }
            else if(child.name == "KMeansParent")
            {
                kmeansParent = child.gameObject;
            }
            else if(child.name == "DBSCANParent")
            {
                dbscanParent = child.gameObject;
            }
            else if (child.name == "DENCLUEParent")
            {
                denclueParent = child.gameObject;
            }
        }
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
            if(menusParent.previousMenus[menusParent.previousMenus.Count-1] != datasetParent)
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
            GetComponent<datasetChangerScript>().startTargetedAction();
            menusParent.GetComponent<BackButtonMenu>().GoBackInMenu();
            GetComponent<DatasetSelectedSpriteToggle>().ShowSprite();
        }
        //change to proper visualization
        else if (transform.parent == vizParent.transform)
        {
            GetComponent<VisualizationChangerScript>().startSelectedAction();
            menusParent.GetComponent<BackButtonMenu>().GoBackInMenu();
            GetComponent<VisualizationSelectedSpriteToggle>().ShowSprite();
        }
        //show the proper algorithm buttons
        else if(transform.parent == algorithmParent.transform)
        {
            GetComponent<ShowProperAlgorithmButtons>().ButtonPressed();
        }
        //K-means buttons functionalities
        else if(transform.parent == kmeansParent.transform)
        {
            //increase or decrease number of spheres
            if (this.name == "NrOfSpheres")
            {
                //increaseDecreseObj = GetComponent<IncreaseDecrease>();
                increaseDecreseObj = kMeansController.GetComponent<IncreaseDecrease>();
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {
                    if (IsInvoking("DecreaseNrSpheres")) CancelInvoke("DecreaseNrSpheres");
                    if (!IsInvoking("IncreaseNrSpheres")) InvokeRepeating("IncreaseNrSpheres", 0, 0.2f);
                }
                else if (difference < 0f)
                {
                    if (IsInvoking("IncreaseNrSpheres")) CancelInvoke("IncreaseNrSpheres");
                    if (!IsInvoking("DecreaseNrSpheres")) InvokeRepeating("DecreaseNrSpheres", 0, 0.2f);
                }
            }
            else if(this.name == "Step Backward")
            {
                kMeansController.GetComponent<KMeansAlgorithm>().PreviousStep();
            }
            else if(this.name == "K-Means Step Forward")
            {
                kMeansController.GetComponent<KMeansAlgorithm>().StartAlgorithm();
            }
            else if(this.name == "Play")
            {
                kMeansController.GetComponent<PlayScript>().TogglePlayPause();
            }
            else if (this.name == "SilhouetteCoef")
            {
                coef.Calculate();
            }
        }
        //DBSCAN buttons functionalities
        else if(transform.parent == dbscanParent.transform)
        {
            menusParent.GetComponent<CoverflowScript>().AssignValues(dbscanParent);
            if (this.name == "epsilon")
            {
                increaseDecreseObj = dbScanController.GetComponents<IncreaseDecrease>()[0];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {
                    if (IsInvoking("DecreaseEpsilon")) CancelInvoke("DecreaseEpsilon");
                    if (!IsInvoking("IncreaseEpsilon")) InvokeRepeating("IncreaseEpsilon", 0, 0.2f);
                }
                else if (difference < 0f)
                {
                    if (IsInvoking("IncreaseEpsilon")) CancelInvoke("IncreaseEpsilon");
                    if (!IsInvoking("DecreaseEpsilon")) InvokeRepeating("DecreaseEpsilon", 0, 0.2f);
                }
            }
            else if(this.name == "minPts")
            {
                increaseDecreseObj = dbScanController.GetComponents<IncreaseDecrease>()[1];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseMinPts")) CancelInvoke("DecreaseMinPts");
                    if (!IsInvoking("IncreaseMinPts")) InvokeRepeating("IncreaseMinPts", 0, 0.2f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseMinPts")) CancelInvoke("IncreaseMinPts");
                    if (!IsInvoking("DecreaseMinPts")) InvokeRepeating("DecreaseMinPts", 0, 0.2f);
                }
            }
            else if(this.name == "EucledianManhattan")
            {
                dbScanController.GetComponent<ToggleEucledianManhattan>().Toggle();
            }
            else if(this.name == "StepBackward")
            {
                dbScanController.GetComponent<DBScanAlgorithm>().DBBackwards();
            }
            else if(this.name == "DBScan Step Forward")
            {
                dbScanController.GetComponent<DBScanAlgorithm>().StartDBSCAN();
            }
            else if(this.name == "Play")
            {
                dbScanController.GetComponent<DBScanPlay>().TogglePlayPause();
            }
            else if (this.name == "SilhouetteCoef")
            {
                coef.Calculate();
            }
        }

        //DENCLUE buttons functionalities
        else if(transform.parent == denclueParent.transform)
        {
            menusParent.GetComponent<CoverflowScript>().AssignValues(denclueParent);
            if(this.name == "influence")
            {
                increaseDecreseObj = denclueController.GetComponents<IncreaseDecrease>()[0];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseInfluence")) CancelInvoke("DecreaseInfluence");
                    if (!IsInvoking("IncreaseInfluence")) InvokeRepeating("IncreaseInfluence", 0, 0.2f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseInfluence")) CancelInvoke("IncreaseInfluence");
                    if (!IsInvoking("DecreaseInfluence")) InvokeRepeating("DecreaseInfluence", 0, 0.2f);
                }
            }
            else if(this.name == "threshold")
            {
                increaseDecreseObj = denclueController.GetComponents<IncreaseDecrease>()[1];
                Vector2 pos = controller.device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                if (difference > 0f)
                {

                    if (IsInvoking("DecreaseThreshold")) CancelInvoke("DecreaseThreshold");
                    if (!IsInvoking("IncreaseThreshold")) InvokeRepeating("IncreaseThreshold", 0, 0.2f);
                }
                else if (difference < 0f)
                {

                    if (IsInvoking("IncreaseThreshold")) CancelInvoke("IncreaseThreshold");
                    if (!IsInvoking("DecreaseThreshold")) InvokeRepeating("DecreaseThreshold", 0, 0.2f);
                }
            }
            else if(this.name == "SquareGaussian")
            {
                denclueController.GetComponent<SquareGaussian>().ToggleSquareGaussian();
            }
            else if(this.name == "DencluePlay")
            {
                denclueController.GetComponent<DenclueAlgorithm>().StartDenclue();
            }
            else if (this.name == "SingleMultiCentered")
            {
                denclueController.GetComponent<ToggleSingleMultiCentered>().Toggle();
            }
        }
        

        //Controls Menu functionalities
        else if (transform.parent.gameObject.name == "ControlsMenu" || transform.parent.gameObject.name == "KMeansControlsMenu")
        {
            if(this.name == "Move")
            {
                ptEvtLsnr.setMoveMode();
            }
            else if(this.name == "Scale")
            {
                ptEvtLsnr.setScalingMode();
            }
            else if(this.name == "Rotate")
            {
                ptEvtLsnr.setRotationMode();
            }
            else if (this.name == "GoToSphere")
            {
                ptEvtLsnr.setSelectDataMode();
            }
            else if(this.name == "GoToGround")
            {
                ground.GetComponent<SetToGround>().rigPosReset = true;
            }
        }
        
    }

    void IncreaseNrSpheres()
    {
        increaseDecreseObj.IncreaseNrSpheres();
    }

    void DecreaseNrSpheres()
    {
        increaseDecreseObj.DecreaseNrSpheres();
    }

    void IncreaseEpsilon()
    {
        increaseDecreseObj.IncreaseEpsilon();
    }

    void DecreaseEpsilon()
    {
        increaseDecreseObj.DecreaseEpsilon();
    }

    void IncreaseMinPts()
    {
        increaseDecreseObj.IncreaseMinPts();
    }

    void DecreaseMinPts()
    {
        increaseDecreseObj.DecreaseMinPts();
    }

    void IncreaseInfluence()
    {
        increaseDecreseObj.IncreaseInfluence();
    }

    void DecreaseInfluence()
    {
        increaseDecreseObj.DecreaseInfluence();
    }

    void IncreaseThreshold()
    {
        increaseDecreseObj.IncreaseThreshold();
    }

    void DecreaseThreshold()
    {
        increaseDecreseObj.DecreaseThreshold();
    }

    public void CancelAllCalls()
    {
        CancelInvoke();
    }

}
