using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledmapGeneration : MonoBehaviour {
    public float[][] mapTilesInfluence;
    public Vector3[] positions;
    public GaussianCoefficients gaussCoef;
    public Material mat;
    public bool gaussianCalculation;
    //plus/minus neighbourhood cubes around the center cube in the matrix
    public int halfLengthOfNeighbourhood;
    public float threshold;
    public GameObject thresholdPlane;
    public List<Vector3> peaks;
    public bool resizeMesh;
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbscan;

    private GameObject obj;
    private Mesh mesh;
    private int counter;
    private List<GameObject> list;
    private Vector3[][][] tiledMapVertices;
    private Vector3[][] mapPositions, verticesMatrix, verticesMaximumMatrix;
    private Vector3[] vertices;
    private int[][] trianglesMatrix, countersMatrix;
    private int[] triangles;
    private float x, y, z;
    private Vector3 startSize, finishSize;
    private Color[][][] tiledMapColors;
    private Color[][] matrixColors;
    private Color[] colors;
    private bool[][] isPeak;
    // Use this for initialization
    void Start () {
        gaussianCalculation = false;
        x = -0.25f;
        y = 0;
        z = -0.25f;
        halfLengthOfNeighbourhood = 3;
        counter = 0;
        threshold = 0.000001f;
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
        tiledMapColors = new Color[200][][];
        peaks = new List<Vector3>();
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];

        startSize = new Vector3(0.66f, 0.001f, 0.66f);
        finishSize = new Vector3(0.66f, 0.66f, 0.66f);
	}
	
	// Update is called once per frame
	void Update () {
        if(resizeMesh)
        {
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, finishSize, Time.deltaTime*1);
        }
		
	}

    public void StartDenclue()
    {
        kMeans.ResetMe();
        dbscan.ResetMe();
        ResetMe();
        IncreaseInfluence();
        obj.transform.localScale = startSize;
        resizeMesh = true;
    }
    
    //increasing influence if "tiles" belong in neighbourhood
    void IncreaseInfluence()
    {
        for (int d = 0; d < positions.Length; d++)
        {
            if (positions[d].x < 0.5f && positions[d].z < 0.5f)
            {
                for (int m = 0; m < 100; m++)
                {
                    for (int l = 0; l < 100; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else if (positions[d].x < 0.5f && positions[d].z >= 0.5f)
            {
                for (int m = 0; m < 100; m++)
                {
                    for (int l = 100; l < 200; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else if (positions[d].x >= 0.5f && positions[d].z < 0.5f)
            {
                for (int m = 100; m < 200; m++)
                {
                    for (int l = 0; l < 100; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else
            {
                for (int m = 100; m < 200; m++)
                {
                    for (int l = 100; l < 200; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
        }
        CountTiles();
        CreateVerticesAndTriangles();
    }

    private void FeedMapTilesInfluence(Vector3 position, int m, int l)
    {
        if (ChebyshevDistance(position, mapPositions[m][l]) <= 0.0075f / 2)
        {
            int gaussX = 0;
            int gaussZ = 0;
            for (int xPos = m - halfLengthOfNeighbourhood; xPos <= m + halfLengthOfNeighbourhood; xPos++)
            {
                for (int zPos = l - halfLengthOfNeighbourhood; zPos <= l + halfLengthOfNeighbourhood; zPos++)
                {
                    //calculate gaussian coefficients only one time
                    if (!gaussCoef.valuesCalculated)
                    {
                        gaussCoef.valuesCalculated = true;
                        for (int gX = 0; gX < gaussCoef.matrixRowLength; gX++)
                        {
                            gaussCoef.gaussianPositionMatrix[gX] = new Vector3[gaussCoef.matrixRowLength];
                            for (int gY = 0; gY < gaussCoef.matrixRowLength; gY++)
                            {
                                gaussCoef.gaussianPositionMatrix[gX][gY] = mapPositions[xPos + gX][zPos + gY];
                            }
                        }
                        gaussCoef.GaussianCoefCalculator();
                    }
                    if (gaussianCalculation)
                    {
                        mapTilesInfluence[xPos][zPos] += gaussCoef.gaussianCoef[gaussX][gaussZ];
                    }
                    else
                    {
                        mapTilesInfluence[xPos][zPos]++;
                    }
                    gaussZ++;
                }
                gaussZ = 0;
                gaussX++;
            }
        }
    }

    //counts the total number of "tiles"
    public void CountTiles()
    {
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                if ((mapTilesInfluence[x][z] > 0 || HasNeighbours(x,z)) && !gaussianCalculation)
                {
                    counter++;
                }
                else if(mapTilesInfluence[x][z]>0 && gaussianCalculation)
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
        countersMatrix = new int[200][];
        isPeak = new bool[200][];
		matrixColors = new Color[counter][];
        verticesMaximumMatrix = new Vector3[200][];
        int currentTile = 0;
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            tiledMapVertices[x] = new Vector3[200][];
            countersMatrix[x] = new int[200];
            verticesMaximumMatrix[x] = new Vector3[200];
            tiledMapColors[x] = new Color[200][];
            isPeak[x] = new bool[200];
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                tiledMapVertices[x][z] = new Vector3[4];
                tiledMapColors[x][z] = new Color[4];
                if(!gaussianCalculation)
                { 
                    //first check for border tiles, to create fictional vertices to access later for creating the walls of the mesh
                    if (mapTilesInfluence[x][z] == 0 && HasNeighbours(x, z))
                    {
                        verticesMatrix[currentTile] = new Vector3[4];
						matrixColors[currentTile] = new Color[4];
                        countersMatrix[x][z] = currentTile;
                        int count = (currentTile + 1) * 4;
                        //bottom left vertex
                        verticesMatrix[currentTile][0].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][0].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][0] = verticesMatrix[currentTile][0];
                        //bottom right vertex
                        verticesMatrix[currentTile][1].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][1].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][1] = verticesMatrix[currentTile][1];
                        //top left vertex
                        verticesMatrix[currentTile][2].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][2].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][2] = verticesMatrix[currentTile][2];
                        //top right vertex
                        verticesMatrix[currentTile][3].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][3].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][3] = verticesMatrix[currentTile][3];

                        trianglesMatrix[currentTile] = new int[18];
                        for (int t = 0; t < 18; t++)
                        {
                            trianglesMatrix[currentTile][t] = count - 4;
                        }
                        // if there is an neighbour down of current tile, create a square that represents wall to the ground
                        if (HasNeighbourDown(x, z))
                        {
                            trianglesMatrix[currentTile][0] = count - 4;                                // vertex 0
                            trianglesMatrix[currentTile][1] = count - 3;                                // 1
                            trianglesMatrix[currentTile][2] = (countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            trianglesMatrix[currentTile][3] = (countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            trianglesMatrix[currentTile][4] = count - 3;                                // 1
                            trianglesMatrix[currentTile][5] = (countersMatrix[x][z - 1] + 1) * 4 - 1;   // 3 from tile down
                        }
                        if (HasNeighbourOnTheLeft(x, z))
                        {
                            trianglesMatrix[currentTile][6] = count - 2;                               // 2
                            trianglesMatrix[currentTile][7] = count - 4;                               // 0
                            trianglesMatrix[currentTile][8] = (countersMatrix[x - 1][z] + 1) * 4 - 1;  // vertex 3 from tile on the left
                            trianglesMatrix[currentTile][9] = count - 4;                               // 0
                            trianglesMatrix[currentTile][10] = (countersMatrix[x - 1][z] + 1) * 4 - 3; // 1 from tile on the left
                            trianglesMatrix[currentTile][11] = (countersMatrix[x - 1][z] + 1) * 4 - 1; // 3 from tile on the left
                        }
						
						//set the color of the current tile to white
						for (int c = 0; c < 4; c++)
						{
							matrixColors[currentTile][c] = new Color(0, 0, Math.Abs(1-verticesMatrix[currentTile][c].y));
                            tiledMapColors[x][z][c] = new Color(0, 0, Math.Abs(1 - verticesMatrix[currentTile][c].y));
                        }
                        verticesMaximumMatrix[x][z] = tiledMapVertices[x][z][0];
                        currentTile++;
                    }
                    else if (mapTilesInfluence[x][z] > 0)
                    {
                        verticesMatrix[currentTile] = new Vector3[4];
						matrixColors[currentTile] = new Color[4];

                        //bottom left vertex
                        verticesMatrix[currentTile][0].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][0].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][0] = verticesMatrix[currentTile][0];
                        //bottom right vertex
                        verticesMatrix[currentTile][1].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][1].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][1] = verticesMatrix[currentTile][1];
                        //top left vertex
                        verticesMatrix[currentTile][2].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][2].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][2] = verticesMatrix[currentTile][2];
                        //top right vertex
                        verticesMatrix[currentTile][3].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][3].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][3] = verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        countersMatrix[x][z] = currentTile;

                        int count = (currentTile + 1) * 4;
                        //need to create 5 extra rectangles around a tile, to look like a cube
                        //center rectangle
                        trianglesMatrix[currentTile] = new int[18];
                        trianglesMatrix[currentTile][0] = count - 4; // vertex 0
                        trianglesMatrix[currentTile][1] = count - 2; // 2
                        trianglesMatrix[currentTile][2] = count - 3; // 1
                        trianglesMatrix[currentTile][3] = count - 2; // 2
                        trianglesMatrix[currentTile][4] = count - 1; // 3
                        trianglesMatrix[currentTile][5] = count - 3; // 1
                                                                     //left rectangle
                        trianglesMatrix[currentTile][6] = count - 2; // 2
                        trianglesMatrix[currentTile][7] = count - 4; // 0
                        trianglesMatrix[currentTile][8] = (countersMatrix[x - 1][z] + 1) * 4 - 1; // vertex 3 from tile on the left
                        trianglesMatrix[currentTile][9] = count - 4;                      // 0
                        trianglesMatrix[currentTile][10] = (countersMatrix[x - 1][z] + 1) * 4 - 3; // 1 from tile on the left
                        trianglesMatrix[currentTile][11] = (countersMatrix[x - 1][z] + 1) * 4 - 1; // 3 from tile on the left
                                                                                                   //down rectangle
                        trianglesMatrix[currentTile][12] = count - 4;                      // vertex 0
                        trianglesMatrix[currentTile][13] = count - 3;                      // 1
                        trianglesMatrix[currentTile][14] = (countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        trianglesMatrix[currentTile][15] = (countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        trianglesMatrix[currentTile][16] = count - 3;                      // 1
                        trianglesMatrix[currentTile][17] = (countersMatrix[x][z - 1] + 1) * 4 - 1;   // 3 from tile down
						
						for (int c = 0; c < 4; c++)
						{
							matrixColors[currentTile][c] = new Color(verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - verticesMatrix[currentTile][c].y));
                            tiledMapColors[x][z][c] = new Color(verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - verticesMatrix[currentTile][c].y));
                        }
                        verticesMaximumMatrix[x][z] = tiledMapVertices[x][z][0];
                        currentTile++;
                    }
                }
                else
                {
                    if (mapTilesInfluence[x][z] > 0)
                    { 
                        verticesMatrix[currentTile] = new Vector3[4];
						matrixColors[currentTile] = new Color[4];
                        //bottom left vertex
                        verticesMatrix[currentTile][0].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][0].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][0] = verticesMatrix[currentTile][0];
                        //bottom right vertex
                        verticesMatrix[currentTile][1].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][1].z = mapPositions[x][z].z - 0.00375f;
                        tiledMapVertices[x][z][1] = verticesMatrix[currentTile][1];
                        //top left vertex
                        verticesMatrix[currentTile][2].x = mapPositions[x][z].x - 0.00375f;
                        verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][2].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][2] = verticesMatrix[currentTile][2];
                        //top right vertex
                        verticesMatrix[currentTile][3].x = mapPositions[x][z].x + 0.00375f;
                        verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        verticesMatrix[currentTile][3].z = mapPositions[x][z].z + 0.00375f;
                        tiledMapVertices[x][z][3] = verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        countersMatrix[x][z] = currentTile;
                        //center rectangle
                        int count = (currentTile + 1) * 4;
                        trianglesMatrix[currentTile] = new int[6];
                        trianglesMatrix[currentTile][0] = count - 4;
                        trianglesMatrix[currentTile][1] = count - 2;
                        trianglesMatrix[currentTile][2] = count - 3;
                        trianglesMatrix[currentTile][3] = count - 2;
                        trianglesMatrix[currentTile][4] = count - 1;
                        trianglesMatrix[currentTile][5] = count - 3;
						
						for (int c = 0; c < 4; c++)
						{
							matrixColors[currentTile][c] = new Color(verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - verticesMatrix[currentTile][c].y));
                            tiledMapColors[x][z][c] = new Color(verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - verticesMatrix[currentTile][c].y));
                        }
                        currentTile++;
                    }
                }
            }
        }
        //need to start from begining, to shift the top right vertex to the bottom left vertex of the top right tile
        currentTile = 0;
        if(gaussianCalculation)
        {
            for (int i = 0; i < mapTilesInfluence.Length; i++)
            {
                for (int j = 0; j < mapTilesInfluence[i].Length; j++)
                {
                    if (mapTilesInfluence[i][j] > 0)
                    {

                        verticesMatrix[currentTile] = ChangeVertices(verticesMatrix[currentTile], tiledMapVertices[i][j + 1][0].y, tiledMapVertices[i][j - 1][3].y, tiledMapVertices[i + 1][j + 1][0].y,
                                                                     tiledMapVertices[i + 1][j][0].y, tiledMapVertices[i - 1][j - 1][3].y, tiledMapVertices[i - 1][j][3].y);
                        tiledMapVertices[i][j] = verticesMatrix[currentTile];
                        verticesMaximumMatrix[i][j] = tiledMapVertices[i][j][3];
                        currentTile++;
                    }
                }
            }
        }

        ConvertMatrixToArray();
    }

    //converts the matrix of vertices to an array
    private void ConvertMatrixToArray()
    {
        ReturnPeaks();
        int nextVertex = 0;
        int nextTriangle = 0;
        vertices = new Vector3[counter * 4];
		colors = new Color[counter * 4];
        if(!gaussianCalculation)
        {
            triangles = new int[counter * 18];
        }
        else
        {
            triangles = new int[counter * 6];
        }
        for(int i=0; i<counter; i++)
        {
            for(int j=0; j<4; j++)
            {
                vertices[nextVertex] = verticesMatrix[i][j];
                //colors[nextVertex] = matrixColors[i][j];
                nextVertex++;
            }
            for(int k=0; k<trianglesMatrix[i].Length; k++)
            {
                triangles[nextTriangle] = trianglesMatrix[i][k];
                nextTriangle++;
            }
        }
        nextVertex = 0;
        Color defColor = new Color(0, 0, 0);
        for(int i=0; i<200;i++)
        {
            for(int j=0; j<200; j++)
            {
                if(tiledMapColors[i][j][0].r >0 || tiledMapColors[i][j][0].b >0)
                {
                    for(int k=0; k<4; k++)
                    {
                        colors[nextVertex] = tiledMapColors[i][j][k];
                        nextVertex++;
                    }
                }
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
        mesh.colors = colors;
        mesh.RecalculateBounds();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshRenderer>().material = mat;
        obj.transform.localPosition = new Vector3(0.819f, 0.002f, 0.751f);
        obj.transform.localRotation = new Quaternion(0, 180, 0,0);
        obj.transform.localScale = new Vector3(0.6600493f, 0.6600493f, 0.6600493f);
        //thresholdPlane.SetActive(true);
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
        mesh = new Mesh();
        if(obj.GetComponent<MeshFilter>()!=null)
        {
            obj.GetComponent<MeshFilter>().mesh = mesh;
        }
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
        resizeMesh = false;
        
        tiledMapVertices = new Vector3[200][][];
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];
        gaussCoef.valuesCalculated = false;
        peaks = new List<Vector3>();
        //thresholdPlane.SetActive(false);
    }

    //checks whether a tile has any neighbours around that are part of the mesh
    bool HasNeighbours(int x, int z)
    {
        if(x>0)
        {
            if(mapTilesInfluence[x-1][z]>0)
            {
                return true;
            }
        }
        if(x<199)
        {
            if(mapTilesInfluence[x+1][z]>0)
            {
                return true;
            }
        }
        if(z>0)
        {
            if(mapTilesInfluence[x][z-1]>0)
            {
                return true;
            }
        }
        if(z<199)
        {
            if(mapTilesInfluence[x][z+1]>0)
            {
                return true;
            }
        }
        return false;
    }

    bool HasNeighbourOnTheLeft(int x, int z)
    {
        if(x>0)
        {
            if (mapTilesInfluence[x - 1][z] > 0)
            {
                return true;
            }
        }
        return false;
    }

    bool HasNeighbourDown(int x, int z)
    {
        if(z>0)
        {
            if(mapTilesInfluence[x][z-1]>0)
            {
                return true;
            }
        }
        return false;
    }

    private void ReturnPeaks()
    {
        for(int p=0; p<200;p++)
        {
            for(int r=0; r<200; r++)
            {
                if(verticesMaximumMatrix[p][r].y>0)
                {
                    List<Vector3> plateauList = new List<Vector3>();
                    plateauList.Add(verticesMaximumMatrix[p][r]);
                    bool tileIsPeak = IterateAround(plateauList, p, r);
                    isPeak[p][r] = tileIsPeak;
                }

                if (isPeak[p][r])
                {
                    for (int i = 0; i < 4; i++)
                    {
                        tiledMapColors[p][r][i] = new Color(1, 1, 1);
                    }
                }
            }
        }
    }

    bool IterateAround(List<Vector3> plateauList, int p, int r)
    {
        bool tileIsPeak = true;
        for(int i = p-1; i<p+2; i++)
        {
            for(int j=r-1; j<r+2; j++)
            {
                if (plateauList.Contains(verticesMaximumMatrix[i][j])) continue;
                if(verticesMaximumMatrix[i][j].y > verticesMaximumMatrix[p][r].y)
                {
                    isPeak[p][r] = false;
                    return false;
                }
                else if(verticesMaximumMatrix[i][j].y < verticesMaximumMatrix[p][r].y)
                {
                    tileIsPeak = true;
                }
                else if(verticesMaximumMatrix[i][j].y == verticesMaximumMatrix[p][r].y)
                {
                    plateauList.Add(verticesMaximumMatrix[i][j]);
                    tileIsPeak = IterateAround(plateauList, i, j);
                    if(!tileIsPeak)
                    {
                        isPeak[i][j] = false;
                        return false;
                    }
                }
            }
        }
        if(tileIsPeak)
        {
            isPeak[p][r] = true;
        }
        return tileIsPeak;
    }
}
