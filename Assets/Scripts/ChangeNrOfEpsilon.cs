using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeNrOfEpsilon : MonoBehaviour {
    private TextMesh textMesh;
    public GameObject dbscan;
    private float epsilon;
    // Use this for initialization
    void Start()
    {
        textMesh = GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        epsilon = dbscan.GetComponent<DBScanAlgorithm>().epsilon;
        textMesh.text = "ε: " + epsilon.ToString();
    }
}
