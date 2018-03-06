using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawTriangles : MonoBehaviour {
    //if position of the plane has changed
    public bool hasChanged = false;
    public Quaternion oldValueRot;
    public Quaternion nowValueRot;
    public Vector3 oldValuePos;
    public Vector3 nowValuePos;
    private Vector3[] points;
    public Transform parentColider;
    public List<GameObject> collidingObjects;
	// Use this for initialization
	void Start () {
		foreach(Transform child in parentColider)
        {
            collidingObjects.Add(child.gameObject);
        }

        oldValuePos = transform.position;
        nowValuePos = transform.position;
        oldValueRot = transform.rotation;
        nowValueRot = transform.rotation;

	}
	
	// Update is called once per frame
	void Update () {
        oldValuePos = nowValuePos;
        oldValueRot = nowValueRot;
        nowValuePos = transform.position;
        nowValueRot = transform.rotation;

        if (oldValuePos != nowValuePos || oldValueRot != nowValueRot)
        {
            hasChanged = true;
            Debug.Log("Position or Rotation has Changed");
        }
        else
        {
            hasChanged = false;
        }
	}
}
