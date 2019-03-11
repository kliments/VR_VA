using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLabelTowardsCamera : MonoBehaviour {
    public Transform camera;
    private Vector3 newRot;
	// Use this for initialization
	void Start () {
        newRot = new Vector3(90,90,0);
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(camera);
        newRot.y = transform.eulerAngles.y;
        transform.eulerAngles = newRot;
	}
}
