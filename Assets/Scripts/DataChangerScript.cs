using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataChangerScript : NetworkBehaviour {
    public static DataChangerScript dataChanger;
    public TextAsset[] datasets;
    public bool isSelected;
    public int currentDataIndex;

    private GameObject ground, scatterplot, cubes, pies, triangles, tetrahedrons, datasetButtonsParent;
    private DataSpaceHandler cubesData;
    private PieChartMeshController piesData;
    private Triangle trianglesData;
    private Tetrahedron tetrahedronsData;
    private KMeansAlgorithm resetKmeans;
    private DBScanAlgorithm resetDBScan;
    private DenclueAlgorithm resetDenclue;

    [SyncVar]
    private int dataIndex;
    // Use this for initialization
    void Start() {
        ground = GameObject.Find("Ground");
        transform.parent = GameObject.Find("EventSystem").transform;

        resetKmeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        resetDBScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        resetDenclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.O))
        {
            CmdLoadDataset(dataIndex);
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject localPlayer = new GameObject();
            NetworkIdentity networkIdentity = new NetworkIdentity();
            foreach (var player in players)
            {
                networkIdentity = player.GetComponent<NetworkIdentity>();
                if (!player.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
            localPlayer.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(netId, localPlayer.GetComponent<NetworkIdentity>());
        }
    }

    [Command]
    public void CmdLoadDataset(int index)
    {
        Debug.Log("Dataset Loaded on server!");
        currentDataIndex = index;
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();
        //Reset the K-means algorithm in case the dataset is changed
        resetKmeans.ResetMe();

        //Reset the DBScan algorithm in case the dataset is changed
        resetDBScan.ResetMe();

        resetDenclue.ResetMe();

        if (scatterplot == null) scatterplot = GameObject.Find("ScatterplotElements");
        if (cubes == null) cubes = FindObject(scatterplot, "DataSpace");
        if (pies == null) pies = FindObject(scatterplot, "PieChartCtrl");
        if (triangles == null) triangles = FindObject(scatterplot, "Triangle");
        if (tetrahedrons == null) tetrahedrons = FindObject(scatterplot, "Tetrahedron");
        if (cubesData == null) cubesData = FindObjectOfType<DataSpaceHandler>();
        if (piesData == null) piesData = FindObjectOfType<PieChartMeshController>();
        if (trianglesData == null) trianglesData = FindObjectOfType<Triangle>();
        if (tetrahedronsData == null) tetrahedronsData = FindObjectOfType<Tetrahedron>();
        foreach (Transform obj in transform.parent)
        {
            if (obj.GetComponent<datasetChangerScript>() != null) obj.GetComponent<datasetChangerScript>().isSelected = false;
        }
        isSelected = true;
        if (cubes.activeSelf)
        {
            cubesData.changeDatafile(datasets[index]);
        }
        else if (pies.activeSelf)
        {
            piesData.changeDatafile(datasets[index]);
        }
        else if (triangles.activeSelf)
        {
            trianglesData.changeDatafile(datasets[index]);
        }
        else if (tetrahedrons.activeSelf)
        {
            tetrahedronsData.changeDatafile(datasets[currentDataIndex]);
        }

        RpcLoadDataset(index);
    }

    [ClientRpc]
    public void RpcLoadDataset(int index)
    {
        if (isServer) return;
        Debug.Log("Dataset Loaded on client!");
        currentDataIndex = index;
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();
        //Reset the K-means algorithm in case the dataset is changed
        resetKmeans.ResetMe();

        //Reset the DBScan algorithm in case the dataset is changed
        resetDBScan.ResetMe();

        resetDenclue.ResetMe();

        if (scatterplot == null) scatterplot = GameObject.Find("ScatterplotElements");
        if (cubes == null) cubes = FindObject(scatterplot, "DataSpace");
        if (pies == null) pies = FindObject(scatterplot, "PieChartCtrl");
        if (triangles == null) triangles = FindObject(scatterplot, "Triangle");
        if (tetrahedrons == null) tetrahedrons = FindObject(scatterplot, "Tetrahedron");
        if (cubesData == null) cubesData = FindObjectOfType<DataSpaceHandler>();
        if (piesData == null) piesData = FindObjectOfType<PieChartMeshController>();
        if (trianglesData == null) trianglesData = FindObjectOfType<Triangle>();
        if (tetrahedronsData == null) tetrahedronsData = FindObjectOfType<Tetrahedron>();
        foreach (Transform obj in transform.parent)
        {
            if (obj.GetComponent<datasetChangerScript>() != null) obj.GetComponent<datasetChangerScript>().isSelected = false;
        }
        isSelected = true;
        if (cubes.activeSelf)
        {
            cubesData.changeDatafile(datasets[index]);
        }
        else if (pies.activeSelf)
        {
            piesData.changeDatafile(datasets[index]);
        }
        else if (triangles.activeSelf)
        {
            trianglesData.changeDatafile(datasets[index]);
        }
        else if (tetrahedrons.activeSelf)
        {
            tetrahedronsData.changeDatafile(datasets[currentDataIndex]);
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
        if (dataChanger == null) dataChanger = this;
    }
    private void OnDisable()
    {
        if (dataChanger != null) dataChanger = null;
    }
}
