using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalButtonScript : MonoBehaviour {
    public GameObject primaryParent, datasetParent, vizParent, algorithmParent, kmeansParent, dbscanParent;
    private Transform responsiveMenu;
    public bool isHover, isPress, toChange;
    private MeshRenderer meshRenderer;
    public Material onHoverMaterial, defaultMaterial;

    // Use this for initialization
	void Start () {
        FindParents();
        isHover = false;
        isPress = false;
        toChange = true;
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.material;
        onHoverMaterial = Resources.Load("Materials/OnHoverMaterial", typeof(Material)) as Material;
    }
	
	// Update is called once per frame
	void Update () {
        if(isHover)
        {
            //if it is supposed to be changed and button is not selected before
            if(toChange && !GetComponent<Animator>().GetBool("selected"))
            {
                toChange = false;
                meshRenderer.material = onHoverMaterial;
            }
        }
        else
        {
            if (meshRenderer.material != defaultMaterial)
            {
                meshRenderer.material = defaultMaterial;
                toChange = true;
            }
        }
	}

    private void OnEnable()
    {
        if(isPress)
        {
            GetComponent<Animator>().SetBool("selected", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("selected", false);
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
            child.gameObject.GetComponent<Animator>().SetBool("selected", false);
            child.gameObject.GetComponent<UniversalButtonScript>().isPress = false;
        }
        GetComponent<Animator>().SetBool("selected", true);
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
        }
        //show the visualizations buttons
        else if (this.name == "Visualizations")
        {
            GetComponent<ShowOrHideVisualizations>().ButtonPressed();
        }
        //show the algorithms buttons
        else if (this.name == "Algorithms")
        {
            GetComponent<ShowOrHideAlgorithms>().ButtonPressed();
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
            GetComponent<ShowOrHidePrimaryMenu>().ButtonPressed();
        }
        //change to proper visualization
        else if (transform.parent == vizParent.transform)
        {
            GetComponent<VisualizationChangerScript>().startSelectedAction();
            GetComponent<ShowOrHidePrimaryMenu>().ButtonPressed();
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
            if(this.name == "NrOfSpheres")
            {
                //
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
            if(this.name == "epsilon")
            {
                //
            }
            else if(this.name == "minPts")
            {
                //
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
        
    }
    
}
