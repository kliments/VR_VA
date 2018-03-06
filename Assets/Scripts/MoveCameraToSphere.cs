using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraToSphere : MonoBehaviour {
    public GameObject cameraRig, cameraEye;
    private GameObject[] spheres;
    public bool setParent, cameraHasArrived = false;
    public bool calculate = true;
    private float height;
    private Vector3 spherePos, distFromCameraToRig;
	// Use this for initialization
	void Start () {
		if(cameraRig == null)
        {
            cameraRig = GameObject.Find("[CameraRig]");
        }
        if (cameraEye == null)
        {
            cameraEye = GameObject.Find("Camera (eye)");
        }
        spheres = GameObject.FindGameObjectsWithTag("sphere");
    }
	
	// Update is called once per frame
	void Update () {
        spherePos = transform.position;
        if (setParent)
        {
            if(calculate)
            {
                calculate = false;
                CalcDistance();
                RemoveParenthoodFromSpheres();
            }
            cameraRig.transform.parent = gameObject.transform;
            cameraRig.transform.position = Vector3.Lerp(cameraRig.transform.position, spherePos+ distFromCameraToRig, Time.deltaTime * 2f);
            if(Vector3.Distance(cameraRig.transform.position, spherePos + distFromCameraToRig) < 0.025f)
            {
                cameraRig.transform.position = spherePos + distFromCameraToRig;
                setParent = false;
            }
        }
	}

    void CalcDistance()
    {
        distFromCameraToRig = cameraRig.transform.position - cameraEye.transform.position;
    }

    void RemoveParenthoodFromSpheres()
    {
        foreach(GameObject sphere in spheres)
        {
            if(sphere.name != gameObject.name)
            {
                sphere.GetComponent<MoveCameraToSphere>().setParent = false;
            }
        }
    }
}
