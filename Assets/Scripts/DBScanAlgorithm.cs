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

    public int clusterID, UNCLASSIFIED, NOISE;
    // Use this for initialization
    void Start () {
        counter = 0;
        clusterID = 1;
        UNCLASSIFIED = 0;
        NOISE = -1;
        epsilon = 0.01f;
        minPts = 3;
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

        /*foreach(Transform dataPoint in dataVisuals.transform)
        {*/
        GameObject dataPoint = dataPoints[0];
        dataPoints.Remove(dataPoint);
        if(dataPoint.gameObject.GetComponent<DBScanProperties>() == null)
        {
            dataPoint.gameObject.AddComponent<DBScanProperties>();
        }
        dataPoint.gameObject.GetComponent<DBScanProperties>().epsilon = epsilon;

        //process points only once
        if(dataPoint.gameObject.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
        {
            if (ExpandCluster(dataVisuals.transform, dataPoint.gameObject, clusterID, epsilon, minPts))
            {
                clusterID++;
                if(clusterID>20)
                {
                    Color color = Random.ColorHSV();
                    pointsColor.Add(color);
                }
            }
        }
        //}
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

    //Expand the cluster
    private bool ExpandCluster(Transform dataPoints, GameObject p, int clusterID, float epsilon, int minPts)
    {
        List<GameObject> seeds = new List<GameObject>();
        seeds = RegionQuery(p, epsilon);
        if (seeds.Count < minPts) //no core point
        {
            p.GetComponent<DBScanProperties>().clusterID = NOISE;
            return false;
        }

        else // enough to create a cluster
        {
            for (int i = 0; i < seeds.Count; i++)
            {
                if(seeds[i].GetComponent<DBScanProperties>() == null)
                {
                    seeds[i].AddComponent<DBScanProperties>();
                }
                seeds[i].GetComponent<DBScanProperties>().clusterID = clusterID;
            }
            seeds.Remove(p);
            while(seeds.Count>0)
            {
                GameObject currentPoint = seeds[0];
                List<GameObject> result = RegionQuery(currentPoint, epsilon);

                //continue if it is a branch but not a leaf
                if(result.Count >= minPts)
                {
                    for(int j = 0; j < result.Count; j++)
                    {
                        GameObject resultPoint = result[j];
                        if (resultPoint.GetComponent<DBScanProperties>().clusterID == NOISE) resultPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                        else if(resultPoint.GetComponent<DBScanProperties>().clusterID == UNCLASSIFIED)
                        {
                            resultPoint.GetComponent<DBScanProperties>().clusterID = clusterID;
                            seeds.Add(resultPoint);
                        }
                    }
                }
                seeds.Remove(currentPoint);
            }
            return true;
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
