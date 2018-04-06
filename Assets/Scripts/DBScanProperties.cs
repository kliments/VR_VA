using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DBScanProperties : MonoBehaviour {
    
    public int UNCLASSIFIED = 0;
    public int NOISE = -1;
    public bool drawMeshAround;
    public int clusterID;
    public float epsilon;
    public Mesh mesh;
    public Material refMat;
    private Material material;
    public GameObject dbScanButton;
    private int layerMask;
    private Vector3[] baseVertices;
    private Vector3 pos;
    private float radiusSphere;
    private float sizeDiamond;
    private Color color;
    // Use this for initialization
    void Start () {
        //starting radius to increase till epsilon
        drawMeshAround = false;
        radiusSphere = 0f;
        sizeDiamond = 0f;
        layerMask = LayerMask.NameToLayer("Environment");
        mesh = new Mesh();
        refMat = new Material(Shader.Find("Transparent/Bumped Diffuse"));
    }
	
	// Update is called once per frame
	void Update () {
		if(drawMeshAround)
        {
            pos = transform.position;
            if(dbScanButton == null)
            {
                dbScanButton = GameObject.Find("DBNextStep");
            }
            if (radiusSphere < epsilon && dbScanButton.GetComponent<DBScanAlgorithm>().euclDist)
            {
                GenerateSphere();
                SetColor();
            }
            else if (sizeDiamond < epsilon && !dbScanButton.GetComponent<DBScanAlgorithm>().euclDist)
            {
                GenerateManhattanDiamond();
                SetColor();
            }
            Graphics.DrawMesh(mesh, pos, Quaternion.identity, refMat, layerMask);

        }
        else if(clusterID == NOISE)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
        }
	}

    private void GenerateSphere()
    {
        mesh.Clear();

        radiusSphere += epsilon/5;
        // Longitude |||
        int nbLong = 24;
        // Latitude ---
        int nbLat = 16;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radiusSphere;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radiusSphere;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radiusSphere;
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }

    private void GenerateManhattanDiamond()
    {
        sizeDiamond += epsilon / 5;
        Vector3 A = new Vector3();
        Vector3 B = new Vector3();
        Vector3 C = new Vector3();
        Vector3 D = new Vector3();
        Vector3 E = new Vector3();
        Vector3 F = new Vector3();
        mesh.Clear();

        #region Vertices
        A.x -= sizeDiamond;
        B.y += sizeDiamond;
        C.x += sizeDiamond;
        D.y -= sizeDiamond;
        E.z -= sizeDiamond;
        F.z += sizeDiamond;
        Vector3[] vertices = new Vector3[6] { A, B, C, D, E, F };
        #endregion
        
        #region Triangles
        int[] triangles = new int[24] {0,1,5,4,1,0,4,0,3,0,5,3,5,1,2,2,1,4,5,2,3,2,4,3};
        #endregion

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void SetColor()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = dbScanButton.GetComponent<DBScanAlgorithm>().pointsColor[clusterID - 1];
        //material.CopyPropertiesFromMaterial(refMat);
        color = gameObject.GetComponent<MeshRenderer>().material.color;
        color.a = 0.05f;
        refMat.color = color;
    }

    public void ResetPoint()
    {
        drawMeshAround = false;
        radiusSphere = 0f;
        sizeDiamond = 0f;
        clusterID = UNCLASSIFIED;
        mesh.Clear();
        gameObject.GetComponent<MeshRenderer>().material.color = gameObject.GetComponent<PreviousStepProperties>().originalColor;
    }
}
