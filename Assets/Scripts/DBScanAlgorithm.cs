using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBScanAlgorithm : MonoBehaviour {
    public float epsilon;
    public int minPts;

    public Transform scatterplot;

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
    private List<GameObject> corePoints;

    public int clusterID, UNCLASSIFIED, NOISE;
    // Use this for initialization
    void Start () {
        counter = 0;
        clusterID = 1;
        UNCLASSIFIED = 0;
        NOISE = -1;
        epsilon = 0.025f;
        minPts = 3;
        corePoints = new List<GameObject>();
        neighbours = new List<List<GameObject>>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void StartDBSCAN()
    {
        if(counter == 0)
        {
            AssignDataPoints();
            counter++;
        }
        if(dataPoints.Count == 0)
        {
            Debug.Log("DBScan finished in " + clusterID.ToString() + " steps!");
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
                    corePoints = RegionQuery(dataPoint, epsilon);
                    if (corePoints.Count < minPts) //no core point
                    {
                        dataPoint.GetComponent<DBScanProperties>().clusterID = NOISE;
                        corePoints = new List<GameObject>();
                    }

                    else
                    {
                        for (int i = 0; i < corePoints.Count; i++)
                        {
                            if (corePoints[i].GetComponent<DBScanProperties>() == null)
                            {
                                corePoints[i].AddComponent<DBScanProperties>();
                            }
                            corePoints[i].GetComponent<DBScanProperties>().epsilon = epsilon;
                            corePoints[i].GetComponent<DBScanProperties>().clusterID = clusterID;
                            dataPoints.Remove(corePoints[i]);
                        }
                        corePoints.Remove(dataPoint);
                        List<GameObject> temp = new List<GameObject>();
                        while (corePoints.Count > 0)
                        {
                            GameObject currentPoint = corePoints[0];
                            List<GameObject> result = RegionQuery(currentPoint, epsilon);
                            foreach (GameObject obj in result)
                            {
                                if (obj.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED || obj.GetComponent<DBScanProperties>().clusterID == NOISE)
                                {
                                    temp.Add(obj);
                                    obj.GetComponent<DBScanProperties>().clusterID = clusterID;
                                    obj.GetComponent<DBScanProperties>().epsilon = epsilon;
                                    dataPoints.Remove(obj);
                                }
                            }
                            corePoints.Remove(currentPoint);
                        }
                        neighbours.Add(temp);
                    }
                }
                dataPoints.Remove(dataPoint);
            }

            //explore the neighbours, and their neighbours, and so on..
            else
            {
                List<GameObject> currentNeighbours = neighbours[0];
                List<GameObject> temp = new List<GameObject>();
                while (currentNeighbours.Count > 0)
                {
                    GameObject currentPoint = currentNeighbours[0];
                    List<GameObject> result = RegionQuery(currentPoint, epsilon);
                    if (result.Count > 0)
                    {
                        foreach (GameObject obj in result)
                        {
                            if (obj.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED || obj.GetComponent<DBScanProperties>().clusterID == NOISE)
                            {
                                obj.GetComponent<DBScanProperties>().epsilon = epsilon;
                                obj.GetComponent<DBScanProperties>().clusterID = clusterID;
                                temp.Add(obj);
                                dataPoints.Remove(obj);
                            }
                        }
                    }
                    currentNeighbours.Remove(currentPoint);
                }

                //add only if there are any new neighbours
                if (temp.Count > 0)
                {
                    neighbours.Add(temp);
                }
                neighbours.RemoveAt(0);

                if (neighbours.Count == 0)
                {
                    clusterID++;
                    if (clusterID > 20)
                    {
                        Color color = Random.ColorHSV();
                        pointsColor.Add(color);
                    }
                }
            }
        }
    }

    //find all datapoints
    void AssignDataPoints()
    {
        foreach(Transform child in scatterplot)
        {
            //Finding the current visualization
            if (child.gameObject.name == "DataSpace" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach(Transform obj in dataVisuals.transform)
                {
                    dataPoints.Add(obj.gameObject);
                }
                return;
            }

            else if (child.gameObject.name == "PieChartCtrl" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach (GameObject obj in dataVisuals.transform)
                {
                    dataPoints.Add(obj);
                }
                return;
            }

            else if (child.gameObject.name == "Triangle" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach (GameObject obj in dataVisuals.transform)
                {
                    dataPoints.Add(obj);
                }
                return;
            }

            else if (child.gameObject.name == "Tetrahedron" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                foreach (GameObject obj in dataVisuals.transform)
                {
                    dataPoints.Add(obj);
                }
                return;
            }
        }
    }

    

    //return all neighbours around the data point
    private List<GameObject> RegionQuery(GameObject obj, float eps)
    {
        List<GameObject> nghbrs = new List<GameObject>();
        foreach(Transform child in dataVisuals.transform)
        {
            if (Distance(child.transform.position, obj.transform.position) <= epsilon)
            {
                nghbrs.Add(child.gameObject);
            }
        }
        return nghbrs;
    }

    private float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }
}
