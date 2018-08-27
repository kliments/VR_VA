using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubesVertices : MonoBehaviour {
    public Vector3[] downLeft, downRight, upLeft, upRight;
    public bool isAddedToAList;
    // Use this for initialization
    void Start () {
        isAddedToAList = false;

        downLeft = new Vector3[3];
        downLeft[0] = GetComponent<MeshFilter>().mesh.vertices[5];
        downLeft[1] = GetComponent<MeshFilter>().mesh.vertices[11];
        downLeft[2] = GetComponent<MeshFilter>().mesh.vertices[18];

        downRight = new Vector3[3];
        downRight[0] = GetComponent<MeshFilter>().mesh.vertices[4];
        downRight[1] = GetComponent<MeshFilter>().mesh.vertices[10];
        downRight[2] = GetComponent<MeshFilter>().mesh.vertices[21];

        upLeft = new Vector3[3];
        upLeft[0] = GetComponent<MeshFilter>().mesh.vertices[3];
        upLeft[1] = GetComponent<MeshFilter>().mesh.vertices[9];
        upLeft[2] = GetComponent<MeshFilter>().mesh.vertices[17];

        upRight = new Vector3[3];
        upRight[0] = GetComponent<MeshFilter>().mesh.vertices[2];
        upRight[1] = GetComponent<MeshFilter>().mesh.vertices[8];
        upRight[2] = GetComponent<MeshFilter>().mesh.vertices[22];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeVertices(GameObject leftCube, GameObject rightCube, GameObject downCube, GameObject upCube)
    {
        Vector3[] newVertices = new Vector3[24];
        for(int i=0; i<3; i++)
        {
            Vector3 newPos = leftCube.transform.TransformPoint(leftCube.GetComponent<MeshFilter>().mesh.vertices[4]);
            Vector3 oldPos = transform.TransformPoint(downLeft[i]);
            oldPos.y = newPos.y;
            downLeft[i] = transform.InverseTransformPoint(oldPos);
        }
        for(int i=0; i<3; i++)
        {
            Vector3 newPos = leftCube.transform.TransformPoint(leftCube.GetComponent<MeshFilter>().mesh.vertices[2]);
            Vector3 oldPos = transform.TransformPoint(upLeft[i]);
            oldPos.y = newPos.y;
            upLeft[i] = transform.InverseTransformPoint(oldPos);
        }
        for(int i=0; i<3; i++)
        {
            Vector3 newPos = downCube.transform.TransformPoint(downCube.GetComponent<MeshFilter>().mesh.vertices[2]); //((rightCube.transform.TransformPoint(rightCube.GetComponent<MeshFilter>().mesh.vertices[5]) + downCube.transform.TransformPoint(downCube.GetComponent<MeshFilter>().mesh.vertices[2]) + transform.TransformPoint(downRight[0])) / 3);
            Vector3 oldPos = transform.TransformPoint(downRight[i]);
            oldPos.y = newPos.y;
            downRight[i] = transform.InverseTransformPoint(oldPos);
        }
        for(int i=0; i<3; i++)
        {
            Vector3 newPos = ((rightCube.transform.TransformPoint(rightCube.GetComponent<MeshFilter>().mesh.vertices[3]) + transform.TransformPoint(upRight[i])) / 2); //+ upCube.transform.TransformPoint(upCube.GetComponent<MeshFilter>().mesh.vertices[4]) 
            Vector3 oldPos = transform.TransformPoint(upRight[i]);
            oldPos.y = newPos.y;
            upRight[i] = transform.InverseTransformPoint(oldPos);
        }

        newVertices = GetComponent<MeshFilter>().mesh.vertices;
        newVertices[5] = downLeft[0];
        newVertices[11] = downLeft[1];
        newVertices[18] = downLeft[2];

        newVertices[4] = downRight[0];
        newVertices[10] = downRight[1];
        newVertices[21] = downRight[2];

        newVertices[3] = upLeft[0];
        newVertices[9] = upLeft[1];
        newVertices[17] = upLeft[2];

        newVertices[2] = upRight[0];
        newVertices[8] = upRight[1];
        newVertices[22] = upRight[2];

        GetComponent<MeshFilter>().mesh.vertices = newVertices;
        GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
}
