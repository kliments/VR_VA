using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledTetrahedronVariables : MonoBehaviour {

    public GameObject scatterplot;
    private Vector3 target;
    public GameObject plane;
    public GameObject trackObj;
    private bool planeHasChanged;
    private Vector3[] linePositions;
    private Vector3 fwd;
    private RaycastHit ray;
    private Vector3[] noLines = new Vector3[2];
    private int index = 0;

    private List<GameObject> listGameobjects;

    public Quaternion oldRot;
    public Quaternion newRot;
    public Vector3 oldPos;
    public Vector3 newPos;
    public Vector3 oldScale;
    public Vector3 newScale;
    public bool tiledTetrahedronHasChanged = true;
    private LineRenderer lr;

    // Use this for initialization
    void Start()
    {

        oldPos = transform.position;
        newPos = transform.position;
        oldRot = transform.rotation;
        newRot = transform.rotation;
        oldScale = transform.localScale;
        newScale = transform.localScale;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        // index = scatterplot.GetComponent<DataSpaceHandler>().ind;
        int j = 0;
        for (int i = 0; i < vertices.Length; i += 4)
        {
            j++;
            GameObject child = new GameObject("child" + j);
            child.transform.position = transform.TransformPoint(vertices[i]);
            child.transform.parent = transform;
            child.AddComponent<LineRenderer>();
            lr = child.GetComponent<LineRenderer>();
            lr.material.color = scatterplot.GetComponent<Tetrahedron>().listOfColors[index];
            //index = scatterplot.GetComponent<DataSpaceHandler>().ind++;
            index++;
            shootRaycast(child, child.transform.position);
        }

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
        planeHasChanged = plane.GetComponent<drawTriangles>().hasChanged;

        if (oldPos != newPos || oldRot != newRot || oldScale != newScale)
        {
            tiledTetrahedronHasChanged = true;
        }
        else
        {
            tiledTetrahedronHasChanged = false;
        }

        if (tiledTetrahedronHasChanged || planeHasChanged)
        {
            foreach (Transform child in transform)
            {
                shootRaycast(child.gameObject, child.position);
            }
        }
    }


    RaycastHit shootRaycast(GameObject obj, Vector3 dataPoint)
    {
        linePositions = new Vector3[2];
        fwd = trackObj.transform.forward;
        int layerMask = 1 << LayerMask.NameToLayer("Environment");


        if (Physics.Raycast(dataPoint, fwd, out ray, Mathf.Infinity, layerMask))
        {
            linePositions[0] = new Vector3(ray.point.x, ray.point.y - 0.002f, ray.point.z);
            linePositions[1] = new Vector3(ray.point.x, ray.point.y + 0.002f, ray.point.z);
            obj.GetComponent<LineRenderer>().SetPositions(linePositions);
            obj.GetComponent<LineRenderer>().startWidth = 0.007f;
            obj.GetComponent<LineRenderer>().endWidth = 0.007f;

            Debug.DrawRay(dataPoint, fwd * 5f, Color.green, 1);
        }
        else
        {
            obj.GetComponent<LineRenderer>().SetPositions(noLines);
        }

        return ray;
    }
}
