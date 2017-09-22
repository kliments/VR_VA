using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triangleBehaviour : MonoBehaviour
{
    public GameObject child;
    public Camera myCamera;
    private Vector3 target;
    public GameObject plane;
    public GameObject trackObj;
    private bool planeHasChanged;
    private Vector3[] linePositions;
    private Vector3 fwd;
    private RaycastHit ray;
    private Vector3[] noLines = new Vector3[2];
    public Color color;
    private Color bla;

    private Quaternion oldRot;
    private Quaternion newRot;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 oldScale;
    private Vector3 newScale;
    private bool pointHasChanged;

    // Use this for initialization
    void Start()
    {
        RaycastHit ray1 = shootRaycast(transform.position);
        noLines[0] = new Vector3(0f,0f,0f);
        noLines[1] = new Vector3(0f, 0f, 0f);


        oldPos = transform.position;
        newPos = transform.position;
        oldRot = transform.rotation;
        newRot = transform.rotation;
        oldScale = transform.localScale;
        newScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        oldPos = newPos;
        newPos = transform.position;

        oldRot = newRot;
        newRot = transform.rotation;

        oldScale = newScale;
        newScale = transform.localScale;

        if (oldPos != newPos || oldRot != newRot || oldScale != newScale)
        {
            pointHasChanged = true;
        }
        else
        {
            pointHasChanged = false;
        }
        
        planeHasChanged = plane.GetComponent<drawTriangles>().hasChanged;
        target = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y, myCamera.transform.position.z);
        transform.LookAt(target);
        if (pointHasChanged || planeHasChanged)
        {
            shootRaycast(transform.position);
        }
    }

    RaycastHit shootRaycast(Vector3 dataPoint)
    {
        linePositions = new Vector3[2];
        fwd = trackObj.transform.forward;
        int layerMask = 1 << LayerMask.NameToLayer("Environment");


        if (Physics.Raycast(dataPoint, fwd, out ray, Mathf.Infinity, layerMask))
        {
            linePositions[0] = new Vector3(ray.point.x, ray.point.y - 0.002f, ray.point.z);
            linePositions[1] = new Vector3(ray.point.x, ray.point.y + 0.002f, ray.point.z);
            child.GetComponent<LineRenderer>().SetPositions(linePositions);
            child.GetComponent<LineRenderer>().startWidth = 0.007f;
            child.GetComponent<LineRenderer>().endWidth = 0.007f;

            child.GetComponent<LineRenderer>().material.color = color;
            Debug.DrawRay(dataPoint, fwd * 5f, Color.green, 1);
        }
        else
        {
            child.GetComponent<LineRenderer>().SetPositions(noLines);
        }

        return ray;
    }
}
