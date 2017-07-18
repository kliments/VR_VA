using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]

public class TetrahedronMesh : MonoBehaviour {
    private Vector3 center;
    private Vector3 A, B, C, D;
    private float OA, OB, OC, OD, AO1, BO1, A1O1, B1O1;
    MeshRenderer meshRenderer;

    public void Init(float[] data, Vector3 vector)
    {
        meshRenderer = gameObject.GetComponent("MeshRenderer") as MeshRenderer;
        if (meshRenderer == null)
        {
            gameObject.AddComponent<MeshRenderer>();
            meshRenderer = gameObject.GetComponent("MeshRenderer") as MeshRenderer;
        }

        Mesh mesh = ((MeshFilter)GetComponent("MeshFilter")).mesh;
        mesh.Clear();
        mesh.name = "Tetrahedron Mesh";

        //position of tetrahedron
        center.x = data[0];
        center.y = data[1];
        center.z = data[2];
        transform.localPosition = center;

        //calculation of coordinates of D
        D.x = center.x;
        D.y = center.y + center.y;
        D.z = center.z;

        //calculation of coordinates of C
        C.x = center.x;
        C.y = center.y - (Mathf.Sin(19.5f) * 0.5f);
        C.z = center.z + Mathf.Sqrt(Mathf.Pow(0.5f, 2) - Mathf.Pow(Mathf.Sin(19.5f) * 0.5f, 2));

        //calculation of coordinates of B
        OB = data[2]; //distance from cetroid to B with angle of 109.5 degrees
        BO1 = Mathf.Cos(19.5f) * OB; // distance from centroid to B with angle of 90 degrees
        B1O1 = BO1 / 2;
        B.x = center.x + Mathf.Sqrt(Mathf.Pow(BO1,2) - Mathf.Pow(B1O1,2));
        B.y = center.y - (Mathf.Sin(19.5f) * OB);
        B.z = center.z - B1O1;

        //calculation of coordinates of A
        OA = data[0]; // distance from centroid to A with angle of 109.5 degrees
        AO1 = Mathf.Cos(19.5f) * OA; // distance from centroid to A with angle of 90 degrees
        A1O1 = AO1 / 2;
        A.x = center.x - Mathf.Sqrt(Mathf.Pow(AO1, 2) - Mathf.Pow(A1O1, 2));
        A.y = center.y - (Mathf.Sin(19.5f)*OA);
        A.z = center.z - A1O1;

        //vertices
        Vector3[] vertices = new Vector3[4] {A, B, C, D};

        //triangles
        int[] tri = new int[12];
        tri[0] = 0;
        tri[1] = 1;
        tri[2] = 2;

        tri[3] = 0;
        tri[4] = 3;
        tri[5] = 1;

        tri[6] = 1;
        tri[7] = 3;
        tri[8] = 2;

        tri[9] = 0;
        tri[10] = 2;
        tri[11] = 3;

        mesh.vertices = vertices;
        mesh.triangles = tri;
            
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

}
