using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetToGround : MonoBehaviour {
    public GameObject cameraRig;
    private Vector3 rigPosition;
    public bool rigPosReset = false;
	// Use this for initialization
	void Start () {
        rigPosition = cameraRig.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("bla");
		if(rigPosReset)
        {
            RemoveParenthoodFromRig();
            cameraRig.transform.position = Vector3.Lerp(cameraRig.transform.position, rigPosition, Time.deltaTime * 1f);
            if (Vector3.Distance(cameraRig.transform.position,rigPosition) < 0.05f)
            {
                cameraRig.transform.position = rigPosition;
                rigPosReset = false;
            }
        }
	}

    public void RemoveParenthoodFromRig()
    {
        if (cameraRig.transform.parent != null)
        {
            cameraRig.transform.parent.gameObject.GetComponent<MoveCameraToSphere>().setParent = false;
            cameraRig.transform.parent = null;
        }
    }
}
