using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixXandZPosition : MonoBehaviour {
    Vector3 oldPos, newPos;
    Quaternion rot;
    public bool isTaken;
    // Use this for initialization
    void Start () {
        oldPos = transform.localPosition;
        rot = transform.localRotation;
        isTaken = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(isTaken)
        {
            newPos = transform.position;
            newPos.x = oldPos.x;
            newPos.z = oldPos.z;
            transform.position = newPos;
            transform.rotation = rot;
        }
        else if(!isTaken && transform.parent!= null)
        {
            transform.parent = null;
        }
        if(transform.position.y < 0.001f)
        {
            newPos = transform.position;
            newPos.y = 0.001f;
            transform.position = newPos;
        }
	}
}
