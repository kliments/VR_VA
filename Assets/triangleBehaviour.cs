using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triangleBehaviour : MonoBehaviour
{
    public Camera myCamera;
    private Vector3 target;
    public GameObject plane;
    public GameObject trackObj;
    private RaycastHit ray;
    public Color color;
    public List<Color> colorList;
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
        target = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y, myCamera.transform.position.z);
        transform.LookAt(target);

        if (Physics.Raycast(gameObject.transform.position, trackObj.transform.forward, out ray, Mathf.Infinity, layerMask))
        {
            Graphics.DrawMesh(pointMesh.gameObject.GetComponent<MeshFilter>().sharedMesh, ray.point, Quaternion.identity, gameObject.GetComponent<MeshRenderer>().material, gameObject.layer);
        }
    }
    
}
