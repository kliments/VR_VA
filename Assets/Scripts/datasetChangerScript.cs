using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class datasetChangerScript : MonoBehaviour
{

    public TextAsset myDataset;
    public GameObject textPrefab;
    public GameObject scatterplot;
    public GameObject spriteGraph;
    public GameObject buttonText;

    private float lastActivation = 0;
    private GameObject myText;
    private GameObject mySprite;

    public GameObject parentViz;
    public Transform responsiveMenu;
    public GameObject cubes;
    public GameObject pies;
    public GameObject triangles;
    public GameObject tetrahedrons;
    public bool isSelected;
    public bool isHovered = false;

    public GameObject resetKmeans;
    public GameObject resetDBScan;
    public GameObject ground;

    public void startTargetedAction()
    {
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();
        //Reset the K-means algorithm in case the dataset is changed
        resetKmeans.GetComponent<KMeansAlgorithm>().ResetMe();

        //Reset the DBScan algorithm in case the dataset is changed
        resetDBScan.GetComponent<DBScanAlgorithm>().ResetMe();

        scatterplot = GameObject.Find("ScatterplotElements");
        cubes = FindObject(scatterplot, "DataSpace");
        pies = FindObject(scatterplot, "PieChartCtrl");
        triangles = FindObject(scatterplot, "Triangle");
        tetrahedrons = FindObject(scatterplot, "Tetrahedron");

        if (Time.time - lastActivation > 2)
        {
            lastActivation = Time.time;
            foreach(datasetChangerScript d in FindObjectsOfType<datasetChangerScript>())
            {
                d.GetComponent<datasetChangerScript>().isSelected = false;
            }
            isSelected = true;
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
    void Start()
    {
        ground = GameObject.Find("Ground");
        responsiveMenu = GameObject.Find("ResponsiveMenu").transform;
        foreach(Transform child in responsiveMenu)
        {
            if(child.name == "KMeansParent")
            {
                foreach(Transform childOfChild in child)
                {
                    if(childOfChild.name == "K-Means Step Forward")
                    {
                        resetKmeans = childOfChild.gameObject;
                    }
                }
            }
            else if(child.name == "DBSCANParent")
            {
                foreach (Transform childOfChild in child)
                {
                    if (childOfChild.name == "DBScan Step Forward")
                    {
                        resetDBScan = childOfChild.gameObject;
                    }
                }
            }
        }
    }

    public void initText() { 
        myText = Instantiate(textPrefab);
        myText.transform.parent = this.transform;
        myText.transform.localPosition = new Vector3(0, 0, 0);
        /*myText.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        myText.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);*/

        String value = myDataset.name;
        //remove the first 4 characters, 'PCA_'
        value = value.Remove(0, 4);
        
        Text mesher = myText.transform.GetChild(0).gameObject.GetComponent<Text>();
        mesher.fontSize = 100;
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

    public void ChangeText(string text)
    {
        text = text.Remove(0, 4);
        buttonText.transform.GetChild(0).GetComponent<Text>().text = text;
    }
}
