using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBScanAlgorithm : ClusteringAlgorithm {
    public float epsilon;
    public int minPts;

    public SilhouetteCoefficient silhouetteCoef;
    public Transform scatterplot;
    public GameObject resetKMeans;
    public GameObject resetDenclue;

    //current data points visualisation
    private GameObject dataVisuals;

    //plane to show when dbscan is finished
    public GameObject dbscanFinishedPlane;

    //color of data points
    public List<Color> pointsColor;

    //the actual data points
    public List<GameObject> dataPoints;
    
    //counter for steps
    public int counter;
    
    private List<List<GameObject>> neighbours;
    private List<GameObject> corePoints, currentCluster;

    public int clusterID, UNCLASSIFIED, NOISE, tempClusterID;

    //Eucledian distance if true, if not then Manhattan
    public bool euclDist;

    //list of steps, used for backward dbscan
    private List<string> steps;
    private List<List<GameObject>> processedPoints;

    public bool allClustersFound, nextStep;
    public GameObject playRoutine;
    // Use this for initialization
    void Start () {
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
        silhouetteCoef = (SilhouetteCoefficient)FindObjectOfType(typeof(SilhouetteCoefficient));
    }
	
	// Update is called once per frame
	void Update () {
        if(nextStep)
        {
            nextStep = false;
            StartDBSCAN();
        }
    }

    public void StartDBSCAN()
    {
        //happens only once, in the beginning
        if(counter == 0)
        {
            resetKMeans.GetComponent<KMeansAlgorithm>().ResetMe();
            resetDenclue.GetComponent<DenclueAlgorithm>().ResetMe();
            silhouetteCoef.currentAlgorithm = this;

            AssignDataPoints();
            PaintAllWhite();
            ShuffleDataPoints();
            counter++;
            steps.Add("firstStep");
        }
        else if(dataPoints.Count == 0 && neighbours.Count == 0)
        {
            Debug.Log("DBScan finished in " + NrOfClusters(dataVisuals.transform).ToString() + " steps!");
            dbscanFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = NrOfClusters(dataVisuals.transform).ToString() + " clusters found!";
            dbscanFinishedPlane.SetActive(true);
            allClustersFound = true;
            steps.Add("finish");
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
                    if (corePoints.Count < minPts) //no core point
                    {
                        dataPoint.GetComponent<DBScanProperties>().clusterID = NOISE;
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
                            corePoints[i].GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
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
                    List<GameObject> result = RegionQuery(currentPoint, epsilon, euclDist);
                    if (result.Count > 0)
                    {
                        foreach (GameObject obj in result)
                        {
                            if (obj.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED || obj.GetComponent<DBScanProperties>().clusterID == NOISE)
                            {
                                obj.GetComponent<DBScanProperties>().epsilon = epsilon;
                                obj.GetComponent<DBScanProperties>().clusterID = clusterID;
                                obj.GetComponent<MeshRenderer>().material.color = pointsColor[clusterID - 1];
                                temp.Add(obj);
                                currentCluster.Add(obj);
                                dataPoints.Remove(obj);
                                //only for cubes
                                if(obj.name.Contains("Cube"))
                                {
                                    AddWireFrame(obj);
                                }
                            }
                        }
                    }
                    currentNeighbours.Remove(currentPoint);
                }

                //add only if there are any new neighbours
                if (temp.Count > 0)
                {
                    neighbours.Add(temp);
                    processedPoints.Insert(processedPoints.Count, copyList(temp));
                    steps.Add("haveNbrs");
                }
                neighbours.RemoveAt(0);

                if (neighbours.Count == 0)
                {
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
    
    //Backward step of DBSCAN algorithm
    public void DBBackwards()
    {
        int currentStep = steps.Count - 1;
        if(steps.Count <=0)
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
            current.GetComponent<MeshRenderer>().material.color = Color.white;
            dataPoints.Insert(0, current);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
        }
        else if (steps[currentStep] == "ptHaveNbrs")
        {
            GameObject current = processedPoints[processedPoints.Count - 1][0];
            current.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
            current.GetComponent<DBScanProperties>().drawMeshAround = false;
            current.GetComponent<DBScanProperties>().ResetPoint();
            current.GetComponent<MeshRenderer>().material.color = Color.white;
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count -1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
                obj.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            dataPoints.Insert(0, current);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            neighbours = new List<List<GameObject>>();
        }
        else if(steps[currentStep] == "haveNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                obj.GetComponent<DBScanProperties>().clusterID = UNCLASSIFIED;
                obj.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            processedPoints.RemoveAt(processedPoints.Count - 1);
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
        }
        else if (steps[currentStep] == "noNbrs")
        {
            foreach (GameObject obj in processedPoints[processedPoints.Count - 1])
            {
                tempClusterID = obj.GetComponent<DBScanProperties>().clusterID;
                obj.GetComponent<DBScanProperties>().ResetPoint();
                obj.GetComponent<DBScanProperties>().clusterID = tempClusterID;
                obj.GetComponent<MeshRenderer>().material.color = pointsColor[tempClusterID - 1];
            }
            neighbours = new List<List<GameObject>>();
            neighbours.Add(processedPoints[processedPoints.Count - 1]);
            processedPoints.RemoveAt(processedPoints.Count - 1);
            steps.RemoveAt(currentStep);
            clusterID--;
        }

    }

    //find all datapoints
    void AssignDataPoints()
    {
        foreach(Transform child in scatterplot)
        {
            //Finding the current visualization
            if (child.gameObject.tag == "coordinatesData" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach(Transform obj in dataVisuals.transform)
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
        foreach(Transform child in dataVisuals.transform)
        {
            if(euclDist)
            {
                if (EucledianDistance(child.transform.position, obj.transform.position) <= epsilon && child.gameObject.GetComponent<DBScanProperties>().clusterID <= 0)
                {
                    if (child != obj)
                    {
                        nghbrs.Add(child.gameObject);
                    }
                }
            }
            else
            {
                if (ManhattanDistance(child.transform.position, obj.transform.position) <= epsilon && child.gameObject.GetComponent<DBScanProperties>().clusterID <= 0)
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
        if(dataVisuals !=null)
        {
            foreach (Transform obj in dataVisuals.transform)
            {
                obj.GetComponent<DBScanProperties>().ResetPoint();
                RemoveWireFrame(obj.gameObject);
                obj.GetComponent<MeshRenderer>().material.color = obj.GetComponent<PreviousStepProperties>().originalColor;
                obj.GetComponent<PreviousStepProperties>().colorList = new List<Color>();
            }
        }
        clusterID = 1;
        corePoints = new List<GameObject>();
        neighbours = new List<List<GameObject>>();
        dataPoints = new List<GameObject>();
        counter = 0;
        allClustersFound = false;
        playRoutine.GetComponent<DBScanPlay>().StopRoutine();
        dbscanFinishedPlane.SetActive(false);
        steps = new List<string>();
        processedPoints = new List<List<GameObject>>();
        clusters = new List<List<GameObject>>();
        currentCluster = new List<GameObject>();
    }

    private void ShuffleDataPoints()
    {

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
    }

    private void PaintAllWhite()
    {
        foreach(GameObject obj in dataPoints)
        {
            obj.GetComponent<MeshRenderer>().material.color = Color.white;

            //only for cubes
            if(obj.name.Contains("Cube"))
            {
                AddWireFrame(obj);
            }
        }
    }

    private void ReturnOriginalColor()
    {
        foreach(Transform obj in dataVisuals.transform)
        {
            obj.GetComponent<MeshRenderer>().material.color = obj.GetComponent<PreviousStepProperties>().originalColor;
        }
    }

    private int NrOfClusters(Transform list)
    {
        int clusters = 0; ;
        foreach(Transform obj in list)
        {
            if(clusters < obj.GetComponent<DBScanProperties>().clusterID)
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
}
