using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalButtonScript : MonoBehaviour {
    public GameObject primaryMenu,primaryParent, datasetParent, vizParent, algorithmParent, kmeansParent, dbscanParent;
    public Material onHoverMaterial, defaultMaterial;
    public ResponsiveMenuScript controller;
    public bool isHover, isPress, toChange;
    public float difference;
    private Transform responsiveMenu;
    private MeshRenderer meshRenderer;
    private IncreaseDecrease increaseDecreseObj;
    private PointerEventListener ptEvtLsnr;
    private BackButtonMenu menusParent;
    private SwapBetweenMenus swapScript;

    // Use this for initialization
	void Start () {
        FindParents();
        isHover = false;
        isPress = false;
        toChange = true;
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.material;
        onHoverMaterial = Resources.Load("Materials/OnHoverMaterial", typeof(Material)) as Material;
        controller = (ResponsiveMenuScript)FindObjectOfType(typeof(ResponsiveMenuScript));
        primaryMenu = GameObject.Find("MenusParent");
        ptEvtLsnr = (PointerEventListener)FindObjectOfType(typeof(PointerEventListener));
        menusParent = (BackButtonMenu)FindObjectOfType(typeof(BackButtonMenu));
        swapScript = (SwapBetweenMenus)FindObjectOfType(typeof(SwapBetweenMenus));
    }
	
	// Update is called once per frame
	void Update () {
        //if hover and not increasing or decreasing
        if(isHover && !controller.increaseDecrease)
        {
            //if it is supposed to be changed and button is not selected before
            if(toChange)
            {
                toChange = false;
                meshRenderer.material = onHoverMaterial;
                if(GetComponent<RotateDatasetButton>() != null)
                {
                    GetComponent<RotateDatasetButton>().isHovered = true;
                }
            }
        }
        else
        {
            if (meshRenderer.material != defaultMaterial)
            {
                meshRenderer.material = defaultMaterial;

                if (GetComponent<RotateDatasetButton>() != null)
                {
                    GetComponent<RotateDatasetButton>().isHovered = false;
                }
            }
            toChange = true;
        }
	}
    

    void FindParents()
    {
        responsiveMenu = GameObject.Find("ResponsiveMenu").transform;
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
        }
    }
    
    public void Press()
    {
        foreach (Transform child in gameObject.transform.parent)
        {
            child.gameObject.GetComponent<UniversalButtonScript>().isPress = false;
        }
        isPress = true;
        isHover = false;
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
            GetComponent<DatasetSelectedSpriteToggle>().ShowSprite();
        }
        //change to proper visualization
        else if (transform.parent == vizParent.transform)
        {
            GetComponent<VisualizationChangerScript>().startSelectedAction();
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
                increaseDecreseObj = GetComponent<IncreaseDecrease>();
                //controller.increaseDecrease = true;
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
                GetComponent<PreviousStep>().obj.GetComponent<KMeansAlgorithm>().PreviousStep();
            }
            else if(this.name == "K-Means Step Forward")
            {
                GetComponent<KMeansAlgorithm>().StartAlgorithm();
            }
            else if(this.name == "Play")
            {
                GetComponent<PlayScript>().buttonWasPressed = true;
            }
        }
        //DBSCAN buttons functionalities
        else if(transform.parent == dbscanParent.transform)
        {
            if (this.name == "epsilon")
            {
                increaseDecreseObj = GetComponent<IncreaseDecrease>();
                //controller.increaseDecrease = true;
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
                increaseDecreseObj = GetComponent<IncreaseDecrease>();
                controller.increaseDecrease = true;
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
                GetComponent<ToggleEucledianManhattan>().Toggle();
            }
            else if(this.name == "StepBackward")
            {
                GetComponent<DBPrevious>().dbSCAN.GetComponent<DBScanAlgorithm>().DBBackwards();
            }
            else if(this.name == "DBScan Step Forward")
            {
                GetComponent<DBScanAlgorithm>().StartDBSCAN();
            }
            else if(this.name == "Play")
            {
                GetComponent<DBScanPlay>().play = true;
            }
        }

        //Controls Menu functionalities
        else if(transform.parent.gameObject.name == "ControlsMenu")
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
            else if(this.name == "Select")
            {
                ptEvtLsnr.setSelectDataMode();
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

    public void CancelAllCalls()
    {
        CancelInvoke();
    }

    private void OnEnable()
    {
        if(meshRenderer!=null && meshRenderer.material != defaultMaterial)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

}
