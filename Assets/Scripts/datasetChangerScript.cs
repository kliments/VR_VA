using UnityEngine;
using System.Collections;
using System;

public class datasetChangerScript : MonoBehaviour
{

    public TextAsset myDataset;
    public GameObject textPrefab;
    public GameObject scatterplot;
    public GameObject spriteGraph;

    private float lastActivation = 0;
    private GameObject myText;
    private GameObject mySprite;

    private GameObject parentViz;
    public GameObject cubes;
    public GameObject pies;
    public GameObject triangles;
    public GameObject tetrahedrons;
    public bool isSelected;
    public bool isHovered = false;

    public Transform resetKmeans;

    private Animator anim;

    public void startTargetedAction()
    {
        //Next Step button for K-means algorithm containing reset script
        foreach (Transform child in resetKmeans)
        {
            if(child.gameObject.name == "NextStep")
            {
                child.gameObject.GetComponent<KMeansAlgorithm>().resetMe();
            }
        }

        parentViz = GameObject.Find("parentVisualizations");
        foreach(Transform child in parentViz.transform)
        {
            if(child.gameObject.name == "DataSpaceButton")
            {
                scatterplot = child.gameObject.GetComponent<VisualizationChangerScript>().Scatterplot;
            }
        }
        cubes = FindObject(scatterplot, "DataSpace");
        pies = FindObject(scatterplot, "PieChartCtrl");
        triangles = FindObject(scatterplot, "Triangle");
        tetrahedrons = FindObject(scatterplot, "Tetrahedron");

        if (Time.time - lastActivation > 2)
        {
            lastActivation = Time.time;
            foreach(datasetChangerScript d in FindObjectsOfType<datasetChangerScript>())
            {
                d.GetComponent<Animator>().SetBool("selected", false);
                d.GetComponent<datasetChangerScript>().isSelected = false;
            }
            this.GetComponent <datasetChangerScript>().isSelected = true;
            this.GetComponent<Animator>().SetBool("selected", true);
            anim = GetComponent<Animator>();
            //FindObjectOfType<axisMenueScript>().resetMenue();
            //FindObjectOfType<pcLoaderScript>().resetMe();
            if (cubes.activeSelf)
            {
                FindObjectOfType<DataSpaceHandler>().changeDatafile(myDataset);
            }
            else if (pies.activeSelf)
            {
                FindObjectOfType<PieChartMeshController>().changeDatafile(myDataset);
            }
            else if (triangles.activeSelf)
            {
                FindObjectOfType<Triangle>().changeDatafile(myDataset);
            }
            else if (tetrahedrons.activeSelf)
            {
                FindObjectOfType<Tetrahedron>().changeDatafile(myDataset);
            }
            lastActivation = Time.time;
        }
    }

   
    // Use this for initialization
    void Start() {
        resetKmeans = GameObject.Find("parentKmeans").transform;
    }

    public void initText() { 
        myText = Instantiate(textPrefab);
        myText.transform.parent = this.transform;
        myText.transform.localPosition = new Vector3(-0.461f, 0.31f, -0.6f);
        myText.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        myText.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);

        String value = myDataset.name;
        //remove the first 4 characters, 'PCA_'
        value = value.Remove(0, 4);

        TextMesh mesher = myText.GetComponent<TextMesh>();
        mesher.text = value;

    }

    public void spriteChanger(int i)
    {
        mySprite = Instantiate(spriteGraph);
        mySprite.transform.parent = this.transform;

        mySprite.transform.localPosition = new Vector3(-0.006f, -0.095f, -0.6f);
        mySprite.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        mySprite.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);

        mySprite.GetComponent<SpriteRenderer>().sprite = Resources.Load(i.ToString(), typeof(Sprite)) as Sprite;
        mySprite.SetActive(false);

    }
	
	// Update is called once per frame
	void Update() {
	    if(isSelected)
        {
            GetComponent<Animator>().SetBool("selected", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("selected", false);
        }

        if(isHovered)
        {
            mySprite.SetActive(true);
            isHovered = false;
        }
        else
        {
            mySprite.SetActive(false);
        }
	}

    public static GameObject FindObject(GameObject parent, string name)
    {
        Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }
}
