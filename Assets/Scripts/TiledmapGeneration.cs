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
    public bool resizeMesh, paintRed;
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbscan;

    private GameObject _obj;
    private Mesh _mesh;
    private int _counter;
    private List<GameObject> _list;
    private Vector3[][][] _tiledMapVertices;
    private Vector3[][] _mapPositions, _verticesMatrix, _verticesMaximumMatrix;
    private Vector3[] _vertices;
    private int[][] _trianglesMatrix, _countersMatrix;
    private int[] _triangles;
    private float _x, _y, _z;
    private Vector3 _startSize, _finishSize;
    private Color[][][] _tiledMapColors;
    private Color[][] _matrixColors;
    private Color[] _colors;
    private Color _clusterColor;
    private bool[][] _isPeak, _clustered;
    // Use this for initialization
    void Start () {
        _x = -0.25f;
        _y = 0;
        _z = -0.25f;
        halfLengthOfNeighbourhood = 3;
        _counter = 0;
        threshold = 0.0021f;
        _obj = new GameObject();

        _list = new List<GameObject>();
        //set initial abstract positions, to refer to float influence
        _mapPositions = new Vector3[200][];
        for(int i=0; i<_mapPositions.Length; i++)
        {
            _mapPositions[i] = new Vector3[200];
            for(int j=0; j<_mapPositions[i].Length; j++)
            {
                _mapPositions[i][j] = new Vector3(_x, _y, _z);
                _z += 0.0075f;
            }
            _x += 0.0075f;
            _z = -0.25f;
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

        _tiledMapVertices = new Vector3[200][][];
        _tiledMapColors = new Color[200][][];
        peaks = new List<Vector3>();
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];

        _startSize = new Vector3(0.66f, 0.001f, 0.66f);
        _finishSize = new Vector3(0.66f, 0.66f, 0.66f);

        paintRed = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(resizeMesh)
        {
            _obj.transform.localScale = Vector3.Lerp(_obj.transform.localScale, _finishSize, Time.deltaTime*1);
        }

        if(paintRed)
        {
            paintRed = false;
            MultiCenteredClusters();
            int nextVertex = 0;
            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 200; j++)
                {
                    if (_tiledMapColors[i][j][0].r > 0 || _tiledMapColors[i][j][0].b > 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            _colors[nextVertex] = _tiledMapColors[i][j][k];
                            nextVertex++;
                        }
                    }
                }
            }
            _mesh.colors = _colors;
        }
		
	}

    public void StartDenclue()
    {
        kMeans.ResetMe();
        dbscan.ResetMe();
        ResetMe();
        IncreaseInfluence();
        _obj.transform.localScale = _startSize;
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
        if (ChebyshevDistance(position, _mapPositions[m][l]) <= 0.0075f / 2)
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
                                gaussCoef.gaussianPositionMatrix[gX][gY] = _mapPositions[xPos + gX][zPos + gY];
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
                    _counter++;
                }
                else if(mapTilesInfluence[x][z]>0 && gaussianCalculation)
                {
                    _counter++;
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
        _verticesMatrix = new Vector3[_counter][];
        _trianglesMatrix = new int[_counter][];
        _countersMatrix = new int[200][];
        _isPeak = new bool[200][];
		_matrixColors = new Color[_counter][];
        _verticesMaximumMatrix = new Vector3[200][];
        _clustered = new bool[200][];
        int currentTile = 0;
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            _tiledMapVertices[x] = new Vector3[200][];
            _countersMatrix[x] = new int[200];
            _verticesMaximumMatrix[x] = new Vector3[200];
            _tiledMapColors[x] = new Color[200][];
            _isPeak[x] = new bool[200];
            _clustered[x] = new bool[200];
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                _tiledMapVertices[x][z] = new Vector3[4];
                _tiledMapColors[x][z] = new Color[4];
                if(!gaussianCalculation)
                { 
                    //first check for border tiles, to create fictional vertices to access later for creating the walls of the mesh
                    if (mapTilesInfluence[x][z] == 0 && HasNeighbours(x, z))
                    {
                        _verticesMatrix[currentTile] = new Vector3[4];
						_matrixColors[currentTile] = new Color[4];
                        _countersMatrix[x][z] = currentTile;
                        int count = (currentTile + 1) * 4;
                        //bottom left vertex
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        _trianglesMatrix[currentTile] = new int[18];
                        for (int t = 0; t < 18; t++)
                        {
                            _trianglesMatrix[currentTile][t] = count - 4;
                        }
                        // if there is an neighbour down of current tile, create a square that represents wall to the ground
                        if (HasNeighbourDown(x, z))
                        {
                            _trianglesMatrix[currentTile][0] = count - 4;                                // vertex 0
                            _trianglesMatrix[currentTile][1] = count - 3;                                // 1
                            _trianglesMatrix[currentTile][2] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            _trianglesMatrix[currentTile][3] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            _trianglesMatrix[currentTile][4] = count - 3;                                // 1
                            _trianglesMatrix[currentTile][5] = (_countersMatrix[x][z - 1] + 1) * 4 - 1;   // 3 from tile down
                        }
                        if (HasNeighbourOnTheLeft(x, z))
                        {
                            _trianglesMatrix[currentTile][6] = count - 2;                               // 2
                            _trianglesMatrix[currentTile][7] = count - 4;                               // 0
                            _trianglesMatrix[currentTile][8] = (_countersMatrix[x - 1][z] + 1) * 4 - 1;  // vertex 3 from tile on the left
                            _trianglesMatrix[currentTile][9] = count - 4;                               // 0
                            _trianglesMatrix[currentTile][10] = (_countersMatrix[x - 1][z] + 1) * 4 - 3; // 1 from tile on the left
                            _trianglesMatrix[currentTile][11] = (_countersMatrix[x - 1][z] + 1) * 4 - 1; // 3 from tile on the left
                        }
						//set the current tile to blue, since it is a "wall tile" on the ground
						for (int c = 0; c < 4; c++)
						{
							_matrixColors[currentTile][c] = new Color(0, 0, Math.Abs(1-_verticesMatrix[currentTile][c].y));
                            _tiledMapColors[x][z][c] = new Color(0, 0, Math.Abs(1 - _verticesMatrix[currentTile][c].y));
                        }
                        _verticesMaximumMatrix[x][z] = _tiledMapVertices[x][z][0];
                        currentTile++;
                    }
                    else if (mapTilesInfluence[x][z] > 0)
                    {
                        _verticesMatrix[currentTile] = new Vector3[4];
						_matrixColors[currentTile] = new Color[4];

                        //bottom left vertex
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        _countersMatrix[x][z] = currentTile;

                        int count = (currentTile + 1) * 4;
                        //need to create 5 extra rectangles around a tile, to look like a cube
                        //center rectangle
                        _trianglesMatrix[currentTile] = new int[18];
                        _trianglesMatrix[currentTile][0] = count - 4; // vertex 0
                        _trianglesMatrix[currentTile][1] = count - 2; // 2
                        _trianglesMatrix[currentTile][2] = count - 3; // 1
                        _trianglesMatrix[currentTile][3] = count - 2; // 2
                        _trianglesMatrix[currentTile][4] = count - 1; // 3
                        _trianglesMatrix[currentTile][5] = count - 3; // 1
                                                                     //left rectangle
                        _trianglesMatrix[currentTile][6] = count - 2; // 2
                        _trianglesMatrix[currentTile][7] = count - 4; // 0
                        _trianglesMatrix[currentTile][8] = (_countersMatrix[x - 1][z] + 1) * 4 - 1; // vertex 3 from tile on the left
                        _trianglesMatrix[currentTile][9] = count - 4;                      // 0
                        _trianglesMatrix[currentTile][10] = (_countersMatrix[x - 1][z] + 1) * 4 - 3; // 1 from tile on the left
                        _trianglesMatrix[currentTile][11] = (_countersMatrix[x - 1][z] + 1) * 4 - 1; // 3 from tile on the left
                                                                                                   //down rectangle
                        _trianglesMatrix[currentTile][12] = count - 4;                      // vertex 0
                        _trianglesMatrix[currentTile][13] = count - 3;                      // 1
                        _trianglesMatrix[currentTile][14] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        _trianglesMatrix[currentTile][15] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        _trianglesMatrix[currentTile][16] = count - 3;                      // 1
                        _trianglesMatrix[currentTile][17] = (_countersMatrix[x][z - 1] + 1) * 4 - 1;   // 3 from tile down
						
						for (int c = 0; c < 4; c++)
						{
							_matrixColors[currentTile][c] = new Color(_verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - _verticesMatrix[currentTile][c].y));
                            _tiledMapColors[x][z][c] = new Color(_verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - _verticesMatrix[currentTile][c].y));
                        }
                        _verticesMaximumMatrix[x][z] = _tiledMapVertices[x][z][0];
                        currentTile++;
                    }
                }
                else
                {
                    if (mapTilesInfluence[x][z] > 0)
                    { 
                        _verticesMatrix[currentTile] = new Vector3[4];
						_matrixColors[currentTile] = new Color[4];
                        //bottom left vertex
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.00375f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.00375f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.00375f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.00375f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        _countersMatrix[x][z] = currentTile;
                        //center rectangle
                        int count = (currentTile + 1) * 4;
                        _trianglesMatrix[currentTile] = new int[6];
                        _trianglesMatrix[currentTile][0] = count - 4;
                        _trianglesMatrix[currentTile][1] = count - 2;
                        _trianglesMatrix[currentTile][2] = count - 3;
                        _trianglesMatrix[currentTile][3] = count - 2;
                        _trianglesMatrix[currentTile][4] = count - 1;
                        _trianglesMatrix[currentTile][5] = count - 3;

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

                        _verticesMatrix[currentTile] = ChangeVertices(_verticesMatrix[currentTile], _tiledMapVertices[i][j + 1][0].y, _tiledMapVertices[i][j - 1][3].y, _tiledMapVertices[i + 1][j + 1][0].y,
                                                                     _tiledMapVertices[i + 1][j][0].y, _tiledMapVertices[i - 1][j - 1][3].y, _tiledMapVertices[i - 1][j][3].y);
                        _tiledMapVertices[i][j] = _verticesMatrix[currentTile];
                        _verticesMaximumMatrix[i][j] = _tiledMapVertices[i][j][3];

                        for (int c = 0; c < 4; c++)
                        {
                            _matrixColors[currentTile][c] = new Color(_verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - _verticesMatrix[currentTile][c].y));
                            _tiledMapColors[i][j][c] = new Color(_verticesMatrix[currentTile][c].y, 0, Math.Abs(1 - _verticesMatrix[currentTile][c].y));
                        }
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
        _vertices = new Vector3[_counter * 4];
		_colors = new Color[_counter * 4];
        if(!gaussianCalculation)
        {
            _triangles = new int[_counter * 18];
        }
        else
        {
            _triangles = new int[_counter * 6];
        }
        for(int i=0; i<_counter; i++)
        {
            for(int j=0; j<4; j++)
            {
                _vertices[nextVertex] = _verticesMatrix[i][j];
                nextVertex++;
            }
            for(int k=0; k<_trianglesMatrix[i].Length; k++)
            {
                _triangles[nextTriangle] = _trianglesMatrix[i][k];
                nextTriangle++;
            }
        }
        nextVertex = 0;
        for(int i=0; i<200;i++)
        {
            for(int j=0; j<200; j++)
            {
                if(_tiledMapColors[i][j][0].r >0 || _tiledMapColors[i][j][0].b >0)
                {
                    for(int k=0; k<4; k++)
                    {
                        _colors[nextVertex] = _tiledMapColors[i][j][k];
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
        _mesh = new Mesh();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();
        _obj.AddComponent<MeshFilter>();
        _obj.AddComponent<MeshRenderer>();
        _obj.GetComponent<MeshFilter>().mesh = _mesh;
        _obj.GetComponent<MeshRenderer>().material = mat;
        _obj.transform.localPosition = new Vector3(0.819f, 0.002f, 0.751f);
        _obj.transform.localRotation = new Quaternion(0, 180, 0,0);
        _obj.transform.localScale = new Vector3(0.6600493f, 0.6600493f, 0.6600493f);
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
        _mesh = new Mesh();
        if(_obj.GetComponent<MeshFilter>()!=null)
        {
            _obj.GetComponent<MeshFilter>().mesh = _mesh;
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
        _counter = 0;
        _vertices = null;
        _triangles = null;
        resizeMesh = false;
        
        _tiledMapVertices = new Vector3[200][][];
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
                if(_verticesMaximumMatrix[p][r].y>0)
                {
                    List<Vector3> plateauList = new List<Vector3>();
                    plateauList.Add(_verticesMaximumMatrix[p][r]);
                    bool tileIsPeak = IteratePlateauAround(plateauList, p, r);
                    _isPeak[p][r] = tileIsPeak;
                }
                //set the peak color to white
                if (_isPeak[p][r])
                {
                    /*for (int i = 0; i < 4; i++)
                    {
                        _tiledMapColors[p][r][i] = new Color(1, 1, 1);
                    }*/
                }
            }
        }
    }

    bool IteratePlateauAround(List<Vector3> plateauList, int p, int r)
    {
        bool tileIsPeak = true;
        for(int i = p-1; i<p+2; i++)
        {
            for(int j=r-1; j<r+2; j++)
            {
                if (plateauList.Contains(_verticesMaximumMatrix[i][j])) continue;
                if(_verticesMaximumMatrix[i][j].y > _verticesMaximumMatrix[p][r].y)
                {
                    _isPeak[p][r] = false;
                    return false;
                }
                else if(_verticesMaximumMatrix[i][j].y < _verticesMaximumMatrix[p][r].y)
                {
                    tileIsPeak = true;
                }
                else if(_verticesMaximumMatrix[i][j].y == _verticesMaximumMatrix[p][r].y)
                {
                    plateauList.Add(_verticesMaximumMatrix[i][j]);
                    tileIsPeak = IteratePlateauAround(plateauList, i, j);
                    if(!tileIsPeak)
                    {
                        _isPeak[i][j] = false;
                        return false;
                    }
                }
            }
        }
        if(tileIsPeak)
        {
            _isPeak[p][r] = true;
        }
        return tileIsPeak;
    }

    private void MultiCenteredClusters()
    {
        for(int a=0; a<200; a++)
        {
            for(int b=0; b<200; b++)
            {
                _clustered[a][b] = false;
            }
        }

        for(int i=0; i<200; i++)
        {
            for(int j=0; j<200; j++)
            {
                if((_tiledMapVertices[i][j][0].y * 0.66f) + 0.002f > threshold && !_clustered[i][j])
                {
                    List<Vector3> clusterList = new List<Vector3>();
                    _clustered[i][j] = true;
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    clusterList.Add(_tiledMapVertices[i][j][0]);
                    for (int c = 0; c < 4; c++)
                    {
                        _tiledMapColors[i][j][c] = _clusterColor;
                    }
                    IterateMultiCenterClusterAround(clusterList, i, j);
                }
            }
        }
    }

    private void IterateMultiCenterClusterAround(List<Vector3> clusterList, int i, int j)
    {
        for(int k = i-1; k<i+2; k++)
        {
            for(int l=j-1; l<j+2; l++)
            {
                if (clusterList.Contains(_tiledMapVertices[k][l][0])) continue;
                else if ((_tiledMapVertices[k][l][0].y * 0.66f) + 0.002f < threshold)
                {
                    //tile on the left
                    if(k==i-1 && l==j)
                    {
                        _tiledMapColors[k][l][1] = _clusterColor;
                        _tiledMapColors[k][l][3] = _clusterColor;
                    }
                    //tile above
                    else if(k==i && l==j+1)
                    {
                        _tiledMapColors[k][l][0] = _clusterColor;
                        _tiledMapColors[k][l][1] = _clusterColor;
                    }
                    //tile on the right
                    else if (k == i+1 && l == j)
                    {
                        _tiledMapColors[k][l][0] = _clusterColor;
                        _tiledMapColors[k][l][2] = _clusterColor;
                    }
                    else if(k==i && l==j-1)
                    {
                        _tiledMapColors[k][l][2] = _clusterColor;
                        _tiledMapColors[k][l][3] = _clusterColor;
                    }
                }
                else if((_tiledMapVertices[k][l][0].y * 0.66f) + 0.002f >= threshold)
                {
                    clusterList.Add(_tiledMapVertices[k][l][0]);
                    _clustered[k][l] = true;
                    for(int c=0; c<4; c++)
                    {
                        _tiledMapColors[k][l][c] = _clusterColor;
                    }
                    IterateMultiCenterClusterAround(clusterList, k, l);
                }
            }
        }
    }
}
