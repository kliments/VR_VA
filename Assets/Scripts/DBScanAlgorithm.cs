﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DBScanAlgorithm : ClusteringAlgorithm {

    public SilhouetteCoefficient silhouetteCoef;
    public Transform scatterplot, canvas;
    public KMeansAlgorithm resetKMeans;
    public DenclueAlgorithm resetDenclue;

    //current data points visualisation
    private GameObject dataVisuals;

    //plane to show when dbscan is finished
    public GameObject dbscanFinishedPlane;

    //color of data points
    public List<Color> pointsColor;

    //the actual data points
    public List<GameObject> dataPoints, noisePoints;

    //counter for steps
    public int counter;

    private List<List<GameObject>> neighbours;
    private List<GameObject> corePoints, currentCluster;

    public int clusterID, UNCLASSIFIED, tempClusterID;
    
    //list of steps, used for backward dbscan
    private List<string> steps;
    private List<List<GameObject>> processedPoints;

    public bool allClustersFound, nextStep, prevStep;
    public DBScanPlay playRoutine;

    private Text prevText1;
    
    private SyncListInt indexList = new SyncListInt();

    [SyncVar]
    public float epsilon;
    [SyncVar]
    public int minPts;

    [SyncVar]
    //Eucledian distance if true, if not then Manhattan
    public bool euclDist;
    // Use this for initialization
    void Start()
    {
        FindAllObjects();
        counter = 0;
        clusterID = 1;
        UNCLASSIFIED = 0;
        NOISE = -1;
        epsilon = 0.05f;
        minPts = 3;
        euclDist = true;
        corePoints = new List<GameObject>();
        neighbours = new List<List<GameObject>>();
        allClustersFound = false;
        steps = new List<string>();
        processedPoints = new List<List<GameObject>>();
        clusters = new List<List<GameObject>>();
        currentCluster = new List<GameObject>();
    }

    // Update is called once per frame
    void Update() {
        if (nextStep)
        {
            nextStep = false;
            CmdStartDBSCAN();
        }
        if(prevStep)
        {
            prevStep = false;
            CmdDBBackwards();
        }
    }

    void FindAllObjects()
    {
        transform.parent = GameObject.Find("EventSystem").transform;
        silhouetteCoef = (SilhouetteCoefficient)FindObjectOfType(typeof(SilhouetteCoefficient));
        scatterplot = GameObject.Find("ScatterplotElements").transform;
        resetKMeans = (KMeansAlgorithm)FindObjectOfType(typeof(KMeansAlgorithm));
        resetDenclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        playRoutine = GetComponent<DBScanPlay>();
        canvas = GameObject.Find("Canvas").transform;

        foreach (Transform child in canvas.transform)
        {
            if (child.name == "dbscanPseudo")
            {
                pseudoCodeText = child.gameObject;
                break;
            }
        }
        foreach (Transform child in scatterplot)
        {
            if(child.name == "dbScanFinishedPlane")
            {
                dbscanFinishedPlane = child.gameObject;
                break;
            }
        }
    }

    private void OnIntChanged(SyncListInt.Operation op, int index)
    {

    }

    public override void OnStartClient()
    {
        indexList.Callback = OnIntChanged;
    }

    [Command]
    public void CmdStartDBSCAN()
    {
        if (!isServer) return;

        //happens only once, in the beginning
        if (counter == 0)
        {
            resetKMeans.GetComponent<KMeansAlgorithm>().ResetMe();
            resetDenclue.GetComponent<DenclueAlgorithm>().ResetMe();
            silhouetteCoef.currentAlgorithm = this;
            pseudoCodeText.SetActive(true);
            AssignDataPoints();
            PaintAllWhite();
            CmdShuffleDataPoints();
            counter++;
            steps.Add("firstStep");
        }
        else if (dataPoints.Count == 0 && neighbours.Count == 0)
        {
            Debug.Log("DBScan finished in " + NrOfClusters(dataVisuals.transform).ToString() + " steps!");
            dbscanFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = NrOfClusters(dataVisuals.transform).ToString() + " clusters found!";
            dbscanFinishedPlane.SetActive(true);
            allClustersFound = true;
            steps.Add("finish");
            int qualityClusterID = 0;
            foreach (var cluster in clusters)
            {
                foreach (var point in cluster)
                {
                    point.GetComponent<ClusterQualityValues>().clusterID = qualityClusterID;
                }
                qualityClusterID++;
            }
            foreach (var point in noisePoints)
            {
                point.GetComponent<ClusterQualityValues>().clusterID = NOISE;
            }

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(5).GetComponent<Text>();
        }
        else
        {//check if there are any neighbours to expand, before trying to find another cluster
            if (neighbours.Count == 0)
            {
                GameObject dataPoint = dataPoints[0];
                if (dataPoint.gameObject.GetComponent<DBScanProperties>() == null)
                {
                    dataPoint.gameObject.AddComponent<DBScanProperties>();
                }

                //process points only once
                if (dataPoint.gameObject.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
                {
                    List<GameObject> temp = new List<GameObject>();
                    corePoints = RegionQuery(dataPoint, epsilon, euclDist);

                    //paint pseudo code text red
                    if (prevText != null) prevText.color = Color.black;
                    if (prevText1 != null) prevText1.color = Color.black;

                    pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

                    pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
                    prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();

                    if (corePoints.Count < minPts) //no core point
                    {
                        dataPoint.GetComponent<DBScanProperties>().clusterID = NOISE;
                        dataPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                        dataPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                        if (dataPoint.transform.parent.name == "PieChartCtrl") PaintPieChart(dataPoint, Color.black);
                        else dataPoint.GetComponent<MeshRenderer>().material.color = Color.black;
                        noisePoints.Add(dataPoint);
                        corePoints = new List<GameObject>();
                        RemoveWireFrame(dataPoint);
                        //necessary for previous steps
                        steps.Add("noise");
                    }

                    else
                    {
                        dataPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                        dataPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                        dataPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                        clusteredPoints++;
                        steps.Add("ptHaveNbrs");

                        temp = new List<GameObject>();
                        for (int i = 0; i < corePoints.Count; i++)
                        {
                            if (corePoints[i].GetComponent<DBScanProperties>() == null)
                            {
                                corePoints[i].AddComponent<DBScanProperties>();
                            }
                            corePoints[i].GetComponent<DBScanProperties>().epsilon = epsilon;
                            corePoints[i].GetComponent<DBScanProperties>().clusterID = clusterID;
                            if(corePoints[i].transform.parent.name == "PieChartCtrl") PaintPieChart(corePoints[i], pointsColor[clusterID - 1]);
                            else corePoints[i].GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
                            temp.Add(corePoints[i]);

                            currentCluster.Add(corePoints[i]);
                            dataPoints.Remove(corePoints[i]);
                        }
                        corePoints.Remove(dataPoint);
                        neighbours.Add(corePoints);
                        processedPoints.Insert(processedPoints.Count, copyList(corePoints));
                    }
                    temp = new List<GameObject>();
                    temp.Add(dataPoint);
                    processedPoints.Insert(processedPoints.Count, copyList(temp));
                }
                dataPoints.Remove(dataPoint);
            }

            //explore the neighbours' neighbours
            else
            {
                List<GameObject> currentNeighbours = neighbours[0];
                processedPoints.Insert(processedPoints.Count, copyList(currentNeighbours));
                List<GameObject> temp = new List<GameObject>();
                while (currentNeighbours.Count > 0)
                {
                    GameObject currentPoint = currentNeighbours[0];
                    currentPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                    currentPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                    currentPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                    clusteredPoints++;
                    List<GameObject> result = RegionQuery(currentPoint, epsilon, euclDist);
                    if (result.Count > 0)
                    {
                        foreach (GameObject obj in result)
                        {
                            if (obj.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED || obj.GetComponent<DBScanProperties>().clusterID == NOISE)
                            {
                                obj.GetComponent<DBScanProperties>().epsilon = epsilon;
                                obj.GetComponent<DBScanProperties>().clusterID = clusterID;
                                if(obj.transform.parent.name == "PieChartCtrl")
                                {
                                    PaintPieChart(obj, pointsColor[clusterID - 1]);
                                }
                                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
                                temp.Add(obj);
                                currentCluster.Add(obj);
                                dataPoints.Remove(obj);
                                //only for cubes
                                if (obj.name.Contains("Cube"))
                                {
                                    AddWireFrame(obj);
                                }
                            }
                        }
                    }
                    dataPoints.Remove(currentPoint);
                    currentNeighbours.Remove(currentPoint);
                }

                //add only if there are any new neighbours
                if (temp.Count > 0)
                {
                    neighbours.Add(temp);
                    processedPoints.Insert(processedPoints.Count, copyList(temp));
                    steps.Add("haveNbrs");

                    //paint pseudo code text red
                    prevText.color = Color.black;
                    prevText1.color = Color.black;
                    pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();

                }
                neighbours.RemoveAt(0);

                if (neighbours.Count == 0)
                {
                    //paint pseudo code text red
                    prevText.color = Color.black;
                    prevText1.color = Color.black;
                    pseudoCodeText.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(4).GetComponent<Text>();

                    clusters.Add(currentCluster);
                    currentCluster = new List<GameObject>();
                    clusterID++;
                    if (clusterID > 20)
                    {
                        Color color = Random.ColorHSV();
                        pointsColor.Add(color);
                    }
                    steps.Add("noNbrs");
                }
            }
        }
        RpcStartDBSCAN();
    }

    [ClientRpc]
    public void RpcStartDBSCAN()
    {
        if (hasAuthority) return;
        //happens only once, in the beginning
        if (counter == 0)
        {
            resetKMeans.GetComponent<KMeansAlgorithm>().ResetMe();
            resetDenclue.GetComponent<DenclueAlgorithm>().ResetMe();
            silhouetteCoef.currentAlgorithm = this;
            pseudoCodeText.SetActive(true);
            AssignDataPoints();
            AssignRandomPointsFromServer();
            PaintAllWhite();
            //ShuffleDataPoints();
            counter++;
            steps.Add("firstStep");
        }
        else if (dataPoints.Count == 0 && neighbours.Count == 0)
        {
            Debug.Log("DBScan finished in " + NrOfClusters(dataVisuals.transform).ToString() + " steps!");
            dbscanFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = NrOfClusters(dataVisuals.transform).ToString() + " clusters found!";
            dbscanFinishedPlane.SetActive(true);
            allClustersFound = true;
            steps.Add("finish");
            int qualityClusterID = 0;
            foreach (var cluster in clusters)
            {
                foreach (var point in cluster)
                {
                    point.GetComponent<ClusterQualityValues>().clusterID = qualityClusterID;
                }
                qualityClusterID++;
            }
            foreach (var point in noisePoints)
            {
                point.GetComponent<ClusterQualityValues>().clusterID = NOISE;
            }

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(5).GetComponent<Text>();
        }
        else
        {//check if there are any neighbours to expand, before trying to find another cluster
            if (neighbours.Count == 0)
            {
                GameObject dataPoint = dataPoints[0];
                if (dataPoint.gameObject.GetComponent<DBScanProperties>() == null)
                {
                    dataPoint.gameObject.AddComponent<DBScanProperties>();
                }

                //process points only once
                if (dataPoint.gameObject.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
                {
                    List<GameObject> temp = new List<GameObject>();
                    corePoints = RegionQuery(dataPoint, epsilon, euclDist);

                    //paint pseudo code text red
                    if (prevText != null) prevText.color = Color.black;
                    if (prevText1 != null) prevText1.color = Color.black;

                    pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

                    pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
                    prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();

                    if (corePoints.Count < minPts) //no core point
                    {
                        dataPoint.GetComponent<DBScanProperties>().clusterID = NOISE;
                        dataPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                        dataPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                        if (dataPoint.transform.parent.name == "PieChartCtrl") PaintPieChart(dataPoint, Color.black);
                        else dataPoint.GetComponent<MeshRenderer>().material.color = Color.black;
                        noisePoints.Add(dataPoint);
                        corePoints = new List<GameObject>();
                        RemoveWireFrame(dataPoint);
                        //necessary for previous steps
                        steps.Add("noise");
                    }

                    else
                    {
                        dataPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                        dataPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                        dataPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                        clusteredPoints++;
                        steps.Add("ptHaveNbrs");

                        temp = new List<GameObject>();
                        for (int i = 0; i < corePoints.Count; i++)
                        {
                            if (corePoints[i].GetComponent<DBScanProperties>() == null)
                            {
                                corePoints[i].AddComponent<DBScanProperties>();
                            }
                            corePoints[i].GetComponent<DBScanProperties>().epsilon = epsilon;
                            corePoints[i].GetComponent<DBScanProperties>().clusterID = clusterID;
                            if (corePoints[i].transform.parent.name == "PieChartCtrl") PaintPieChart(corePoints[i], pointsColor[clusterID - 1]);
                            else corePoints[i].GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
                            temp.Add(corePoints[i]);

                            currentCluster.Add(corePoints[i]);
                            dataPoints.Remove(corePoints[i]);
                        }
                        corePoints.Remove(dataPoint);
                        neighbours.Add(corePoints);
                        processedPoints.Insert(processedPoints.Count, copyList(corePoints));
                    }
                    temp = new List<GameObject>();
                    temp.Add(dataPoint);
                    processedPoints.Insert(processedPoints.Count, copyList(temp));
                }
                dataPoints.Remove(dataPoint);
            }

            //explore the neighbours' neighbours
            else
            {
                List<GameObject> currentNeighbours = neighbours[0];
                processedPoints.Insert(processedPoints.Count, copyList(currentNeighbours));
                List<GameObject> temp = new List<GameObject>();
                while (currentNeighbours.Count > 0)
                {
                    GameObject currentPoint = currentNeighbours[0];
                    currentPoint.GetComponent<DBScanProperties>().epsilon = epsilon;
                    currentPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                    currentPoint.GetComponent<DBScanProperties>().drawMeshAround = true;
                    clusteredPoints++;
                    List<GameObject> result = RegionQuery(currentPoint, epsilon, euclDist);
                    if (result.Count > 0)
                    {
                        foreach (GameObject obj in result)
                        {
                            if (obj.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED || obj.GetComponent<DBScanProperties>().clusterID == NOISE)
                            {
                                obj.GetComponent<DBScanProperties>().epsilon = epsilon;
                                obj.GetComponent<DBScanProperties>().clusterID = clusterID;
                                if (obj.transform.parent.name == "PieChartCtrl")
                                {
                                    PaintPieChart(obj, pointsColor[clusterID - 1]);
                                }
                                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
                                temp.Add(obj);
                                currentCluster.Add(obj);
                                dataPoints.Remove(obj);
                                //only for cubes
                                if (obj.name.Contains("Cube"))
                                {
                                    AddWireFrame(obj);
                                }
                            }
                        }
                    }
                    dataPoints.Remove(currentPoint);
                    currentNeighbours.Remove(currentPoint);
                }

                //add only if there are any new neighbours
                if (temp.Count > 0)
                {
                    neighbours.Add(temp);
                    processedPoints.Insert(processedPoints.Count, copyList(temp));
                    steps.Add("haveNbrs");

                    //paint pseudo code text red
                    prevText.color = Color.black;
                    prevText1.color = Color.black;
                    pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();

                }
                neighbours.RemoveAt(0);

                if (neighbours.Count == 0)
                {
                    //paint pseudo code text red
                    prevText.color = Color.black;
                    prevText1.color = Color.black;
                    pseudoCodeText.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                    prevText = pseudoCodeText.transform.GetChild(4).GetComponent<Text>();

                    clusters.Add(currentCluster);
                    currentCluster = new List<GameObject>();
                    clusterID++;
                    if (clusterID > 20)
                    {
                        Color color = Random.ColorHSV();
                        pointsColor.Add(color);
                    }
                    steps.Add("noNbrs");
                }
            }
        }
    }

    [Command]
    //Backward step of DBSCAN algorithm
    public void CmdDBBackwards()
    {
        if (!isServer) return;
        int currentStep = steps.Count - 1;
        if (steps.Count <= 0)
        {
            return;
        }
        if (steps[currentStep] == "firstStep")
        {
            ReturnOriginalColor();
            counter--;
            steps.RemoveAt(currentStep);
        }
        else if (steps[currentStep] == "noise")
        {
            GameObject current = processedPoints[processedPoints.Count - 1][0];
            current.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
            if(current.transform.parent.name == "PieChartCtrl")
            {
                PaintPieChart(current, Color.white);
            }
            else current.GetComponent<MeshRenderer>().material.color = Color.white;
            dataPoints.Insert(0, current);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;

            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();
        }
        else if (steps[currentStep] == "ptHaveNbrs")
        {
            GameObject current = processedPoints[processedPoints.Count - 1][0];
            current.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
            current.GetComponent<DBScanProperties>().drawMeshAround = false;
            current.GetComponent<DBScanProperties>().ResetPoint();
            if (current.transform.parent.name == "PieChartCtrl") PaintPieChart(current, Color.white);
            else current.GetComponent<MeshRenderer>().material.color = Color.white;
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;

                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, Color.white);
                else obj.GetComponent<MeshRenderer>().material.color = Color.white;
                dataPoints.Insert(0, obj);
            }
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            neighbours = new List<List<GameObject>>();

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();
        }
        else if (steps[currentStep] == "haveNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, Color.white);
                else obj.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, pointsColor[tempClusterID - 1]);
                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();

        }
        else if (steps[currentStep] == "noNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, pointsColor[tempClusterID - 1]);
                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            clusterID--;
            clusters.RemoveAt(clusters.Count - 1);
            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(4).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(4).GetComponent<Text>();
        }
        else if(steps[currentStep] == "finish")
        {
            dbscanFinishedPlane.SetActive(false);
            allClustersFound = false;
            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(5).GetComponent<Text>();
            steps.RemoveAt(currentStep);
            CmdDBBackwards();
        }
        RpcDBBackwards();
    }

    [ClientRpc]
    public void RpcDBBackwards()
    {
        if (hasAuthority) return;
        int currentStep = steps.Count - 1;
        if (steps.Count <= 0)
        {
            return;
        }
        if (steps[currentStep] == "firstStep")
        {
            ReturnOriginalColor();
            counter--;
            steps.RemoveAt(currentStep);
        }
        else if (steps[currentStep] == "noise")
        {
            GameObject current = processedPoints[processedPoints.Count - 1][0];
            current.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
            if (current.transform.parent.name == "PieChartCtrl")
            {
                PaintPieChart(current, Color.white);
            }
            else current.GetComponent<MeshRenderer>().material.color = Color.white;
            dataPoints.Insert(0, current);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;

            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();
        }
        else if (steps[currentStep] == "ptHaveNbrs")
        {
            GameObject current = processedPoints[processedPoints.Count - 1][0];
            current.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
            current.GetComponent<DBScanProperties>().drawMeshAround = false;
            current.GetComponent<DBScanProperties>().ResetPoint();
            if (current.transform.parent.name == "PieChartCtrl") PaintPieChart(current, Color.white);
            else current.GetComponent<MeshRenderer>().material.color = Color.white;
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;

                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, Color.white);
                else obj.GetComponent<MeshRenderer>().material.color = Color.white;
                dataPoints.Insert(0, obj);
            }
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            neighbours = new List<List<GameObject>>();

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();

            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText1 = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();
        }
        else if (steps[currentStep] == "haveNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, Color.white);
                else obj.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, pointsColor[tempClusterID - 1]);
                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);

            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();

        }
        else if (steps[currentStep] == "noNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                if (obj.transform.parent.name == "PieChartCtrl") PaintPieChart(obj, pointsColor[tempClusterID - 1]);
                else obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            clusterID--;
            clusters.RemoveAt(clusters.Count - 1);
            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(4).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(4).GetComponent<Text>();
        }
        else if (steps[currentStep] == "finish")
        {
            dbscanFinishedPlane.SetActive(false);
            allClustersFound = false;
            //paint pseudo code text red
            prevText.color = Color.black;
            prevText1.color = Color.black;
            pseudoCodeText.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(5).GetComponent<Text>();
            steps.RemoveAt(currentStep);
            CmdDBBackwards();
        }
    }


    //find all datapoints
    void AssignDataPoints()
    {
        dataPoints = new List<GameObject>();
        foreach (Transform child in scatterplot)
        {
            //Finding the current visualization
            if (child.gameObject.tag == "coordinatesData" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach (Transform obj in dataVisuals.transform)
                {
                    dataPoints.Add(obj.gameObject);
                }
                break;
            }
        }
    }

    //return all neighbours around the data point
    private List<GameObject> RegionQuery(GameObject obj, float eps, bool euclDist)
    {
        List<GameObject> nghbrs = new List<GameObject>();
        foreach (Transform child in dataVisuals.transform)
        {
            if (euclDist)
            {
                if (EucledianDistance(child.transform.position, obj.transform.position) <= epsilon && child.gameObject.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
                {
                    if (child != obj)
                    {
                        nghbrs.Add(child.gameObject);
                    }
                }
            }
            else
            {
                if (ManhattanDistance(child.transform.position, obj.transform.position) <= epsilon && child.gameObject.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
                {
                    if (child != obj)
                    {
                        nghbrs.Add(child.gameObject);
                    }
                }
            }
        }
        return nghbrs;
    }

    private float EucledianDistance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    private float ManhattanDistance(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z));
    }

    public void ResetMe()
    {
        if (dataVisuals != null)
        {
            foreach (Transform obj in dataVisuals.transform)
            {
                obj.GetComponent<DBScanProperties>().ResetPoint();
                RemoveWireFrame(obj.gameObject);

                if (obj.transform.parent.name == "PieChartCtrl")
                {
                    obj.GetComponent<MeshRenderer>().materials[0].color = Color.red;
                    obj.GetComponent<MeshRenderer>().materials[1].color = Color.blue;
                    obj.GetComponent<MeshRenderer>().materials[2].color = Color.green;
                }
                else obj.GetComponent<MeshRenderer>().material.color = obj.GetComponent<PreviousStepProperties>().originalColor;
                obj.GetComponent<PreviousStepProperties>().colorList = new List<Color>();
            }
        }
        clusteredPoints = 0;
        clusterID = 1;
        corePoints = new List<GameObject>();
        neighbours = new List<List<GameObject>>();
        dataPoints = new List<GameObject>();
        counter = 0;
        allClustersFound = false;
        playRoutine.StopRoutine();
        dbscanFinishedPlane.SetActive(false);
        steps = new List<string>();
        processedPoints = new List<List<GameObject>>();
        clusters = new List<List<GameObject>>();
        currentCluster = new List<GameObject>();

        pseudoCodeText.SetActive(false);
        if (prevText != null) prevText.color = Color.black;
        if (prevText1 != null) prevText1.color = Color.black;
        noisePoints = new List<GameObject>();
        silhouetteCoef.ResetMe();
    }

    [Command]
    private void CmdShuffleDataPoints()
    {
        indexList.Clear();
        System.Random rng = new System.Random();
        int n = dataPoints.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            GameObject value = dataPoints[k];
            dataPoints[k] = dataPoints[n];
            dataPoints[n] = value;
        }

        foreach(var point in dataPoints)
        {
            indexList.Add(point.GetComponent<DBScanProperties>().index);
        }
    }

    private void AssignRandomPointsFromServer()
    {
        List<GameObject> tempList = new List<GameObject>();
        foreach(var obj in dataPoints)
        {
            tempList.Add(obj);
        }

        dataPoints = new List<GameObject>();
        for(int i=0; i<tempList.Count; i++)
        {
            dataPoints.Add(tempList[indexList[i]]);
        }
    }

    private void PaintAllWhite()
    {
        if (dataPoints[0].transform.parent.name == "PieChartCtrl")
        {
            foreach (var obj in dataPoints)
            {
                PaintPieChart(obj, Color.white);
            }
        }
        else
        {
            foreach (GameObject obj in dataPoints)
            {
                obj.GetComponent<MeshRenderer>().material.color = Color.white;

                //only for cubes
                if (obj.name.Contains("Cube"))
                {
                    AddWireFrame(obj);
                }
            }
        }
    }

    private void ReturnOriginalColor()
    {
        if(dataVisuals.name == "PieChartCtrl")
        {
            foreach (Transform obj in dataVisuals.transform)
            {
                obj.GetComponent<MeshRenderer>().materials[0].color = Color.red;
                obj.GetComponent<MeshRenderer>().materials[1].color = Color.blue;
                obj.GetComponent<MeshRenderer>().materials[2].color = Color.green;
            }
        }
        else
        {
            foreach (Transform obj in dataVisuals.transform)
            {
                obj.GetComponent<MeshRenderer>().material.color = obj.GetComponent<PreviousStepProperties>().originalColor;
            }
        }
    }

    private int NrOfClusters(Transform list)
    {
        int clusters = 0; ;
        foreach (Transform obj in list)
        {
            if (clusters < obj.GetComponent<DBScanProperties>().clusterID)
            {
                clusters = obj.GetComponent<DBScanProperties>().clusterID;
            }
        }
        return clusters;
    }

    private List<GameObject> copyList(List<GameObject> listToCopy)
    {
        List<GameObject> temp = new List<GameObject>(listToCopy);
        return temp;
    }

    private void AddWireFrame(GameObject obj)
    {
        //line renderer for drawing the Lines on the edges of the cube
        if (obj.GetComponent<LineRenderer>() == null)
        {
            obj.AddComponent<LineRenderer>();
        }
        if (obj.GetComponent<DrawWiredCube>() == null)
        {
            obj.AddComponent<DrawWiredCube>();
        }
        obj.GetComponent<DrawWiredCube>().DrawWire();
    }

    private void RemoveWireFrame(GameObject obj)
    {
        Destroy(obj.GetComponent<LineRenderer>());
        Destroy(obj.GetComponent<DrawWiredCube>());
    }

    private void PaintPieChart(GameObject point, Color color)
    {
        foreach(var mat in point.GetComponent<MeshRenderer>().materials)
        {
            mat.color = color;
        }
    }
}
