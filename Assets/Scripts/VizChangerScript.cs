using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VizChangerScript : NetworkBehaviour
{
    public int currentVizIndex;
    public static VizChangerScript vizChanger;
    public GameObject scatterplot;
    private DataChangerScript dataChanger;
    private TextAsset currentDataset;
    private GameObject cubes, pies, triangles, tetrahedrons;
    private SetToGround ground;
    private KMeansAlgorithm resetKmeans;
    private DBScanAlgorithm resetDBScan;
    private DenclueAlgorithm resetDenclue;
    private int cubesCounter, piesCounter, trnglCounter, ttrhdrnCounter = 0;


    [SyncVar]
    private int vizIndex;
    // Use this for initialization
    void Start () {
        dataChanger = (DataChangerScript)FindObjectOfType(typeof(DataChangerScript));
        ground = (SetToGround)FindObjectOfType(typeof(SetToGround));
        resetKmeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        resetDBScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        resetDenclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.P))
        {
            CmdChangeVisualization(vizIndex);
        }
	}

    [Command]
    public void CmdChangeVisualization(int index)
    {
        Debug.Log("Visualization changed on server!");
        currentVizIndex = index;
        ground.rigPosReset = true;
        ground.RemoveParenthoodFromRig();

        resetKmeans.ResetMe();
        resetDBScan.ResetMe();
        resetDenclue.ResetMe();
        
        if (cubes == null) cubes = FindObject(scatterplot, "DataSpace");
        if (pies == null) pies = FindObject(scatterplot, "PieChartCtrl");
        if (triangles == null) triangles = FindObject(scatterplot, "Triangle");
        if (tetrahedrons == null) tetrahedrons = FindObject(scatterplot, "Tetrahedron");
		
		if(dataChanger == null) dataChanger = (DataChangerScript)FindObjectOfType(typeof(DataChangerScript));
        currentDataset = dataChanger.datasets[dataChanger.currentDataIndex];
        
        //load cubes
        if (currentVizIndex == 0)
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

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (cubesCounter == 0)
            {
                cubesCounter++;
                cubes.GetComponent<DataSpaceHandler>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                cubes.SetActive(true);
            }
            else
            {
                cubes.SetActive(true);
                cubes.GetComponent<DataSpaceHandler>().changeDatafile(currentDataset);
            }

        }
        else if (currentVizIndex == 1)
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

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (piesCounter == 0)
            {
                piesCounter++;
                pies.GetComponent<PieChartMeshController>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                pies.SetActive(true);
            }
            else
            {
                pies.SetActive(true);
                pies.GetComponent<PieChartMeshController>().changeDatafile(currentDataset);
            }
        }
        else if (currentVizIndex == 2)
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
            if (trnglCounter == 0)
            {
                trnglCounter++;
                triangles.GetComponent<Triangle>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                triangles.SetActive(true);
            }
            else
            {
                triangles.SetActive(true);
                triangles.GetComponent<Triangle>().changeDatafile(currentDataset);
            }
        }
        else if (currentVizIndex == 3)
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
            else if (triangles.activeSelf)
            {
                Triangle dummy = triangles.GetComponent<Triangle>();
                dummy.resetMe();
                triangles.SetActive(false);
            }

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (ttrhdrnCounter == 0)
            {
                ttrhdrnCounter++;
                tetrahedrons.GetComponent<Tetrahedron>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                tetrahedrons.SetActive(true);
            }
            else
            {
                tetrahedrons.SetActive(true);
                tetrahedrons.GetComponent<Tetrahedron>().changeDatafile(currentDataset);
            }
        }
        RpcChangeVisualization(index);
    }

    [ClientRpc]
    public void RpcChangeVisualization(int index)
    {
        if (isServer || !hasAuthority) return;
        currentVizIndex = index;
        ground.rigPosReset = true;
        ground.RemoveParenthoodFromRig();

        resetKmeans.ResetMe();
        resetDBScan.ResetMe();
        resetDenclue.ResetMe();

        if (cubes == null) cubes = FindObject(scatterplot, "DataSpace");
        if (pies == null) pies = FindObject(scatterplot, "PieChartCtrl");
        if (triangles == null) triangles = FindObject(scatterplot, "Triangle");
        if (tetrahedrons == null) tetrahedrons = FindObject(scatterplot, "Tetrahedron");

		if(dataChanger == null) dataChanger = (DataChangerScript)FindObjectOfType(typeof(DataChangerScript));
        currentDataset = dataChanger.datasets[dataChanger.currentDataIndex];

        //load cubes
        if (currentVizIndex == 0)
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

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (cubesCounter == 0)
            {
                cubesCounter++;
                cubes.GetComponent<DataSpaceHandler>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                cubes.SetActive(true);
            }
            else
            {
                cubes.SetActive(true);
                cubes.GetComponent<DataSpaceHandler>().changeDatafile(currentDataset);
            }

        }
        else if (currentVizIndex == 1)
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

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (piesCounter == 0)
            {
                piesCounter++;
                pies.GetComponent<PieChartMeshController>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                pies.SetActive(true);
            }
            else
            {
                pies.SetActive(true);
                pies.GetComponent<PieChartMeshController>().changeDatafile(currentDataset);
            }
        }
        else if (currentVizIndex == 2)
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
            if (trnglCounter == 0)
            {
                trnglCounter++;
                triangles.GetComponent<Triangle>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                triangles.SetActive(true);
            }
            else
            {
                triangles.SetActive(true);
                triangles.GetComponent<Triangle>().changeDatafile(currentDataset);
            }
        }
        else if (currentVizIndex == 3)
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
            else if (triangles.activeSelf)
            {
                Triangle dummy = triangles.GetComponent<Triangle>();
                dummy.resetMe();
                triangles.SetActive(false);
            }

            //this is needed since the first time the Start function is being called two times for some reason, and after that not even once
            if (ttrhdrnCounter == 0)
            {
                ttrhdrnCounter++;
                tetrahedrons.GetComponent<Tetrahedron>().data = dataChanger.datasets[dataChanger.currentDataIndex];
                tetrahedrons.SetActive(true);
            }
            else
            {
                tetrahedrons.SetActive(true);
                tetrahedrons.GetComponent<Tetrahedron>().changeDatafile(currentDataset);
            }
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

    private void OnEnable()
    {
        if (vizChanger == null) vizChanger = this;
    }
    private void OnDisable()
    {
        if (vizChanger != null) vizChanger = null;
    }
}
