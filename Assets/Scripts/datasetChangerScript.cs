using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class datasetChangerScript : MonoBehaviour
{

    public TextAsset myDataset;
    public GameObject textPrefab;
    public GameObject scatterplot;

    private float lastActivation = 0;
    private GameObject myText;
    
    public Transform responsiveMenu;
    public GameObject cubes;
    public GameObject pies;
    public GameObject triangles;
    public GameObject tetrahedrons;
    public bool isSelected;
    public bool isHovered = false;

    public KMeansAlgorithm resetKmeans;
    public DBScanAlgorithm resetDBScan;
    public DenclueAlgorithm resetDenclue;
    public GameObject ground;
    public Sprite datasetSelected;

    private DataSpaceHandler cubesData;
    private PieChartMeshController piesData;
    private Triangle trianglesData;
    private Tetrahedron tetrahedronsData;
    public void startTargetedAction()
    {
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();
        //Reset the K-means algorithm in case the dataset is changed
        resetKmeans.ResetMe();

        //Reset the DBScan algorithm in case the dataset is changed
        resetDBScan.ResetMe();

        resetDenclue.ResetMe();

        if(scatterplot == null) scatterplot = GameObject.Find("ScatterplotElements");
        if(cubes == null) cubes = FindObject(scatterplot, "DataSpace");
        if(pies == null) pies = FindObject(scatterplot, "PieChartCtrl");
        if(triangles == null) triangles = FindObject(scatterplot, "Triangle");
        if(tetrahedrons == null)tetrahedrons = FindObject(scatterplot, "Tetrahedron");
        if (cubesData == null) cubesData = FindObjectOfType<DataSpaceHandler>();
        if (piesData == null) piesData = FindObjectOfType<PieChartMeshController>();
        if (trianglesData == null) trianglesData = FindObjectOfType<Triangle>();
        if (tetrahedronsData == null) tetrahedronsData = FindObjectOfType<Tetrahedron>();
        foreach(Transform obj in transform.parent)
        {
            if(obj.GetComponent<datasetChangerScript>() != null) obj.GetComponent<datasetChangerScript>().isSelected = false;
        }
        isSelected = true;
        if (cubes.activeSelf)
        {
            cubesData.changeDatafile(myDataset);
        }
        else if (pies.activeSelf)
        {
            piesData.changeDatafile(myDataset);
        }
        else if (triangles.activeSelf)
        {
            trianglesData.changeDatafile(myDataset);
        }
        else if (tetrahedrons.activeSelf)
        {
            tetrahedronsData.changeDatafile(myDataset);
        }
    }

   
    // Use this for initialization
    void Start()
    {
        ground = GameObject.Find("Ground");
        responsiveMenu = transform;
        while(responsiveMenu.name != "ResponsiveMenu")
        {
            responsiveMenu = responsiveMenu.parent;
        }
        resetKmeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        resetDBScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        resetDenclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
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
}
