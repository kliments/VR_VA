using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledmapGeneration : MonoBehaviour {
    public float[][] mapTilesInfluence;
    public Vector3[] positions;
    public GaussianCoefficients gaussCoef;
    public Material mat;

    //plus/minus neighbourhood cubes around the center cube in the matrix
    public int halfLengthOfNeighbourhood;

    private GameObject obj;
    private Mesh mesh;
    private int counter;
    private List<GameObject> list;
    private Vector3[][][] tiledMapVertices;
    private Vector3[][] mapPositions, verticesMatrix;
    private Vector3[] vertices;
    private int[][] trianglesMatrix;
    private int[] triangles;
    private float x, y, z;
    // Use this for initialization
    void Start () {
        x = -0.25f;
        y = 0;
        z = -0.25f;
        halfLengthOfNeighbourhood = 3;
        counter = 0;
        obj = new GameObject();

        list = new List<GameObject>();
        //set initial abstract positions, to refer to float influence
        mapPositions = new Vector3[200][];
        for(int i=0; i<mapPositions.Length; i++)
        {
            mapPositions[i] = new Vector3[200];
            for(int j=0; j<mapPositions[i].Length; j++)
            {
                mapPositions[i][j] = new Vector3(x, y, z);
                z += 0.0075f;
            }
            x += 0.0075f;
            z = -0.25f;
        }

        //influence depending on how many neighbouring "tiles" will be affected
        mapTilesInfluence = new float[200][];
        for(int m=0; m<mapTilesInfluence.Length; m++)
        {
            mapTilesInfluence[m] = new float[200];
            for(int l=0; l<mapTilesInfluence[m].Length; l++)
            {
                mapTilesInfluence[m][l] = 0;
            }
        }

        tiledMapVertices = new Vector3[200][][];

        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];
        /*if (data.positions.Length == 0)
        {
            Invoke("IncreaseInfluence", 0.1f);
        }*/
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetMouseButtonDown(0))
        {
            ResetMe();
            IncreaseInfluence();
        }
		
	}
    
    //increasing influence if "tiles" belong in neighbourhood
    void IncreaseInfluence()
    {
        for (int m = 0; m < mapTilesInfluence.Length; m++)
        {
            for (int l = 0; l < mapTilesInfluence[m].Length; l++)
            {
                for (int d = 0; d < positions.Length; d++)
                {
                    if (ChebyshevDistance(positions[d], mapPositions[m][l])<= 0.0075f/2)
                    {
                        int gaussX = 0;
                        int gaussZ = 0;
                        for(int xPos = m-halfLengthOfNeighbourhood; xPos<=m+ halfLengthOfNeighbourhood; xPos++)
                        {
                            for(int zPos = l- halfLengthOfNeighbourhood; zPos<=l+ halfLengthOfNeighbourhood; zPos++)
                            {
                                //calculate gaussian coefficients only one time
                                if(!gaussCoef.valuesCalculated)
                                {
                                    gaussCoef.valuesCalculated = true;                                    
                                    for(int gX = 0; gX<gaussCoef.matrixRowLength; gX++)
                                    {
                                        gaussCoef.gaussianPositionMatrix[gX] = new Vector3[gaussCoef.matrixRowLength];
                                        for(int gY=0; gY<gaussCoef.matrixRowLength; gY++)
                                        {
                                            gaussCoef.gaussianPositionMatrix[gX][gY] = mapPositions[xPos + gX][zPos + gY];
                                        }
                                    }
                                    gaussCoef.GaussianCoefCalculator();
                                }
                                mapTilesInfluence[xPos][zPos] += gaussCoef.gaussianCoef[gaussX][gaussZ];
                                gaussZ++;
                            }
                            gaussZ = 0;
                            gaussX++;
                        }
                    }
                }
            }
        }
        CountTiles();
        CreateVerticesAndTriangles();
    }

    //counts the total number of "tiles"
    public void CountTiles()
    {
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                if (mapTilesInfluence[x][z] > 0)
                {
                    counter++;
                }
            }
        }
    }

    //calculates Chebyshev Distance to find neighbourhood tiles
    public static float ChebyshevDistance(Vector3 a, Vector3 b)
    {
        var dx = Mathf.Abs(a.x - b.x);
        var dy = Mathf.Abs(a.z - b.z);
        return (dx + dy) - Mathf.Min(dx, dy);
    }

    //creates 4 vertices and 2 triangles on each tile
    private void CreateVerticesAndTriangles()
    {
        verticesMatrix = new Vector3[counter][];
        trianglesMatrix = new int[counter][];
        int currentTile = 0;
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            tiledMapVertices[x] = new Vector3[200][];
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                tiledMapVertices[x][z] = new Vector3[4];
                if (mapTilesInfluence[x][z] > 0)
                {
                    verticesMatrix[currentTile] = new Vector3[4];
                    //bottom left vertex
                    verticesMatrix[currentTile][0].x = mapPositions[x][z].x - 0.00375f;
                    verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z]/50;
                    verticesMatrix[currentTile][0].z = mapPositions[x][z].z - 0.00375f;
                    tiledMapVertices[x][z][0] = verticesMatrix[currentTile][0];
                    //bottom right vertex
                    verticesMatrix[currentTile][1].x = mapPositions[x][z].x + 0.00375f;
                    verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 50;
                    verticesMatrix[currentTile][1].z = mapPositions[x][z].z - 0.00375f;
                    tiledMapVertices[x][z][1] = verticesMatrix[currentTile][1];
                    //top left vertex
                    verticesMatrix[currentTile][2].x = mapPositions[x][z].x - 0.00375f;
                    verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 50;
                    verticesMatrix[currentTile][2].z = mapPositions[x][z].z + 0.00375f;
                    tiledMapVertices[x][z][2] = verticesMatrix[currentTile][2];
                    //top right vertex
                    verticesMatrix[currentTile][3].x = mapPositions[x][z].x + 0.00375f;
                    verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 50;
                    verticesMatrix[currentTile][3].z = mapPositions[x][z].z + 0.00375f;
                    tiledMapVertices[x][z][3] = verticesMatrix[currentTile][3];

                    int count = (currentTile + 1) * 4;
                    trianglesMatrix[currentTile] = new int[6];
                    trianglesMatrix[currentTile][0] = count - 4;
                    trianglesMatrix[currentTile][1] = count - 2;
                    trianglesMatrix[currentTile][2] = count - 3;
                    trianglesMatrix[currentTile][3] = count - 2;
                    trianglesMatrix[currentTile][4] = count - 1;
                    trianglesMatrix[currentTile][5] = count - 3; 
                    currentTile++;
                }
            }
        }
        currentTile = 0;
        for(int i=0; i<mapTilesInfluence.Length; i++)
        {
            for(int j=0; j<mapTilesInfluence[i].Length; j++)
            {
                if(mapTilesInfluence[i][j]>0)
                {

                    verticesMatrix[currentTile] = ChangeVertices(verticesMatrix[currentTile], tiledMapVertices[i][j+1][0].y,tiledMapVertices[i][j-1][3].y, tiledMapVertices[i+1][j+1][0].y,
                                                                 tiledMapVertices[i+1][j][0].y, tiledMapVertices[i-1][j-1][3].y, tiledMapVertices[i-1][j][3].y);
                    tiledMapVertices[i][j] = verticesMatrix[currentTile];
                    currentTile++;
                }
            }
        }
        ConvertMatrixToArray();
    }

    //converts the matrix of vertices to an array
    private void ConvertMatrixToArray()
    {
        int nextVertex = 0;
        int nextTriangle = 0;
        vertices = new Vector3[counter * 4];
        triangles = new int[counter * 6];
        for(int i=0; i<counter; i++)
        {
            for(int j=0; j<4; j++)
            {
                vertices[nextVertex] = verticesMatrix[i][j];
                nextVertex++;
            }
            for(int k=0; k<6; k++)
            {
                triangles[nextTriangle] = trianglesMatrix[i][k];
                nextTriangle++;
            }
        }
        CreateMesh();
    }

    //creates mesh
    private void CreateMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshRenderer>().material = mat;
        obj.transform.localPosition = new Vector3(0.819f, 0, 0.751f);
        obj.transform.localRotation = new Quaternion(0, 180, 0,0);
        obj.transform.localScale = new Vector3(0.6600493f, 0.6600493f, 0.6600493f);
    }    

    private Vector3[] ChangeVertices(Vector3[] actualVertex, float top,float down, float topRight, float right, float downLeft, float left)
    {
        Vector3[] vertex = new Vector3[4];
        if(actualVertex[0].y > 0)
            {
                actualVertex[0].y = downLeft;
                actualVertex[1].y = down;
                actualVertex[2].y = left;
                actualVertex[3].y = (top + topRight + right) / 3;
                vertex = actualVertex;
            }
        return vertex;
    }

    public void ResetMe()
    {
        //influence depending on how many neighbouring "tiles" will be affected
        mapTilesInfluence = new float[200][];
        for (int m = 0; m < mapTilesInfluence.Length; m++)
        {
            mapTilesInfluence[m] = new float[200];
            for (int l = 0; l < mapTilesInfluence[m].Length; l++)
            {
                mapTilesInfluence[m][l] = 0;
            }
        }
        counter = 0;
        vertices = null;
        triangles = null;
        
        tiledMapVertices = new Vector3[200][][];
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];
        gaussCoef.valuesCalculated = false;
    }
}
