using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DBScanProperties : MonoBehaviour {
    
    public int UNCLASSIFIED = 0;
    public int NOISE = -1;
    public int clusterID;
    public float epsilon;
    public Mesh mesh;
    public bool colorChanged;
    public GameObject dbScanButton;
	// Use this for initialization
	void Start () {
        colorChanged = false;
        dbScanButton = GameObject.Find("DBNextStep");
	}
	
	// Update is called once per frame
	void Update () {
		if(clusterID>0 && !colorChanged)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = dbScanButton.GetComponent<DBScanAlgorithm>().pointsColor[clusterID-1];
            colorChanged = true;
        }
	}
}
