using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNrOfEpsilonAndMinPts : MonoBehaviour {
    public Text textMeshEpsilon;
    public Text textMeshMinPts;
    public GameObject dbscan;
    private float epsilon;
    private float minPts;
    // Use this for initialization
    void Start()
    {
        epsilon = dbscan.GetComponent<DBScanAlgorithm>().epsilon;
        minPts = dbscan.GetComponent<DBScanAlgorithm>().minPts;
    }

    // Update is called once per frame
    void Update()
    {
        epsilon = dbscan.GetComponent<DBScanAlgorithm>().epsilon;
        minPts = dbscan.GetComponent<DBScanAlgorithm>().minPts;
        epsilon = (float)System.Math.Round(epsilon, 2);
        textMeshEpsilon.text = "ε: " + epsilon.ToString();
        textMeshMinPts.text = "minPts: " + minPts.ToString();
    }
}
