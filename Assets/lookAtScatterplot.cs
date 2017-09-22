using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtScatterplot : MonoBehaviour {
    private Vector3 target;
    public GameObject targetObj;
	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update ()
    {
        /*target = new Vector3(targetObj.transform.position.x, targetObj.transform.position.y, targetObj.transform.position.z);
        
		transform.LookAt(target);*/
        transform.forward = targetObj.transform.up * (-1);
    }
}
