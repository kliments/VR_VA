using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour {
    private Camera cam;
	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(cam.transform);
        transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, 0);
	}
}
