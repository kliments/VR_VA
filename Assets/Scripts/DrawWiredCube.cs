using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWiredCube : MonoBehaviour {
    
    private Mesh cubeMesh;
    private Vector3[] cubeVertices;
    private LineRenderer lr;
    private Vector3 oldSize, newSize;
    public Vector3[] wireVertices;

    // Use this for initialization
    void Start () {
        oldSize = newSize = gameObject.transform.lossyScale;
        lr = new LineRenderer();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void DrawWire()
    {
        SetPositions();
        if (gameObject.GetComponent<LineRenderer>() == null)
        {
            gameObject.AddComponent<LineRenderer>();
        }
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 16;
        lr.SetPositions(wireVertices);

        lr.startWidth = gameObject.transform.lossyScale.x/5;
        lr.endWidth = gameObject.transform.lossyScale.x/5;
        lr.material.color = Color.black;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }

    public void ResetWire()
    {
        cubeMesh.Clear();
        cubeMesh = GetComponent<MeshFilter>().mesh;
        cubeVertices = cubeMesh.vertices;
        SetPositions();
        oldSize = newSize = gameObject.transform.localScale;
    }

    private void SetPositions()
    {
        cubeMesh = GetComponent<MeshFilter>().mesh;
        cubeVertices = cubeMesh.vertices;
        wireVertices = new Vector3[16];
        wireVertices[0] = cubeVertices[0];
        wireVertices[1] = cubeVertices[1];
        wireVertices[2] = cubeVertices[3];
        wireVertices[3] = cubeVertices[2];
        wireVertices[4] = cubeVertices[0];
        wireVertices[5] = cubeVertices[6];
        wireVertices[6] = cubeVertices[4];
        wireVertices[7] = cubeVertices[2];
        wireVertices[8] = cubeVertices[4];
        wireVertices[9] = cubeVertices[5];
        wireVertices[10] = cubeVertices[3];
        wireVertices[11] = cubeVertices[5];
        wireVertices[12] = cubeVertices[7];
        wireVertices[13] = cubeVertices[1];
        wireVertices[14] = cubeVertices[7];
        wireVertices[15] = cubeVertices[6];
    }
}
