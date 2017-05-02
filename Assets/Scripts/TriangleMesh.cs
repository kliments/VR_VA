using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]

public class TriangleMesh : MonoBehaviour {
    
	private float x;
	private float y;
	private float z;
    MeshRenderer mMeshRenderer;

    // Use this for initialization
    public void Init (float[] data, Vector3 vector) {
        mMeshRenderer = gameObject.GetComponent("MeshRenderer") as MeshRenderer;
        if (mMeshRenderer == null)
        {
            gameObject.AddComponent<MeshRenderer>();
            mMeshRenderer = gameObject.GetComponent("MeshRenderer") as MeshRenderer;
        }


        Mesh mesh = ((MeshFilter)GetComponent("MeshFilter")).mesh;
        mesh.Clear();
        mesh.name = "Triangle Mesh";
        transform.localPosition = new Vector3(vector.x, vector.y, vector.z);
        transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
        x = data[0];
        y = data[1];
        z = data[2];

		//vertices
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(x,y,z), new Vector3( x - (x/2 * Mathf.Sqrt(3)),y - x/2 ,z), new Vector3(x,y+y,z), new Vector3(x + (z/2 * Mathf.Sqrt(3)),y - z/2 ,z)
		};

		//triangles
		int[] tri = new int[9];
		tri[0] = 0;
		tri[1] = 2;
		tri[2] = 1;

		tri[3] = 0;
		tri[4] = 3;
		tri[5] = 2;

		tri[6] = 0;
		tri[7] = 1;
		tri[8] = 3;

		//normals
		Vector3[] normals = new Vector3[4];
		normals[0] = Vector3.up;
		normals[1] = Vector3.up;
		normals[2] = Vector3.up;
		normals[3] = Vector3.up;

		//UVs
		/*Vector2[] uv = new Vector2[4];
		uv[0] = new Vector2(0.5f,0.5f);
		uv[1] = new Vector2(0,0);
		uv[2] = new Vector2(1,1f);
		uv[3] = new Vector2(1,1);*/

		//Assign arrays
		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.normals = normals;
		//mesh.uv = uv;
    }
}
