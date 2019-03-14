using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ClusteringAlgorithm : MonoBehaviour {

    public List<List<GameObject>> clusters;
    public GameObject pseudoCodeText;
    public Text prevText;
    public int clusteredPoints;
    public int NOISE = -1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float[][] SortClusters()
    {
        //List<List<float>> clustersValues = new List<List<float>>();
        float[][] clustersValues = new float[clusters.Count][];
        for(int i=0; i<clusters.Count; i++)
        {
            //List<float> cluster = new List<float>();
            clustersValues[i] = new float[clusters[i].Count];
            for(int j=0; j<clusters[i].Count; j++)
            {
                //cluster.Add(clusters[i][j].GetComponent<ClusterQualityValues>().Si);
                clustersValues[i][j] = clusters[i][j].GetComponent<ClusterQualityValues>().Si;
            }
            //clustersValues.Add(cluster);
        }
        foreach(var cluster in clustersValues)
        {
            System.Array.Sort(cluster);
        }
        return clustersValues;
    }
}
