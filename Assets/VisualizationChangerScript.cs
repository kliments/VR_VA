using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationChangerScript : MonoBehaviour {

    private TextAsset myDataset;
    public GameObject[] typeOfVisualization;
    private float lastActivation = 0;
    private GameObject myText;
    public Transform ChooserElementsContainer;
    public GameObject Scatterplot;
    private GameObject cubes;
    private GameObject pies;
    private GameObject triangles;
    private GameObject tetrahedrons;
    public GameObject parent;
    public GameObject ground;

    private GameObject[] visualizationss;
    private int cube = 0;
    private int pie = 0;
    private int triangle = 0;
    private int tetrahedron = 0;
    public bool isSelected;


    private int trnglCounter, ttrhdrnCounter = 0;
    public GameObject resetKmeans;
    public GameObject resetDBScan;

    private int vizLength;
    public void startSelectedAction()
    {
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();

        resetKmeans.GetComponent<KMeansAlgorithm>().ResetMe();
        resetDBScan.GetComponent<DBScanAlgorithm>().ResetMe();
        
        vizLength = typeOfVisualization.Length;
        cubes = FindObject(Scatterplot, "DataSpace");
        pies = FindObject(Scatterplot, "PieChartCtrl");
        triangles = FindObject(Scatterplot, "Triangle");
        tetrahedrons = FindObject(Scatterplot, "Tetrahedron");

        myDataset = data(ChooserElementsContainer);

        if (Time.time - lastActivation > 2)
        {
            lastActivation = Time.time;
            foreach (VisualizationChangerScript v in FindObjectsOfType<VisualizationChangerScript>())
            {
                v.GetComponent<Animator>().SetBool("selected", false);
                v.GetComponent<VisualizationChangerScript>().isSelected = false;
            }
            this.GetComponent<VisualizationChangerScript>().isSelected = true;
            this.GetComponent<Animator>().SetBool("selected", true);
            if (this.name == "DataSpaceButton")
            {
                if (pies.activeSelf)
                {
                    PieChartMeshController dummy = pies.GetComponent<PieChartMeshController>();
                    dummy.resetMe();
                    pies.SetActive(false);
                }
                else if (triangles.activeSelf)
                {
                    Triangle dummy = triangles.GetComponent<Triangle>();
                    dummy.resetMe();
                    triangles.SetActive(false);
                }
                else if (tetrahedrons.activeSelf)
                {
                    Tetrahedron dummy = tetrahedrons.GetComponent<Tetrahedron>();
                    dummy.resetMe();
                    tetrahedrons.SetActive(false);
                }
                cubes.SetActive(true);
                cubes.GetComponent<DataSpaceHandler>().changeDatafile(myDataset);
                this.GetComponent<Animator>().SetBool("selected", true);

            }
            else if (this.name == "PieChartCtrlButton")
            {
                if (cubes.activeSelf)
                {
                    DataSpaceHandler dummy = cubes.GetComponent<DataSpaceHandler>();
                    dummy.resetMe();

                    cubes.SetActive(false);
                }
                else if (triangles.activeSelf)
                {
                    Triangle dummy = triangles.GetComponent<Triangle>();
                    dummy.resetMe();
                    triangles.SetActive(false);
                }
                else if (tetrahedrons.activeSelf)
                {
                    Tetrahedron dummy = tetrahedrons.GetComponent<Tetrahedron>();
                    dummy.resetMe();
                    tetrahedrons.SetActive(false);
                }

                pies.SetActive(true);
                pies.GetComponent<PieChartMeshController>().changeDatafile(myDataset);
                this.GetComponent<Animator>().SetBool("selected", true);

            }
            else if (this.name == "TriangleButton")
            {
                if (cubes.activeSelf)
                {
                    DataSpaceHandler dummy = cubes.GetComponent<DataSpaceHandler>();
                    dummy.resetMe();

                    cubes.SetActive(false);
                }
                else if (pies.activeSelf)
                {
                    PieChartMeshController dummy = pies.GetComponent<PieChartMeshController>();
                    dummy.resetMe();
                    pies.SetActive(false);
                }
                else if (tetrahedrons.activeSelf)
                {
                    Tetrahedron dummy = tetrahedrons.GetComponent<Tetrahedron>();
                    dummy.resetMe();
                    tetrahedrons.SetActive(false);
                }

                //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
                if(trnglCounter == 0)
                {
                    trnglCounter++;
                    triangles.GetComponent<Triangle>().data = data(ChooserElementsContainer);
                    triangles.SetActive(true);
                }
                else
                {
                    triangles.SetActive(true);
                    triangles.GetComponent<Triangle>().changeDatafile(myDataset);
                }
                this.GetComponent<Animator>().SetBool("selected", true);
            }
            else if (this.name == "TetrahedronButton")
            {
                if (cubes.activeSelf)
                {
                    DataSpaceHandler dummy = cubes.GetComponent<DataSpaceHandler>();
                    dummy.resetMe();

                    cubes.SetActive(false);
                }
                else if(pies.activeSelf)
                {
                    PieChartMeshController dummy = pies.GetComponent<PieChartMeshController>();
                    dummy.resetMe();
                    pies.SetActive(false);
                }
                else if (triangles.activeSelf)
                {
                    Triangle dummy = triangles.GetComponent<Triangle>();
                    dummy.resetMe();
                    triangles.SetActive(false);
                }

                //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
                if(ttrhdrnCounter == 0)
                {
                    ttrhdrnCounter++;
                    tetrahedrons.GetComponent<Tetrahedron>().data = data(ChooserElementsContainer);
                    tetrahedrons.SetActive(true);
                }
                else
                {
                    tetrahedrons.SetActive(true);
                    tetrahedrons.GetComponent<Tetrahedron>().changeDatafile(myDataset);
                }
                this.GetComponent<Animator>().SetBool("selected", true);
            }
            lastActivation = Time.time;
        }
    }

        // Use this for initialization
        void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {

    }

    private void OnEnable()
    {
        if (isSelected)
        {
            GetComponent<Animator>().SetBool("selected", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("selected", false);
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

    public static TextAsset data(Transform parent)
    {
        TextAsset myData;
        foreach(Transform t in parent)
        {
            if (t.gameObject.GetComponent<Animator>().GetBool("selected") || t.gameObject.GetComponent<datasetChangerScript>().isSelected == true)
                {
                myData = t.gameObject.GetComponent<datasetChangerScript>().myDataset;
                return myData;
            }
        }
        return null;
    }

    private void ResetSpheres()
    {

    }
}
