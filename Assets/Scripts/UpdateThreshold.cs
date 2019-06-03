using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateThreshold : MonoBehaviour {
    public GameObject activateController;
    public Transform threshold;
	// Use this for initialization
	void Start () {
        activateController = GameObject.Find("[CameraRig]").transform.GetChild(2).gameObject;
        threshold = GameObject.Find("ThresholdPlane").transform;
	}
	
	// Update is called once per frame
	void Update () {
    }
}
