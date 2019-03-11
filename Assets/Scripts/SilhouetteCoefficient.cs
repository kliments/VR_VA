using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilhouetteCoefficient : MonoBehaviour {
    public bool calculate;
    public Transform dataContainer;
    public ClusteringAlgorithm currentAlgorithm;
    private List<GameObject> dataPoints;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(calculate)
        {
            calculate = false;
            AssignData();
            Debug.Log(Coef());
        }
	}

    private float Coef()
    {
        int clusterID = 0;
        float count = 0;
        float coef = 0;
        float minAverage = 0;
        foreach(Transform point in dataContainer)
        {
            clusterID = point.GetComponent<ClusterQualityValues>().clusterID;
            point.GetComponent<ClusterQualityValues>().Ai = AverageDistance(point.gameObject, currentAlgorithm.clusters[clusterID]);
            for(int i=0; i<currentAlgorithm.clusters.Count; i++)
            {
                if (i == clusterID) continue;
                if (minAverage > AverageDistance(point.gameObject, currentAlgorithm.clusters[i])) minAverage = AverageDistance(point.gameObject, currentAlgorithm.clusters[i]);
            }
            point.GetComponent<ClusterQualityValues>().Bi = minAverage;
            point.GetComponent<ClusterQualityValues>().Si = (point.GetComponent<ClusterQualityValues>().Bi - point.GetComponent<ClusterQualityValues>().Ai) / Mathf.Max(point.GetComponent<ClusterQualityValues>().Ai, point.GetComponent<ClusterQualityValues>().Bi);
            coef += point.GetComponent<ClusterQualityValues>().Si;
            count++;
        }
        return coef/count;
    }

    void AssignData()
    {
        dataPoints = new List<GameObject>();
        foreach(Transform child in dataContainer)
        {
            if(child.gameObject.GetComponent<ClusterQualityValues>() != null)
            {
                dataPoints.Add(child.gameObject);
            }
        }
    }

    float AverageDistance(GameObject point, List<GameObject> cluster)
    {
        float totalDist = 0;
        foreach(var p in cluster)
        {
            if (p == point) continue;
            totalDist += Vector3.Distance(point.transform.position, p.transform.position);
        }
        return totalDist / cluster.Count;
    }
}
