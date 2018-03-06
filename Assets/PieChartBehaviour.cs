using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieChartBehaviour : MonoBehaviour
{
    public Camera myCamera;
    private Vector3 target;
    public GameObject trackObj;
    private RaycastHit ray;
    public Color color;
    private int layerMask;
    public GameObject pointMesh;
    private Material material;
    private Mesh mesh;
    // Use this for initialization
    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Environment");
        material = gameObject.GetComponent<MeshRenderer>().material;
        material.color = color;
        mesh = pointMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        target = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y, myCamera.transform.position.z);
        transform.LookAt(target);
        
        if (Physics.Raycast(gameObject.transform.position, trackObj.transform.forward, out ray, Mathf.Infinity, layerMask))
        {
            Graphics.DrawMesh(mesh, ray.point, Quaternion.identity, material, gameObject.layer);
        }
    }
    
}
