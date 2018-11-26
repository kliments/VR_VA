﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledmapGeneration : MonoBehaviour {
    public float[][] mapTilesInfluence;
    public float threshold;
    public Vector3[] positions;
    public List<Vector3> peaks;
    public Material mat, mat2;
    public bool gaussianCalculation, resizeMesh, _multiCenteredGaussian, _multiCenteredSquareWave;
    //plus/minus neighbourhood cubes around the center cube in the matrix
    public int halfLengthOfNeighbourhood;
    public GameObject thresholdPlane;
    public GaussianCoefficients gaussCoef;
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbscan;

    private GameObject _obj, _additionalObj;
    private Mesh _mesh;
    private List<GameObject> _list;
    private Vector3[][][] _tiledMapVertices;
    private Vector3[][] _mapPositions, _verticesMatrix, _verticesMaximumMatrix;
    private Vector3[] _vertices;
    private Vector3 _startSize, _finishSize, _pos0, _pos1, _pos2, _pos3;
    private List<Vector3> _additionalVertices;
    private int[][] _trianglesMatrix, _countersMatrix;
    private int[] _triangles, _originalTriangles;
    private List<List<int>> _additionalTriangles, _indexIterationBuffer;
    private int _counter, _tile0, _tile1, _tile2, _tile3;
    private float _x, _y, _z;
    private Color[][][] _tiledMapColors;
    private Color[][] _matrixColors;
    private Color[] _colors;
    private Color _clusterColor;
    private List<Color> _additionalVerticesColor;
    private bool[][] _isPeak, _clustered;
    private bool _skip;
    private RaycastHit _hit;
    // Use this for initialization
    void Start () {
        _x = -0.25f;
        _y = 0;
        _z = -0.25f;
        halfLengthOfNeighbourhood = 3;
        _counter = 0;
        _obj = new GameObject();
        _additionalObj = new GameObject();

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

        _startSize = new Vector3(1, 0.001f, 1);
        _finishSize = new Vector3(1, 1, 1);

        _additionalVertices = new List<Vector3>();
        _additionalTriangles = new List<List<int>>();
        _additionalVerticesColor = new List<Color>();
        _indexIterationBuffer = new List<List<int>>();
        _multiCenteredGaussian = false;
        _multiCenteredSquareWave = false;
    }
	
	// Update is called once per frame
	void Update () {
        if(resizeMesh)
        {
            _obj.transform.localScale = Vector3.Lerp(_obj.transform.localScale, _finishSize, Time.deltaTime*1);
        }

        if(_multiCenteredGaussian)
        {
            _multiCenteredGaussian = false;
            _additionalTriangles = new List<List<int>>();
            MultiCenteredGaussianClusters();
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

            CreateAdditionalMesh();
        }
        if(_multiCenteredSquareWave)
        {
            _multiCenteredSquareWave = false;
            _additionalTriangles = new List<List<int>>();
            MultiCenteredSquaredWaveClusters();
            /*int nextVertex = 0;
            for (int i=0; i<200; i++)
            {
                for(int j=0; j<200; j++)
                {
                    if (_tiledMapColors[i][j][0].r > 0 || _tiledMapColors[i][j][0].b > 0 )
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            _colors[nextVertex] = _tiledMapColors[i][j][k];
                            nextVertex++;
                        }
                    }
                }
            }
            _mesh.colors = _colors;*/
            CreateAdditionalMesh();
        }
		
	}

    public void StartDenclue()
    {
        kMeans.ResetMe();
        dbscan.ResetMe();
        ResetMe();
        IncreaseInfluence();
        _obj.transform.localScale = _finishSize;
        //resizeMesh = true;
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

                //if it is Square wave mesh
                if (!gaussianCalculation)
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
                        //need to create 2 extra rectangles around a tile
                        //center rectangle
                        _trianglesMatrix[currentTile] = new int[18];
                        _trianglesMatrix[currentTile][0] = count - 4;                                // vertex 0
                        _trianglesMatrix[currentTile][1] = count - 2;                                // 2
                        _trianglesMatrix[currentTile][2] = count - 3;                                // 1
                        _trianglesMatrix[currentTile][3] = count - 2;                                // 2
                        _trianglesMatrix[currentTile][4] = count - 1;                                // 3
                        _trianglesMatrix[currentTile][5] = count - 3;                                // 1
                        //left rectangle
                        _trianglesMatrix[currentTile][6] = count - 2;                                // 2
                        _trianglesMatrix[currentTile][7] = count - 4;                                // 0
                        _trianglesMatrix[currentTile][8] = (_countersMatrix[x - 1][z] + 1) * 4 - 1;  // vertex 3 from tile on the left
                        _trianglesMatrix[currentTile][9] = count - 4;                                // 0
                        _trianglesMatrix[currentTile][10] = (_countersMatrix[x - 1][z] + 1) * 4 - 3; // 1 from tile on the left
                        _trianglesMatrix[currentTile][11] = (_countersMatrix[x - 1][z] + 1) * 4 - 1; // 3 from tile on the left
                        //down rectangle
                        _trianglesMatrix[currentTile][12] = count - 4;                                 // vertex 0
                        _trianglesMatrix[currentTile][13] = count - 3;                                 // 1
                        _trianglesMatrix[currentTile][14] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        _trianglesMatrix[currentTile][15] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                        _trianglesMatrix[currentTile][16] = count - 3;                                 // 1
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

                //if it is Gaussian mesh
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
        //need to start from begining, to reposition vertices in Gaussian mesh
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
                        _verticesMaximumMatrix[i][j] = returnMax(_verticesMatrix[currentTile][0], _verticesMatrix[currentTile][1], _verticesMatrix[currentTile][2], _verticesMatrix[currentTile][3]);

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

    private Vector3 returnMax(Vector3 dL, Vector3 dR, Vector3 uL, Vector3 uR)
    {
        if (dL.y > dR.y && dL.y > uL.y && dL.y > uR.y) return dL;
        else if (dR.y > dL.y && dR.y > uL.y && dR.y > uR.y) return dR;
        else if (uL.y > dL.y && uL.y > dR.y && uL.y > uR.y) return uL;
        else return uR;
    }

    //converts the matrix of vertices to an array
    private void ConvertMatrixToArray()
    {
        int nextVertex = 0;
        int nextTriangle = 0;
        List<int> temp = new List<int>();
        _vertices = new Vector3[_counter * 4];
		_colors = new Color[_counter * 4];
        if(!gaussianCalculation)
        {
            _triangles = new int[_counter * 18];
            _originalTriangles = new int[_counter * 18];
        }
        else
        {
            _triangles = new int[_counter * 6];
            _originalTriangles = new int[_counter * 6];
        }
        for(int i=0; i<_counter; i++)
        {
            for(int j=0; j<4; j++)
            {
                _vertices[nextVertex] = _verticesMatrix[i][j];
                _additionalVertices.Add(_verticesMatrix[i][j]);
                nextVertex++;
            }
            for(int k=0; k<_trianglesMatrix[i].Length; k++)
            {
                _triangles[nextTriangle] = _trianglesMatrix[i][k];
                _originalTriangles[nextTriangle] = _trianglesMatrix[i][k];
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
                        _additionalVerticesColor.Add(_tiledMapColors[i][j][k]);
                        nextVertex++;
                    }
                }
            }
        }
        CreateMesh();
        ReturnPeaks();
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
        _obj.transform.localPosition = new Vector3(-0.581f, 0.002f, -0.63f);
    }    
    
    //change vertices of tiles to look like single mesh
    private Vector3[] ChangeVertices(Vector3[] actualVertex, float top, float down, float topRight, float right, float downLeft, float left)
    {
        Vector3[] vertex = new Vector3[4];
        if (actualVertex[0].y > 0)
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
        _originalTriangles = null;
        resizeMesh = false;
        
        _tiledMapVertices = new Vector3[200][][];
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];
        gaussCoef.valuesCalculated = false;
        peaks = new List<Vector3>();

        _additionalVertices = new List<Vector3>();
        _additionalTriangles = new List<List<int>>();
        _additionalVerticesColor = new List<Color>();
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
                /*if (_isPeak[p][r])
                {
                    int tile = _countersMatrix[p][r]*4 +3;
                    var pos = _obj.transform.TransformPoint(_vertices[tile]);
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = pos;
                    obj.transform.localScale = new Vector3(0.00002f, 0.00002f, 0.00002f);
                    for (int i = 0; i < 4; i++)
                    {
                        _tiledMapColors[p][r][i] = new Color(1, 1, 1);
                    }
                }*/
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

    private void MultiCenteredGaussianClusters()
    {
        ResetMesh();
        for (int a = 0; a < 200; a++)
        {
            for (int b = 0; b < 200; b++)
            {
                _clustered[a][b] = false;
            }
        }

        for (int i=0; i<200; i++)
        {
            for(int j=0; j<200; j++)
            {
                _tile0 = _countersMatrix[i][j] * 4;
                _tile1 = _countersMatrix[i][j] * 4 + 1;
                _tile2 = _countersMatrix[i][j] * 4 + 2;
                _tile3 = _countersMatrix[i][j] * 4 + 3;

                //if already belongs to cluster or if the maximum point is below the threshold, continue
                if (_clustered[i][j] || (_verticesMaximumMatrix[i][j].y + 0.002f < threshold)) continue;

                if (AllVerticesAbove(_tiledMapVertices[i][j], threshold))
                {
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[i][j] = true;
                    IterateMultiCenterGaussianClusterAround(i, j);
                }
                else if (ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], _tiledMapVertices[i][j][2], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], _tiledMapVertices[i][j][3], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][1], _tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold))
                {
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    IterateMultiCenterGaussianClusterAround(i, j);
                }

                else if (TwoVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][2], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][1], _tiledMapVertices[i][j][3], threshold))
                {
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    IterateMultiCenterGaussianClusterAround(i, j);
                }
                else if(_tiledMapVertices[i][j][0].y + 0.002f > threshold || _tiledMapVertices[i][j][1].y + 0.002f > threshold ||
                        _tiledMapVertices[i][j][2].y + 0.002f > threshold || _tiledMapVertices[i][j][3].y + 0.002f > threshold)
                {
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    IterateMultiCenterGaussianClusterAround(i, j);
                }
                //iterate over buffer of indexes already colored with cluster color
                if(_indexIterationBuffer.Count != 0)
                {
                    for(int b=0; b<_indexIterationBuffer.Count; b++)
                    {
                        IterateMultiCenterGaussianClusterAround(_indexIterationBuffer[b][0], _indexIterationBuffer[b][1]);
                    }
                    _indexIterationBuffer = new List<List<int>>();
                }
            }
        }
    }

    //Resets original mesh back to its original vertices, triangles and color
    private void ResetMesh()
    {
        _additionalVertices = new List<Vector3>();
        _additionalVerticesColor = new List<Color>();
        for (int t = 0; t < _originalTriangles.Length; t++)
        {
            _triangles[t] = _originalTriangles[t];
        }
        for (int v = 0; v < _vertices.Length; v++)
        {
            _additionalVertices.Add(_vertices[v]);
            _additionalVerticesColor.Add(_colors[v]);
        }
        _obj.GetComponent<MeshFilter>().mesh = new Mesh();
        _obj.GetComponent<MeshFilter>().mesh.vertices = _vertices;
        _obj.GetComponent<MeshFilter>().mesh.triangles = _triangles;
    }

    private void IterateMultiCenterGaussianClusterAround(int i, int j)
    {
        for(int k = i-1; k<i+2; k++)
        {
            for (int l = j - 1; l < j + 2; l++)
            {
                //if they are not connected with the previous tile, skip
                if (NotNeighbors(_tiledMapVertices[i][j], _tiledMapVertices[k][l])) continue;
                //if already belongs to cluster or if the maximum point is below the threshold, continue
                if (_clustered[k][l] || (_verticesMaximumMatrix[k][l].y + 0.002f < threshold)) continue;

                Vector3 vertex;
                _tile0 = _countersMatrix[k][l] * 4;
                _pos0 = _obj.transform.TransformPoint(_vertices[_tile0]);
                _tile1 = _countersMatrix[k][l] * 4 + 1;
                _pos1 = _obj.transform.TransformPoint(_vertices[_tile1]);
                _tile2 = _countersMatrix[k][l] * 4 + 2;
                _pos2 = _obj.transform.TransformPoint(_vertices[_tile2]);
                _tile3 = _countersMatrix[k][l] * 4 + 3;
                _pos3 = _obj.transform.TransformPoint(_vertices[_tile3]);

                //paint all vertices if all of them above the threshold
                if (AllVerticesAbove(_tiledMapVertices[k][l], threshold))
                {
                    _clustered[k][l] = true;
                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;


                    //add indexes for later iteration
                    List<int> temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                //three vertices are above the threshold, find two points where it cuts with threshold plane and add to mesh
                #region 3VerticesAbove
                //if 1,2,3 vertices are above the threshold, find points where it cuts the thresholdPlane
                else if (ThreeVerticesAbove(_tiledMapVertices[k][l][1],_tiledMapVertices[k][l][2],_tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos1, _pos0 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1-vertex.y)));
                    }
                    if(Physics.Raycast(_pos2, _pos0 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    //upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for(int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                //if 0,2,3 vertices are above the threshold..
                else if (ThreeVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][2], _tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos0, _pos1 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos1 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;
                    
                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                //if 0,1,3 vertices are above the threshold..
                else if (ThreeVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][1], _tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos0, _pos2 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos2 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    //upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                //if 0,1,2 vertices are above the threshold..
                else if (ThreeVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][1], _tiledMapVertices[k][l][2], threshold))
                {
                    if (Physics.Raycast(_pos2, _pos3 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos1, _pos3 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    //upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);
                    
                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                #endregion

                //two vertices are above the threshold, find two points where it cuts with threshold plane and add to mesh
                #region 2VerticesAbove
                else if (TwoVerticesAbove(_tiledMapVertices[k][l][2], _tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos2, _pos0 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos1-_pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    // lower triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);
                    
                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    _additionalTriangles.Add(temp);
                    
                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }

                else if (TwoVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][1], threshold))
                {
                    if (Physics.Raycast(_pos0, _pos2 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos1, _pos3 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    // lower triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }

                else if (TwoVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][2], threshold))
                {
                    if (Physics.Raycast(_pos0, _pos1 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos2, _pos3 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    // lower triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_tile3);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }

                else if(TwoVerticesAbove(_tiledMapVertices[k][l][1], _tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos1, _pos0 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos2 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    // lower triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 1);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }

                //tilted tile
                else if(TwoVerticesAbove(_tiledMapVertices[k][l][1], _tiledMapVertices[k][l][2], threshold))
                {
                    if (Physics.Raycast(_pos1, _pos0 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -8
                        _additionalVertices.Add(vertex); // -7
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos2, _pos0 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -6
                        _additionalVertices.Add(vertex); // -5
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos1, _pos3 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos2, _pos3 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 8);
                    temp.Add(_tile2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 8);
                    temp.Add(_additionalVertices.Count - 6);
                    temp.Add(_tile2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                //tilted tile
                else if (TwoVerticesAbove(_tiledMapVertices[k][l][0], _tiledMapVertices[k][l][3], threshold))
                {
                    if (Physics.Raycast(_pos0, _pos1 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -8
                        _additionalVertices.Add(vertex); // -7
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos1 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -6
                        _additionalVertices.Add(vertex); // -5
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos0, _pos2 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos2 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    // upper triangles with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 8);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 8);
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 6);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                #endregion

                //one vertex is above the threshold, find two points where it cuts with threshold plane and add to mesh
                #region 1VertexAbove

                else if ((_tiledMapVertices[k][l][0].y + 0.002f) > threshold)
                {
                    if (Physics.Raycast(_pos0, _pos1 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos0, _pos2 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangle with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    //lower triangles with normal color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);
                    
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_tile3);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                else if (_tiledMapVertices[k][l][2].y + 0.002f > threshold)
                {
                    if (Physics.Raycast(_pos2, _pos0 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos2, _pos3 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangle with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    //lower triangles with normal color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile0);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile3);
                    temp.Add(_tile0);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_tile1);
                    temp.Add(_tile0);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                else if((_tiledMapVertices[k][l][1].y + 0.002f) > threshold)
                {
                    if (Physics.Raycast(_pos1, _pos0 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos1, _pos3 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangle with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);

                    //lower triangles with normal color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_tile0);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                else if(_tiledMapVertices[k][l][3].y + 0.002f > threshold)
                {
                    if (Physics.Raycast(_pos3, _pos2 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _pos1 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangle with cluster color
                    List<int> temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);

                    //lower triangles with normal color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile0);
                    temp.Add(_tile2);
                    _additionalTriangles.Add(temp);

                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_tile1);
                    temp.Add(_tile0);
                    _additionalTriangles.Add(temp);

                    //remove previous triangles
                    for (int t = 0; t < 6; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 6 + t] = 0;
                    }

                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[k][l] = true;

                    //add indexes for later iteration
                    temp = new List<int>();
                    temp.Add(k);
                    temp.Add(l);
                    _indexIterationBuffer.Add(temp);
                }
                #endregion
            }
        }
    }

    private void MultiCenteredSquaredWaveClusters()
    {
        ResetMesh();
        for(int a = 0; a<200; a++)
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
                _tile0 = _countersMatrix[i][j] * 4;
                _tile1 = _countersMatrix[i][j] * 4 + 1;
                _tile2 = _countersMatrix[i][j] * 4 + 2;
                _tile3 = _countersMatrix[i][j] * 4 + 3;

                //if tile already belongs to cluster or if it is below the threshold continue
                if (_clustered[i][j] || (_tiledMapVertices[i][j][0].y + 0.002f) < threshold) continue;
                else
                {
                    _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    IterateMultiCenterSquaredWaveClusterAround(i, j);
                }
                //iterate over buffer of indexes already colored with cluster color
                if (_indexIterationBuffer.Count != 0)
                {
                    for (int b = 0; b < _indexIterationBuffer.Count; b++)
                    {
                        IterateMultiCenterSquaredWaveClusterAround(_indexIterationBuffer[b][0], _indexIterationBuffer[b][1]);
                    }
                    _indexIterationBuffer = new List<List<int>>();
                }
            }
        }
    }

    private void IterateMultiCenterSquaredWaveClusterAround(int i, int j)
    {
        for(int k = i-1; k<i+2; k++)
        {
            for(int l = j-1; l<j+2; l++)
            {
                if (_clustered[k][l] || (_tiledMapVertices[k][l][0].y + 0.002f) < threshold) continue;
                _clustered[k][l] = true;
                Vector3 vertex;
                List<int> temp = new List<int>();
                _tile0 = _countersMatrix[k][l] * 4;
                _pos0 = _obj.transform.TransformPoint(_vertices[_tile0]);
                _tile1 = _countersMatrix[k][l] * 4 + 1;
                _pos1 = _obj.transform.TransformPoint(_vertices[_tile1]);
                _tile2 = _countersMatrix[k][l] * 4 + 2;
                _pos2 = _obj.transform.TransformPoint(_vertices[_tile2]);
                _tile3 = _countersMatrix[k][l] * 4 + 3;
                _pos3 = _obj.transform.TransformPoint(_vertices[_tile3]);

                //left tile
                int _leftTile1 = _countersMatrix[k - 1][l] * 4 + 1;
                int _leftTile3 = _countersMatrix[k - 1][l] * 4 + 3;
                Vector3 _leftPos1 = _obj.transform.TransformPoint(_vertices[_leftTile1]);
                Vector3 _leftPos3 = _obj.transform.TransformPoint(_vertices[_leftTile3]);

                //down tile
                int _downTile2 = _countersMatrix[k][l - 1] * 4 + 2;
                int _downTile3 = _countersMatrix[k][l - 1] * 4 + 3;
                Vector3 _downPos2 = _obj.transform.TransformPoint(_vertices[_downTile2]);
                Vector3 _downPos3 = _obj.transform.TransformPoint(_vertices[_downTile3]);

                //right tile
                int _rightTile0 = _countersMatrix[k + 1][l] * 4;
                int _rightTile2 = _countersMatrix[k + 1][l] * 4 + 2;
                Vector3 _rightPos0 = _obj.transform.TransformPoint(_vertices[_rightTile0]);
                Vector3 _rightPos2 = _obj.transform.TransformPoint(_vertices[_rightTile2]);

                //up tile
                int _upTile0 = _countersMatrix[k][l + 1] * 4;
                int _upTile1 = _countersMatrix[k][l + 1] * 4 + 1;
                Vector3 _upPos0 = _obj.transform.TransformPoint(_vertices[_upTile0]);
                Vector3 _upPos1 = _obj.transform.TransformPoint(_vertices[_upTile1]);

                //if current tile is higher than down tile
                if (_pos0.y > _downPos2.y && _downPos2.y + 0.002f < threshold)
                {
                    if(Physics.Raycast(_pos0, _downPos2 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos1, _downPos3 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    //upper triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);
                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_downTile2);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_downTile3);
                    temp.Add(_downTile2);
                    _additionalTriangles.Add(temp);

                    for(int t=12; t<18; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 18 + t] = 0;
                    }
                }
                //if current tile is higher than left tile that is lower than threshold
                if (_pos0.y > _leftPos1.y && _leftPos1.y + 0.002f < threshold)
                {
                    if(Physics.Raycast(_pos0, _leftPos1 - _pos0, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos2, _leftPos3 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile0);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_tile2);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);
                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_leftTile1);
                    temp.Add(_additionalVertices.Count - 1);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_leftTile1);
                    temp.Add(_leftTile3);
                    _additionalTriangles.Add(temp);
                    
                    for (int t = 6; t < 12; t++)
                    {
                        _triangles[_countersMatrix[k][l] * 18 + t] = 0;
                    }
                }
                //if current tile is higher than right tile
                if (_pos1.y > _rightPos0.y && _rightPos0.y + 0.002f < threshold)
                {
                    if (Physics.Raycast(_pos1, _rightPos0 - _pos1, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _rightPos2 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    
                    //upper triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile1);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_additionalVertices.Count - 4);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_tile3);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile1);
                    _additionalTriangles.Add(temp);
                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_rightTile2);
                    temp.Add(_rightTile0);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 1);
                    temp.Add(_rightTile2);
                    temp.Add(_additionalVertices.Count - 3);
                    _additionalTriangles.Add(temp);

                    for(int t=6; t<12; t++)
                    {
                        _triangles[_countersMatrix[k+1][l] * 18 + t] = 0;
                    }
                }
                //if current tile is higher than up tile
                if(_pos2.y > _upPos0.y && _upPos0.y + 0.002f < threshold)
                {
                    if (Physics.Raycast(_pos2, _upPos0 - _pos2, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -4
                        _additionalVertices.Add(vertex); // -3
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }
                    if (Physics.Raycast(_pos3, _upPos1 - _pos3, out _hit, 5f))
                    {
                        vertex = _hit.point;
                        vertex.x += 0.581f;
                        vertex.y -= 0.002f;
                        vertex.z += 0.63f;
                        _additionalVertices.Add(vertex); // -2
                        _additionalVertices.Add(vertex); // -1
                        _additionalVerticesColor.Add(_clusterColor);
                        _additionalVerticesColor.Add(new Color(vertex.y, 0, Mathf.Abs(1 - vertex.y)));
                    }

                    //upper triangles with cluster color
                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 2);
                    temp.Add(_tile3);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_tile2);
                    temp.Add(_additionalVertices.Count - 4);
                    temp.Add(_additionalVertices.Count - 2);
                    _additionalTriangles.Add(temp);
                    //lower triangles with original color
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_upTile0);
                    temp.Add(_upTile1);
                    _additionalTriangles.Add(temp);
                    temp = new List<int>();
                    temp.Add(_additionalVertices.Count - 3);
                    temp.Add(_upTile1);
                    temp.Add(_additionalVertices.Count - 1);
                    _additionalTriangles.Add(temp);
                    
                    for (int t = 12; t < 18; t++)
                    {
                        _triangles[_countersMatrix[k][l+1] * 18 + t] = 0;
                    }
                }

                _additionalVerticesColor[_tile0] = _clusterColor;
                _additionalVerticesColor[_tile1] = _clusterColor;
                _additionalVerticesColor[_tile2] = _clusterColor;
                _additionalVerticesColor[_tile3] = _clusterColor;
                _clustered[k][l] = true;

                //add indexes for later iteration
                temp = new List<int>();
                temp.Add(k);
                temp.Add(l);
                _indexIterationBuffer.Add(temp);
            }
        }
    }

    private void CreateAdditionalMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] tempVertices = new Vector3[_additionalVertices.Count];
        Color[] tempColors = new Color[_additionalVerticesColor.Count];
        //int[] tempTriangles = new int[_additionalTriangles.Count * 3];
        int[] tempTriangles = new int[_additionalTriangles.Count * 3 + _triangles.Length];
        for(int i=0; i<_additionalVertices.Count; i++)
        {
            tempVertices[i] = _additionalVertices[i];
            tempColors[i] = _additionalVerticesColor[i];
        }
        int x = 0;
        int y = 0;
        int z = 0;
        for(int i=0; i<_triangles.Length; i++)
        {
            tempTriangles[i] = _triangles[i];
            x = i+1;
        }
        for(int i=0; i<_additionalTriangles.Count; i++)
        {
            for(int j=0; j<_additionalTriangles[i].Count; j++)
            {
                tempTriangles[x] = _additionalTriangles[i][j];
                x++;
            }
        }
        mesh.vertices = tempVertices;
        mesh.triangles = tempTriangles;
        mesh.colors = tempColors;
        mesh.RecalculateBounds();
        _obj.GetComponent<MeshFilter>().mesh = null;
        _obj.GetComponent<MeshFilter>().mesh = mesh;
    }

    private bool AllVerticesAbove(Vector3[] tile, float threshold)
    {
        foreach (Vector3 vertex in tile)
        {
            if ((vertex.y + 0.002f) < threshold)
                return false;
        }
        return true;
    }
    
    private bool ThreeVerticesAbove(Vector3 a, Vector3 b, Vector3 c, float threshold)
    {
        if ((a.y + 0.002f) >= threshold && (b.y + 0.002f) >= threshold && (c.y + 0.002f) >= threshold) return true;
        return false;
    }

    private bool TwoVerticesAbove(Vector3 a, Vector3 b, float threshold)
    {
        if ((a.y + 0.002f) > threshold && (b.y + 0.002f) > threshold) return true;
        return false;
    }

    private bool NotNeighbors(Vector3[] center, Vector3[] current)
    {
        if (center[0].y == current[3].y && (center[0].y + 0.002f) > threshold ||
            center[0].y == current[2].y && (center[0].y + 0.002f) > threshold ||
            center[0].y == current[1].y && (center[0].y + 0.002f) > threshold) return false;
        else if (center[1].y == current[3].y && (center[1].y + 0.002f) > threshold ||
                 center[1].y == current[2].y && (center[1].y + 0.002f) > threshold ||
                 center[1].y == current[0].y && (center[1].y + 0.002f) > threshold) return false;
        else if (center[2].y == current[0].y && (center[2].y + 0.002f) > threshold ||
                 center[2].y == current[1].y && (center[2].y + 0.002f) > threshold ||
                 center[2].y == current[3].y && (center[2].y + 0.002f) > threshold) return false;
        else if (center[3].y == current[0].y && (center[3].y + 0.002f) > threshold ||
                 center[3].y == current[1].y && (center[3].y + 0.002f) > threshold ||
                 center[3].y == current[2].y && (center[3].y + 0.002f) > threshold) return false;
        return true;
    }
}
