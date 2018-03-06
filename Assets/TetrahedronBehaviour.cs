using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronBehaviour : MonoBehaviour
{
    public GameObject trackObj;
    private RaycastHit ray;
    public Color color;
    private int layerMask;
    public GameObject pointMesh;


    // Use this for initialization
    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Environment");
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(gameObject.transform.position, trackObj.transform.forward, out ray, Mathf.Infinity, layerMask))
        {
            Graphics.DrawMesh(pointMesh.gameObject.GetComponent<MeshFilter>().sharedMesh, ray.point, Quaternion.identity, gameObject.GetComponent<MeshRenderer>().material, gameObject.layer);
        }
    }
    
}
