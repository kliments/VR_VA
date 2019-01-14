using System;
using System.Collections.Generic;
using UnityEngine;

public class TiledmapGeneration : MonoBehaviour {
    public float[][] mapTilesInfluence;
    public float threshold;
    public Vector3[] positions;
    public List<Vector3> peaks, _peaksPosition;
    public Material mat, mat2;
    public bool gaussianCalculation, resizeMesh, _multiCenteredGaussian, _multiCenteredSquareWave, returnPeaks, _singleCenteredSquaredWave, multiCentered;
    //plus/minus neighbourhood cubes around the center cube in the matrix
    public int halfLengthOfNeighbourhood;
    public GameObject thresholdPlane;
    public GaussianCoefficients gaussCoef;
    public KMeansAlgorithm kMeans;
    public DBScanAlgorithm dbscan;
    public int colorCounter = 0;
    public List<Color> clusterColors;

    private GameObject _obj;
    private Mesh _mesh;
    private List<List<int>> _additionalTriangles, _indexIterationBuffer, _counterIndexes;
    private List<GameObject> _list;
    private List<Color> _additionalVerticesColor;
    private List<Vector3> _additionalVertices;
    private List<int>[][] _extraVerticesCounter;
    private List<Vector3>[][] _extraVertices;
    private Vector3[][][] _tiledMapVertices;
    private Vector3[][] _mapPositions, _verticesMatrix, _verticesMaximumMatrix;
    private Vector3[] _vertices;
    private Vector3 _startSize, _finishSize, _pos0, _pos1, _pos2, _pos3;
    private Color[][][] _tiledMapColors;
    private Color[][] _matrixColors;
    private Color[] _colors;
    private Color _clusterColor;
    private bool[][] _isPeak, _clustered, _pathFinderChecked;
    private bool _skip;
    private int[][] _trianglesMatrix, _countersMatrix;
    private int[] _triangles, _originalTriangles;
    private int _counter, _tile0, _tile1, _tile2, _tile3, a, b, c, d;
    private float _x, _y, _z;
    private RaycastHit _hit;
    // Use this for initialization
    void Start () {
        _x = -0.25f;
        _y = 0;
        _z = -0.25f;
        halfLengthOfNeighbourhood = 3;
        _counter = 0;
        _obj = new GameObject();

        _list = new List<GameObject>();
        //set initial abstract positions, to refer to float influence
        _mapPositions = new Vector3[150][];
        for(int i=0; i<_mapPositions.Length; i++)
        {
            _mapPositions[i] = new Vector3[150];
            for(int j=0; j<_mapPositions[i].Length; j++)
            {
                _mapPositions[i][j] = new Vector3(_x, _y, _z);
                _z += 0.01f;
            }
            _x += 0.01f;
            _z = -0.25f;
        }

        //influence depending on how many neighbouring "tiles" will be affected
        mapTilesInfluence = new float[150][];
        for(int m=0; m<mapTilesInfluence.Length; m++)
        {
            mapTilesInfluence[m] = new float[150];
            for(int l=0; l<mapTilesInfluence[m].Length; l++)
            {
                mapTilesInfluence[m][l] = 0;
            }
        }

        _tiledMapVertices = new Vector3[150][][];
        _tiledMapColors = new Color[150][][];
        peaks = new List<Vector3>();
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];

        _startSize = new Vector3(1, 0.001f, 1);
        _finishSize = new Vector3(1, 1, 1);

        _additionalVertices = new List<Vector3>();
        _extraVerticesCounter = new List<int>[150][];
        _extraVertices = new List<Vector3>[150][];
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
            multiCentered = true;
            _multiCenteredGaussian = false;
            _additionalTriangles = new List<List<int>>();
            MultiCenteredGaussianClusters();
            int nextVertex = 0;
            for (int i = 0; i < 150; i++)
            {
                for (int j = 0; j < 150; j++)
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
            multiCentered = true;
            _multiCenteredSquareWave = false;
            _additionalTriangles = new List<List<int>>();
            MultiCenteredSquaredWaveClusters();
            CreateAdditionalMesh();
        }

        if(_singleCenteredSquaredWave)
        {
            multiCentered = false;
            _singleCenteredSquaredWave = false;
            _additionalTriangles = new List<List<int>>();
            SingleCenteredSquaredWaveClusters();
            CreateAdditionalMesh();
        }

        if (returnPeaks)
        {
            returnPeaks = false;
            ReturnPeaks();
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
                for (int m = 0; m < 75; m++)
                {
                    for (int l = 0; l < 75; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else if (positions[d].x < 0.5f && positions[d].z >= 0.5f)
            {
                for (int m = 0; m < 75; m++)
                {
                    for (int l = 75; l < 150; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else if (positions[d].x >= 0.5f && positions[d].z < 0.5f)
            {
                for (int m = 75; m < 150; m++)
                {
                    for (int l = 0; l < 75; l++)
                    {
                        FeedMapTilesInfluence(positions[d], m, l);
                    }
                }
            }
            else
            {
                for (int m = 75; m < 150; m++)
                {
                    for (int l = 75; l < 150; l++)
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
        if (ChebyshevDistance(position, _mapPositions[m][l]) <= 0.01f / 2)
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
        _countersMatrix = new int[150][];
        _counterIndexes = new List<List<int>>();
        _isPeak = new bool[150][];
        _pathFinderChecked = new bool[150][];
		_matrixColors = new Color[_counter][];
        _verticesMaximumMatrix = new Vector3[150][];
        _clustered = new bool[150][];
        int currentTile = 0;
        for (int x = 0; x < mapTilesInfluence.Length; x++)
        {
            List<int> _indexesList = new List<int>();
            _tiledMapVertices[x] = new Vector3[150][];
            _extraVerticesCounter[x] = new List<int>[150];
            _extraVertices[x] = new List<Vector3>[150];
            _countersMatrix[x] = new int[150];
            _verticesMaximumMatrix[x] = new Vector3[150];
            _tiledMapColors[x] = new Color[150][];
            _isPeak[x] = new bool[150];
            _pathFinderChecked[x] = new bool[150];
            _clustered[x] = new bool[150];
            for (int z = 0; z < mapTilesInfluence[x].Length; z++)
            {
                _tiledMapVertices[x][z] = new Vector3[4];
                _tiledMapColors[x][z] = new Color[4];
                _indexesList = new List<int>();

                //if it is Square wave mesh
                if (!gaussianCalculation)
                { 
                    //first check for border tiles, to create fictional vertices to access later for creating the walls of the mesh
                    if (mapTilesInfluence[x][z] == 0 && HasNeighbours(x, z))
                    {
                        _verticesMatrix[currentTile] = new Vector3[4];
						_matrixColors[currentTile] = new Color[4];
                        _countersMatrix[x][z] = currentTile;
                        _indexesList.Add(x);
                        _indexesList.Add(z);
                        int count = (currentTile + 1) * 4;
                        //bottom left vertex
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        _trianglesMatrix[currentTile] = new int[18];
                        for (int t = 0; t < 18; t++)
                        {
                            _trianglesMatrix[currentTile][t] = count - 4;
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
                        // if there is an neighbour down of current tile, create a square that represents wall to the ground
                        if (HasNeighbourDown(x, z))
                        {
                            _trianglesMatrix[currentTile][12] = count - 4;                                // vertex 0
                            _trianglesMatrix[currentTile][13] = count - 3;                                // 1
                            _trianglesMatrix[currentTile][14] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            _trianglesMatrix[currentTile][15] = (_countersMatrix[x][z - 1] + 1) * 4 - 2;   // 2 from tile down
                            _trianglesMatrix[currentTile][16] = count - 3;                                // 1
                            _trianglesMatrix[currentTile][17] = (_countersMatrix[x][z - 1] + 1) * 4 - 1;   // 3 from tile down
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
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 100;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        _countersMatrix[x][z] = currentTile;
                        _indexesList.Add(x);
                        _indexesList.Add(z);

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
                        _verticesMatrix[currentTile][0].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][0].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][0].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][0] = _verticesMatrix[currentTile][0];
                        //bottom right vertex
                        _verticesMatrix[currentTile][1].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][1].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][1].z = _mapPositions[x][z].z - 0.005f;
                        _tiledMapVertices[x][z][1] = _verticesMatrix[currentTile][1];
                        //top left vertex
                        _verticesMatrix[currentTile][2].x = _mapPositions[x][z].x - 0.005f;
                        _verticesMatrix[currentTile][2].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][2].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][2] = _verticesMatrix[currentTile][2];
                        //top right vertex
                        _verticesMatrix[currentTile][3].x = _mapPositions[x][z].x + 0.005f;
                        _verticesMatrix[currentTile][3].y = mapTilesInfluence[x][z] / 50;
                        _verticesMatrix[currentTile][3].z = _mapPositions[x][z].z + 0.005f;
                        _tiledMapVertices[x][z][3] = _verticesMatrix[currentTile][3];

                        //current tile saved in the matrix of integers, to access the vertices
                        _countersMatrix[x][z] = currentTile;
                        _indexesList.Add(x);
                        _indexesList.Add(z);
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
                if(_indexesList.Count != 0) _counterIndexes.Add(_indexesList);
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
        ReturnPeaks();
        if (!gaussianCalculation)
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
        for(int i=0; i< 150; i++)
        {
            for(int j=0; j< 150; j++)
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
    }

    //creates mesh
    private void CreateMesh()
    {
        _mesh = new Mesh();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();
        if(_obj.GetComponent<MeshFilter>() == null) _obj.AddComponent<MeshFilter>();
        if(_obj.GetComponent<MeshRenderer>() == null) _obj.AddComponent<MeshRenderer>();
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
        _extraVerticesCounter = new List<int>[150][];
        _extraVertices = new List<Vector3>[150][];
        _pathFinderChecked = new bool[150][];
        if (_obj.GetComponent<MeshFilter>()!=null)
        {
            _obj.GetComponent<MeshFilter>().mesh = _mesh;
        }
        //influence depending on how many neighbouring "tiles" will be affected
        mapTilesInfluence = new float[150][];
        for (int m = 0; m < mapTilesInfluence.Length; m++)
        {
            mapTilesInfluence[m] = new float[150];
            _extraVerticesCounter[m] = new List<int>[150];
            _extraVertices[m] = new List<Vector3>[150];
            _pathFinderChecked[m] = new bool[150];
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
        
        _tiledMapVertices = new Vector3[150][][];
        gaussCoef.matrixRowLength = halfLengthOfNeighbourhood * 2 + 1;
        gaussCoef.floorTileCounter = gaussCoef.matrixRowLength * gaussCoef.matrixRowLength;
        gaussCoef.gaussianPositionMatrix = new Vector3[gaussCoef.matrixRowLength][];
        gaussCoef.valuesCalculated = false;
        peaks = new List<Vector3>();

        _additionalVertices = new List<Vector3>();
        _additionalTriangles = new List<List<int>>();
        _additionalVerticesColor = new List<Color>();
        _peaksPosition = new List<Vector3>();
        for (int i=20; i<clusterColors.Count; i++)
        {
            clusterColors.RemoveAt(i);
        }
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
        if(x<149)
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
        if(z<149)
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
        List<int> plateauList = new List<int>();
        for (int p=0; p< 150; p++)
        {
            for(int r=0; r< 150; r++)
            {
                if(_verticesMaximumMatrix[p][r].y>0)
                {
                    if(!gaussianCalculation)
                    {
                        plateauList = new List<int>();
                        plateauList.Add(_countersMatrix[p][r]);
                        _isPeak[p][r] = IteratePlateauAround(plateauList, p, r);
                        if (!_isPeak[p][r] && plateauList.Count>0)
                        {
                            foreach(int q in plateauList)
                            {
                                _isPeak[_counterIndexes[q][0]][_counterIndexes[q][1]] = false;
                            }
                        }
                    }
                    else
                    {// vertex 0 is peak
                        if (_tiledMapVertices[p][r][0].y > _tiledMapVertices[p][r][1].y && _tiledMapVertices[p][r][0].y > _tiledMapVertices[p][r][2].y && 
                           _tiledMapVertices[p][r][0].y > _tiledMapVertices[p - 1][r][0].y && _tiledMapVertices[p][r][0].y > _tiledMapVertices[p - 1][r][3].y &&
                           _tiledMapVertices[p][r][0].y > _tiledMapVertices[p - 1][r - 1][1].y && _tiledMapVertices[p][r][0].y > _tiledMapVertices[p - 1][r - 1][2].y &&
                           _tiledMapVertices[p][r][0].y > _tiledMapVertices[p][r - 1][0].y && _tiledMapVertices[p][r][0].y > _tiledMapVertices[p][r - 1][3].y)
                        {
                            _isPeak[p][r] = true;

                            if (_tiledMapVertices[p][r][0].y < _tiledMapVertices[p - 1][r - 1][0].y && !Is12Tilted(_tiledMapVertices[p - 1][r - 1])) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][0].y < _tiledMapVertices[p - 1][r][2].y) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][0].y < _tiledMapVertices[p][r - 1][1].y) _isPeak[p][r] = false;
                        }
                        // vertex 2 is peak
                        else if
                          (_tiledMapVertices[p][r][2].y > _tiledMapVertices[p][r][0].y && _tiledMapVertices[p][r][2].y > _tiledMapVertices[p][r][3].y &&
                           _tiledMapVertices[p][r][2].y > _tiledMapVertices[p - 1][r][1].y && _tiledMapVertices[p][r][2].y > _tiledMapVertices[p - 1][r][2].y &&
                           _tiledMapVertices[p][r][2].y > _tiledMapVertices[p - 1][r + 1][0].y && _tiledMapVertices[p][r][2].y > _tiledMapVertices[p - 1][r + 1][3].y &&
                           _tiledMapVertices[p][r][2].y > _tiledMapVertices[p][r + 1][1].y && _tiledMapVertices[p][r][2].y > _tiledMapVertices[p][r + 1][2].y)
                        {
                            _isPeak[p][r] = true;
                            if (_tiledMapVertices[p][r][2].y < _tiledMapVertices[p][r][1].y) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][2].y < _tiledMapVertices[p - 1][r][0].y && !Is12Tilted(_tiledMapVertices[p - 1][r])) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][2].y < _tiledMapVertices[p - 1][r + 1][2].y) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][2].y < _tiledMapVertices[p][r + 1][3].y && !Is12Tilted(_tiledMapVertices[p][r + 1])) _isPeak[p][r] = false;
                        }
                        // vertex 1 is peak
                        else if
                          (_tiledMapVertices[p][r][1].y > _tiledMapVertices[p][r][0].y && _tiledMapVertices[p][r][1].y > _tiledMapVertices[p][r][3].y &&
                           _tiledMapVertices[p][r][1].y > _tiledMapVertices[p][r - 1][1].y && _tiledMapVertices[p][r][1].y > _tiledMapVertices[p][r - 1][2].y &&
                           _tiledMapVertices[p][r][1].y > _tiledMapVertices[p + 1][r - 1][0].y && _tiledMapVertices[p][r][1].y > _tiledMapVertices[p + 1][r - 1][3].y &&
                           _tiledMapVertices[p][r][1].y > _tiledMapVertices[p + 1][r][1].y && _tiledMapVertices[p][r][1].y > _tiledMapVertices[p + 1][r][2].y)
                        {
                            _isPeak[p][r] = true;

                            if (_tiledMapVertices[p][r][1].y < _tiledMapVertices[p][r - 1][0].y && !Is12Tilted(_tiledMapVertices[p][r - 1])) _isPeak[p][r] = false;
                            if(_tiledMapVertices[p][r][1].y < _tiledMapVertices[p + 1][ r - 1][1].y) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][1].y < _tiledMapVertices[p + 1][r][3].y && !Is12Tilted(_tiledMapVertices[p + 1][r])) _isPeak[p][r] = false;
                        }
                        // vertex 3 is peak
                        else if
                          (_tiledMapVertices[p][r][3].y > _tiledMapVertices[p][r][1].y && _tiledMapVertices[p][r][3].y > _tiledMapVertices[p][r][2].y &&
                           _tiledMapVertices[p][r][3].y > _tiledMapVertices[p + 1][r][0].y && _tiledMapVertices[p][r][3].y > _tiledMapVertices[p + 1][r][3].y &&
                           _tiledMapVertices[p][r][3].y > _tiledMapVertices[p + 1][r + 1][1].y && _tiledMapVertices[p][r][3].y > _tiledMapVertices[p + 1][r + 1][2].y &&
                           _tiledMapVertices[p][r][3].y > _tiledMapVertices[p][r + 1][0].y && _tiledMapVertices[p][r][3].y > _tiledMapVertices[p][r + 1][3].y)
                        {
                            _isPeak[p][r] = true;

                            if (_tiledMapVertices[p][r][3].y < _tiledMapVertices[p + 1][r][1].y) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][3].y < _tiledMapVertices[p + 1][r + 1][3].y && !Is12Tilted(_tiledMapVertices[p + 1][r + 1])) _isPeak[p][r] = false;
                            if (_tiledMapVertices[p][r][3].y < _tiledMapVertices[p][r + 1][2].y) _isPeak[p][r] = false;
                        }
                        else _isPeak[p][r] = false;
                    }
                    
                }
                /*set the peak color to white
                if (_isPeak[p][r] && _verticesMaximumMatrix[p][r].y + 0.002f > threshold)
                {
                    var pos = _obj.transform.TransformPoint(_verticesMaximumMatrix[p][r]);
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pos.x -= 0.581f;
                    pos.y += 0.002f;
                    pos.z -= 0.63f;
                    obj.transform.localScale = new Vector3(0.00002f, 0.00002f, 0.00002f);
                    obj.transform.position = pos;
                    for (int c = 0; c < 4; c++)
                    {
                        _tiledMapColors[p][r][c] = new Color(1, 1, 1);
                    }
                }*/
            }
        }

        for(int i=0; i< 150; i++)
        {
            for(int j=0; j< 150; j++)
            {
                if (_isPeak[i][j] && _verticesMaximumMatrix[i][j].y + 0.002f > threshold)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        _tiledMapColors[i][j][c] = new Color(1, 1, 1);
                    }
                }
            }
        }
    }

    //when vertices 1 and 2 are smaller than vertices 0 and 3
    private bool Is12Tilted(Vector3[] tile)
    {
        if (tile[1].y < tile[0].y && tile[1].y < tile[3].y &&
            tile[2].y < tile[0].y && tile[2].y < tile[3].y) return true;
        return false;
    }

    private void SingleCenteredSquaredWaveClusters()
    {
        Vector3 peakPos = new Vector3();
        colorCounter = 0;
        _peaksPosition = new List<Vector3>();
        if(clusterColors.Count > 20)
        {
            for(int c = clusterColors.Count - 1; c>=20; c--)
            {
                clusterColors.Remove(clusterColors[c]);
            }
        }
        ResetMesh();
        List<int> mainList = new List<int>();
        for (int a = 0; a < 150; a++)
        {
            for (int b = 0; b < 150; b++)
            {
                _clustered[a][b] = false;
            }
        }
        for (int i=0; i< 150; i++)
        {
            for(int j=0; j< 150; j++)
            {
                if(_isPeak[i][j] && !_clustered[i][j] && (_verticesMaximumMatrix[i][j].y + 0.002f) > threshold )
                {
                    //list for peaks' position
                    peakPos = (_tiledMapVertices[i][j][0] + _tiledMapVertices[i][j][1] + _tiledMapVertices[i][j][2] + _tiledMapVertices[i][j][3]) / 4;
                    _peaksPosition.Add(peakPos);
                    a = 1;
                    b = 1;
                    c = 1;
                    d = 1;
                    mainList = new List<int>();
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        if (!clusterColors.Contains(_clusterColor)) clusterColors.Add(_clusterColor);
                        while (!clusterColors.Contains(_clusterColor))
                        {
                            _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                            clusterColors.Add(_clusterColor);
                        }
                    }
                    mainList.Add(_countersMatrix[i][j]);

                    for(int x=i-1; x<=i+1; x++)
                    {
                        for(int y=j-1; y<= j+1; y++)
                        {
                            if (!_clustered[x][y] && _verticesMaximumMatrix[x][y].y + 0.002f > threshold)
                            {
                                if (x == i && y == j) continue;
                                mainList.Add(_countersMatrix[x][y]);
                            }
                        }
                    }
                    mainList = SortListByHeight(mainList);
                    for(int l=0; l<mainList.Count; l++)
                    {
                        int x = _counterIndexes[mainList[l]][0];
                        int y = _counterIndexes[mainList[l]][1];
                        IterateSingleCenteredSquaredWave(mainList, x, y, a, b, c, d);
                        ColorTiles(x, y, _clusterColor);
                    }
                    colorCounter++;
                }
            }
        }
        for(int i=0; i<150; i++)
        {
            for(int j=0; j<150; j++)
            {
                if ((_verticesMaximumMatrix[i][j].y + 0.002f) > threshold && !_pathFinderChecked[i][j])
                {
                    List<int> list = new List<int>();
                    _pathFinderChecked[i][j] = true;
                    bool peakReached = false;
                    list.Add(_countersMatrix[i][j]);
                    for(int r=0; r<list.Count; r++)
                    {
                        peakReached = PeakReached(_counterIndexes[list[r]][0], _counterIndexes[list[r]][1], list);
                        if (peakReached) break;
                    }
                    if (!peakReached)
                    {
                        foreach(var tile in list)
                        {
                            int m = _counterIndexes[tile][0];
                            int n = _counterIndexes[tile][1];
                            bool toBreak = false;
                            for(int m1 = m-1; m1<= m+1; m1++)
                            {
                                for(int n1 = n-1; n1 <= n+1; n1++)
                                {
                                    if (m != m1 && n != n1) continue;
                                    if(_verticesMaximumMatrix[m][n].y <= _verticesMaximumMatrix[m1][n1].y &&
                                        _additionalVerticesColor[tile*4] != _additionalVerticesColor[_countersMatrix[m1][n1] * 4])
                                    {
                                        ColorTiles(m, n, _additionalVerticesColor[_countersMatrix[m1][n1] * 4]);
                                        toBreak = true;
                                        break;
                                    }
                                }
                                if (toBreak) break;
                            }
                        }
                    }
                }
            }
        }
    }
    //path finder from tile to peak
    private bool PeakReached(int i, int j, List<int> list)
    {
        bool peakReached = false;
        int countIJ = _countersMatrix[i][j];
        int countKL;
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                countKL = _countersMatrix[k][l];
                if (_isPeak[k][l])
                {
                    peakReached = true;
                    return true;
                }
                else if (_verticesMaximumMatrix[k][l].y + 0.002f < threshold) continue;
                else if (k == i && l == j) continue;
                else if (list.Contains(countKL)) continue;
                else if (_additionalVerticesColor[countKL * 4] != _additionalVerticesColor[countIJ * 4]) continue;
                else
                {
                    _pathFinderChecked[k][l] = true;
                    list.Add(_countersMatrix[k][l]);
                }
            }
        }
        return peakReached;
    }

    //sorts and returns a list of tiles by their height descending
    private List<int> SortListByHeight(List<int> list)
    {
        list.Sort((a, b) => _verticesMaximumMatrix[_counterIndexes[b][0]][_counterIndexes[b][1]].y.CompareTo(_verticesMaximumMatrix[_counterIndexes[a][0]][_counterIndexes[a][1]].y));
        return list;
    }

    private void IterateSingleCenteredSquaredWave(List<int> list,int i, int j, int a1, int b1, int c1, int d1)
    {
        List<int> currentNeighbours = new List<int>();
        Vector3 tilePos = new Vector3();
        Color tileColor = new Color();
        //check for walls first
        for(int k=i-a1; k<=i+b1; k++)
        {
            for(int l=j-c1; l<=j+d1; l++)
            {
                if (_verticesMaximumMatrix[i][j].y < _verticesMaximumMatrix[k][l].y && _additionalVerticesColor[_countersMatrix[k][l] * 4 + 1] != _clusterColor)
                {
                    if (k < i)
                    {
                        a1 = 0;
                    }
                    else if (k > i)
                    {
                        b1 = 0;
                    }
                    else if (l < j)
                    {
                        c1 = 0;
                    }
                    else if (l > j)
                    {
                        d1 = 0;
                    }
                }
            }
        }

        for(int k=i-a1; k<=i+b1; k++)
        {
            for(int l = j-c1; l<= j+d1; l++)
            {
                _tile0 = _countersMatrix[k][l] * 4;
                _tile1 = _countersMatrix[k][l] * 4 + 1;
                _tile2 = _countersMatrix[k][l] * 4 + 2;
                _tile3 = _countersMatrix[k][l] * 4 + 3;
                if ((_tiledMapVertices[k][l][0].y + 0.002f) < threshold) continue;
                else if(_verticesMaximumMatrix[i][j].y >= _verticesMaximumMatrix[k][l].y)
                {
                    if (!list.Contains(_countersMatrix[k][l]))
                    {
                        //if already clustered, check for distance between previous and current cluster peak
                        if (_clustered[k][l] && _additionalVerticesColor[_countersMatrix[k][l] * 4 + 1]!=_clusterColor)
                        {
                            tilePos = (_tiledMapVertices[k][l][0] + _tiledMapVertices[k][l][1] + _tiledMapVertices[k][l][2] + _tiledMapVertices[k][l][3]) / 4;
                            tileColor = _additionalVerticesColor[_countersMatrix[k][l] * 4 + 1];
                            int counter = 0;
                            foreach (var color in clusterColors)
                            {
                                if (tileColor == color) break;
                                counter++;
                            }
                            //2D representations of positions with X and Z coords
                            Vector2 _tilePos2 = new Vector2(tilePos.x, tilePos.z);
                            Vector2 _currentPeak = new Vector2(_peaksPosition[_peaksPosition.Count - 1].x, _peaksPosition[_peaksPosition.Count - 1].z);
                            Vector2 _tilePrevPeak = new Vector2(_peaksPosition[counter].x, _peaksPosition[counter].z);
                            if (Vector2.Distance(_tilePos2, _currentPeak) < Vector2.Distance(_tilePos2, _tilePrevPeak))
                            {
                                currentNeighbours.Add(_countersMatrix[k][l]);
                                foreach (var item in _extraVerticesCounter[k][l])
                                {
                                    _additionalVerticesColor[item] = _clusterColor;
                                }
                            }
                        }
                        else
                        {
                            currentNeighbours.Add(_countersMatrix[k][l]);
                        }
                    }
                }
            }
        }
        currentNeighbours = SortListByHeight(currentNeighbours);
        for(int n=0; n<currentNeighbours.Count; n++)
        {
            for(int q=0; q<list.Count; q++)
            {
                if(_verticesMaximumMatrix[_counterIndexes[currentNeighbours[n]][0]][_counterIndexes[currentNeighbours[n]][1]].y >
                    _verticesMaximumMatrix[_counterIndexes[list[q]][0]] [_counterIndexes[list[q]][1]].y)
                {
                    list.Insert(q, currentNeighbours[n]);
                    break;
                }
            }
            if (!list.Contains(currentNeighbours[n])) list.Add(currentNeighbours[n]);
        }
    }

    //Function for coloring vertices of tiles and updates triangles if needed
    void ColorTiles(int k, int l, Color color)
    {
        _extraVerticesCounter[k][l] = new List<int>();
        _extraVertices[k][l] = new List<Vector3>();
        Vector3 peakPos = new Vector3();
        _clustered[k][l] = true;
        if(_isPeak[k][l])
        {
            //calculate the current center of the peak, if combined from few tiles
            peakPos = (_tiledMapVertices[k][l][0] + _tiledMapVertices[k][l][1] + _tiledMapVertices[k][l][2] + _tiledMapVertices[k][l][3]) / 4;
            peakPos = (peakPos + _peaksPosition[_peaksPosition.Count - 1]) / 2;
            _peaksPosition[_peaksPosition.Count - 1] = peakPos;
        }
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

        #region border tiles with different colors
        //paint left wall if left tile is clustered
        if(_clustered[k-1][l] && _additionalVerticesColor[_leftTile1] != color)
        {
            if(_pos0.y > _leftPos1.y)
            {
                Vector3 tempVector = new Vector3(_leftPos1.x + 0.581f, _leftPos1.y - 0.002f, _leftPos1.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_leftPos3.x + 0.581f, _leftPos3.y - 0.002f, _leftPos3.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(color);
                _additionalVerticesColor.Add(color);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_tile0);
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_tile2);
                _triangles[_countersMatrix[k][l] * 18 + 6] = _tile0;
                _triangles[_countersMatrix[k][l] * 18 + 7] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l] * 18 + 8] = _tile2;

                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_tile2);
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_additionalVertices.Count - 1);
                _triangles[_countersMatrix[k][l] * 18 + 9] = _tile2;
                _triangles[_countersMatrix[k][l] * 18 + 10] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l] * 18 + 11] = _additionalVertices.Count - 1;
                //_additionalTriangles.Add(temp);
            }
            else
            {
                Vector3 tempVector = new Vector3(_pos0.x + 0.581f, _pos0.y - 0.002f, _pos0.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_pos2.x + 0.581f, _pos2.y - 0.002f, _pos2.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(_additionalVerticesColor[_leftTile1]);
                _additionalVerticesColor.Add(_additionalVerticesColor[_leftTile3]);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_leftTile1);
                temp.Add(_additionalVertices.Count - 1);
                _triangles[_countersMatrix[k][l] * 18 + 6] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l] * 18 + 7] = _leftTile1;
                _triangles[_countersMatrix[k][l] * 18 + 8] = _additionalVertices.Count - 1;
                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_leftTile1);
                temp.Add(_leftTile3);
                _triangles[_countersMatrix[k][l] * 18 + 9] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l] * 18 + 10] = _leftTile1;
                _triangles[_countersMatrix[k][l] * 18 + 11] = _leftTile3;
                //_additionalTriangles.Add(temp);
            }
        }

        //paint down wall if down tile is clustered
        if(_clustered[k][l - 1] && _additionalVerticesColor[_downTile2] != color)
        {
            if(_pos0.y > _downPos2.y)
            {
                Vector3 tempVector = new Vector3(_downPos2.x + 0.581f, _downPos2.y - 0.002f, _downPos2.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_downPos3.x + 0.581f, _downPos3.y - 0.002f, _downPos3.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(color);
                _additionalVerticesColor.Add(color);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_tile0);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_additionalVertices.Count - 2);
                _triangles[_countersMatrix[k][l] * 18 + 12] = _tile0;
                _triangles[_countersMatrix[k][l] * 18 + 13] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l] * 18 + 14] = _additionalVertices.Count - 2;
                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_tile1);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_tile0);
                _triangles[_countersMatrix[k][l] * 18 + 15] = _tile1;
                _triangles[_countersMatrix[k][l] * 18 + 16] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l] * 18 + 17] = _tile0;
                //_additionalTriangles.Add(temp);
            }
            else
            {
                Vector3 tempVector = new Vector3(_pos0.x + 0.581f, _pos0.y - 0.002f, _pos0.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_pos1.x + 0.581f, _pos1.y - 0.002f, _pos1.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(_additionalVerticesColor[_downTile2]);
                _additionalVerticesColor.Add(_additionalVerticesColor[_downTile3]);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_downTile2);
                _triangles[_countersMatrix[k][l] * 18 + 12] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l] * 18 + 13] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l] * 18 + 14] = _downTile2;
                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_downTile3);
                temp.Add(_downTile2);
                _triangles[_countersMatrix[k][l] * 18 + 15] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l] * 18 + 16] = _downTile3;
                _triangles[_countersMatrix[k][l] * 18 + 17] = _downTile2;
                //_additionalTriangles.Add(temp);
            }
        }

        //paint right wall if right tile is clustered
        if(_clustered[k + 1][l] && _additionalVerticesColor[_rightTile0] != color)
        {
            if(_pos1.y > _rightPos0.y)
            {
                Vector3 tempVector = new Vector3(_rightPos0.x + 0.581f, _rightPos0.y - 0.002f, _rightPos0.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_rightPos2.x + 0.581f, _rightPos2.y - 0.002f, _rightPos2.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(color);
                _additionalVerticesColor.Add(color);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_tile1);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_additionalVertices.Count - 2);
                //_additionalTriangles.Add(temp);
                _triangles[_countersMatrix[k + 1][l] * 18 + 6] = _tile1;
                _triangles[_countersMatrix[k + 1][l] * 18 + 7] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k + 1][l] * 18 + 8] = _additionalVertices.Count - 2;
                temp = new List<int>();
                temp.Add(_tile3);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_tile1);
                _triangles[_countersMatrix[k + 1][l] * 18 + 9] = _tile3;
                _triangles[_countersMatrix[k + 1][l] * 18 + 10] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k + 1][l] * 18 + 11] = _tile1;
                //_additionalTriangles.Add(temp);
            }
            else
            {
                Vector3 tempVector = new Vector3(_pos1.x + 0.581f, _pos1.y - 0.002f, _pos1.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_pos3.x + 0.581f, _pos3.y - 0.002f, _pos3.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(_additionalVerticesColor[_rightTile0]);
                _additionalVerticesColor.Add(_additionalVerticesColor[_rightTile2]);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_rightTile0);
                //_additionalTriangles.Add(temp);
                _triangles[_countersMatrix[k + 1][l] * 18 + 6] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k + 1][l] * 18 + 7] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k + 1][l] * 18 + 8] = _rightTile0;
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_rightTile2);
                temp.Add(_rightTile0);
                _triangles[_countersMatrix[k + 1][l] * 18 + 9] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k + 1][l] * 18 + 10] = _rightTile2;
                _triangles[_countersMatrix[k + 1][l] * 18 + 11] = _rightTile0;
                //_additionalTriangles.Add(temp);
            }
        }
        if(_clustered[k][l + 1] && _additionalVerticesColor[_upTile0] != color)
        {
            if (_pos2.y > _upPos0.y)
            {
                Vector3 tempVector = new Vector3(_upPos0.x + 0.581f, _upPos0.y - 0.002f, _upPos0.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_upPos1.x + 0.581f, _upPos1.y - 0.002f, _upPos1.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(color);
                _additionalVerticesColor.Add(color);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_tile2);
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_additionalVertices.Count - 1);
                _triangles[_countersMatrix[k][l + 1] * 18 + 12] = _tile2;
                _triangles[_countersMatrix[k][l + 1] * 18 + 13] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l + 1] * 18 + 14] = _additionalVertices.Count - 1;
                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_tile2);
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_tile3);
                _triangles[_countersMatrix[k][l + 1] * 18 + 15] = _tile2;
                _triangles[_countersMatrix[k][l + 1] * 18 + 16] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l + 1] * 18 + 17] = _tile3;
                //_additionalTriangles.Add(temp);
            }
            else
            {
                Vector3 tempVector = new Vector3(_pos2.x + 0.581f, _pos2.y - 0.002f, _pos2.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                tempVector = new Vector3(_pos3.x + 0.581f, _pos3.y - 0.002f, _pos3.z + 0.63f);
                _additionalVertices.Add(tempVector);
                //save vertex position
                _extraVertices[k][l].Add(tempVector);
                _additionalVerticesColor.Add(_additionalVerticesColor[_upTile0]);
                _additionalVerticesColor.Add(_additionalVerticesColor[_upTile1]);
                //save counter of color for additional vertices, for later changes
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 2);
                _extraVerticesCounter[k][l].Add(_additionalVerticesColor.Count - 1);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_additionalVertices.Count - 2);
                temp.Add(_upTile0);
                _triangles[_countersMatrix[k][l + 1] * 18 + 12] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l + 1] * 18 + 13] = _additionalVertices.Count - 2;
                _triangles[_countersMatrix[k][l + 1] * 18 + 14] = _upTile0;
                //_additionalTriangles.Add(temp);
                temp = new List<int>();
                temp.Add(_additionalVertices.Count - 1);
                temp.Add(_upTile0);
                temp.Add(_upTile1);
                _triangles[_countersMatrix[k][l + 1] * 18 + 15] = _additionalVertices.Count - 1;
                _triangles[_countersMatrix[k][l + 1] * 18 + 16] = _upTile0;
                _triangles[_countersMatrix[k][l + 1] * 18 + 17] = _upTile1;
                //_additionalTriangles.Add(temp);
            }
        }

        #endregion

        #region end of cluster
        //if current tile is higher than left tile that is lower than threshold
        if (_pos0.y > _leftPos1.y && _leftPos1.y <= threshold)
        {
            if (Physics.Raycast(_pos0, _leftPos1 - _pos0, out _hit, 5f))
            {
                vertex = _hit.point;
                vertex.x += 0.581f;
                vertex.y -= 0.002f;
                vertex.z += 0.63f;
                _additionalVertices.Add(vertex); // -4
                _additionalVertices.Add(vertex); // -3
                _additionalVerticesColor.Add(color);
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
                _additionalVerticesColor.Add(color);
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

        //if current tile is higher than down tile
        if (_pos0.y > _downPos2.y && _downPos2.y <= threshold)
        {
            if (Physics.Raycast(_pos0, _downPos2 - _pos0, out _hit, 5f))
            {
                vertex = _hit.point;
                vertex.x += 0.581f;
                vertex.y -= 0.002f;
                vertex.z += 0.63f;
                _additionalVertices.Add(vertex); // -4
                _additionalVertices.Add(vertex); // -3
                _additionalVerticesColor.Add(color);
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
                _additionalVerticesColor.Add(color);
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

            for (int t = 12; t < 18; t++)
            {
                _triangles[_countersMatrix[k][l] * 18 + t] = 0;
            }
        }
        
        //if current tile is higher than right tile
        if (_pos1.y > _rightPos0.y && _rightPos0.y <= threshold)
        {
            if (Physics.Raycast(_pos1, _rightPos0 - _pos1, out _hit, 5f))
            {
                vertex = _hit.point;
                vertex.x += 0.581f;
                vertex.y -= 0.002f;
                vertex.z += 0.63f;
                _additionalVertices.Add(vertex); // -4
                _additionalVertices.Add(vertex); // -3
                _additionalVerticesColor.Add(color);
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
                _additionalVerticesColor.Add(color);
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

            for (int t = 6; t < 12; t++)
            {
                _triangles[_countersMatrix[k + 1][l] * 18 + t] = 0;
            }
        }

        //if current tile is higher than up tile
        if (_pos2.y > _upPos0.y && _upPos0.y <= threshold)
        {
            if (Physics.Raycast(_pos2, _upPos0 - _pos2, out _hit, 5f))
            {
                vertex = _hit.point;
                vertex.x += 0.581f;
                vertex.y -= 0.002f;
                vertex.z += 0.63f;
                _additionalVertices.Add(vertex); // -4
                _additionalVertices.Add(vertex); // -3
                _additionalVerticesColor.Add(color);
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
                _additionalVerticesColor.Add(color);
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
                _triangles[_countersMatrix[k][l + 1] * 18 + t] = 0;
            }
        }
        #endregion
        _additionalVerticesColor[_tile0] = color;
        _additionalVerticesColor[_tile1] = color;
        _additionalVerticesColor[_tile2] = color;
        _additionalVerticesColor[_tile3] = color;
        _clustered[k][l] = true;
    }

    private void SingleCenteredGaussianClusters()
    {
        Vector3 peakPos = new Vector3();
        _peaksPosition = new List<Vector3>();
        colorCounter = 0;
        if (clusterColors.Count > 20)
        {
            for (int c = clusterColors.Count - 1; c >= 20; c--)
            {
                clusterColors.Remove(clusterColors[c]);
            }
        }
        ResetMesh();
        List<int> mainList = new List<int>();

        for (int a = 0; a < 150; a++)
        {
            for (int b = 0; b < 150; b++)
            {
                _clustered[a][b] = false;
            }
        }
        for(int i=0; i<150; i++)
        {
            for(int j=0; j<150; j++)
            {
                if(_isPeak[i][j] && !Is12Tilted(_tiledMapVertices[i][j]) && (_verticesMaximumMatrix[i][j].y + 0.002f)>threshold)
                {
                    peakPos = _verticesMaximumMatrix[i][j];
                    _peaksPosition.Add(peakPos);
                    a = 1;
                    b = 1;
                    c = 1;
                    d = 1;

                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        if (!clusterColors.Contains(_clusterColor)) clusterColors.Add(_clusterColor);
                        while (!clusterColors.Contains(_clusterColor))
                        {
                            _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                            clusterColors.Add(_clusterColor);
                        }
                    }
                    mainList.Add(_countersMatrix[i][j]);
                    for(int x=i-1; x<=i+1; x++)
                    {
                        for(int y=j-1; y<=j+1; y++)
                        {
                            if (!_clustered[x][y] && _verticesMaximumMatrix[x][y].y + 0.002f > threshold)
                            {
                                if (x == i && y == j) continue;
                                else if (Is12Tilted(_tiledMapVertices[x][y])) continue;
                                mainList.Add(_countersMatrix[x][y]);
                            }
                        }
                    }
                    for(int l=0; l<mainList.Count; l++)
                    {
                        int x = _counterIndexes[mainList[l]][0];
                        int y = _counterIndexes[mainList[l]][1];
                        IterateSingleCenteredGaussian(mainList, x, y, a, b, c, d);
                        /////////////////////////////////////////////////////// continue here for coloring maybe.. or maybe color in iteratesinglecenteredgaussian
                    }
                    colorCounter++;
                }
            }
        }
    }

    private void IterateSingleCenteredGaussian(List<int> list, int i, int j, int a1, int b1, int c1, int d1)
    {
        for(int k=i-1; k<=i+1; k++)
        {
            for(int l=j-1; l<=j+1; l++)
            {
                //if the tile is not tilted in half
                if(!Is12Tilted(_tiledMapVertices[k][l]))
                {
                    //if they are not connected with the previous tile, skip
                    if (NotNeighbors(_tiledMapVertices[i][j], _tiledMapVertices[k][l])) continue;
                    //if below the threshold or already clustered, continue
                    else if ((_verticesMaximumMatrix[k][l].y + 0.002f) < threshold || _clustered[k][l]) continue;
                    //add to the list if it is not end of cluster or it is not already in the list
                    else if(!EndOfCluster(_tiledMapVertices[i][j], _tiledMapVertices[k][l]))
                    {
                        ////dodaj obojuvanje tukaaaaaaaaaaaaa!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        if(!list.Contains(_countersMatrix[k][l])) list.Add(_countersMatrix[k][l]);
                    }
                }
            }
        }
    }

    //Stop searching if second tile goes up from first (end of cluster)
    private bool EndOfCluster(Vector3[] currentTile, Vector3[] neighbourTile)
    {
        //left tile
        if(currentTile[0] == neighbourTile[1] && currentTile[2] == neighbourTile[3])if (neighbourTile[0].y > currentTile[0].y) return true;
        //right tile
        else if (currentTile[1] == neighbourTile[0] && currentTile[3] == neighbourTile[2]) if (neighbourTile[3].y > currentTile[3].y) return true;
        //down tile
        else if(currentTile[0] == neighbourTile[2] && currentTile[1] == neighbourTile[3]) if (neighbourTile[0].y > currentTile[0].y) return true;
        //up tile
        else if (currentTile[2] == neighbourTile[0] && currentTile[3] == neighbourTile[1]) if (neighbourTile[3].y > currentTile[3].y) return true;
        //up-left tile
        else if (currentTile[2] == neighbourTile[1]) if (neighbourTile[0].y > currentTile[2].y && neighbourTile[3].y > currentTile[2].y) return true;
        //up-right tile
        else if (currentTile[3] == neighbourTile[0]) if (neighbourTile[1].y > currentTile[3].y && neighbourTile[2].y > currentTile[3].y) return true;
        //down-left tile
        else if (currentTile[0] == neighbourTile[3]) if (neighbourTile[1].y > currentTile[0].y && neighbourTile[2].y > currentTile[0].y) return true;
        //down-right tile
        else if (currentTile[1] == neighbourTile[2]) if (neighbourTile[0].y > currentTile[1].y && neighbourTile[3].y > currentTile[1].y) return true;

        return false;
    }

    //Recursive function that detects plateau regions
    bool IteratePlateauAround(List<int> plateauList, int p, int r)
    {
        bool tileIsPeak = false;
        for (int i = p-1; i<p+2; i++)
        {
            for(int j=r-1; j<r+2; j++)
            {
                if (plateauList.Contains(_countersMatrix[i][j])) continue;
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
                    plateauList.Add(_countersMatrix[i][j]);
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
        colorCounter = 0;
        ResetMesh();
        for (int a = 0; a < 150; a++)
        {
            for (int b = 0; b < 150; b++)
            {
                _clustered[a][b] = false;
            }
        }

        for (int i=0; i<150; i++)
        {
            for(int j=0; j< 150; j++)
            {
                _tile0 = _countersMatrix[i][j] * 4;
                _tile1 = _countersMatrix[i][j] * 4 + 1;
                _tile2 = _countersMatrix[i][j] * 4 + 2;
                _tile3 = _countersMatrix[i][j] * 4 + 3;

                //if already belongs to cluster or if the maximum point is below the threshold, continue
                if (_clustered[i][j] || (_verticesMaximumMatrix[i][j].y + 0.002f < threshold)) continue;

                if (AllVerticesAbove(_tiledMapVertices[i][j], threshold))
                {
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        clusterColors.Add(_clusterColor);
                    }
                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    _clustered[i][j] = true;
                    IterateMultiCenterGaussianClusterAround(i, j);
                    colorCounter++;
                }
                else if (ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], _tiledMapVertices[i][j][2], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], _tiledMapVertices[i][j][3], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold) ||
                         ThreeVerticesAbove(_tiledMapVertices[i][j][1], _tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold))
                {
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        clusterColors.Add(_clusterColor);
                    }
                    IterateMultiCenterGaussianClusterAround(i, j);
                    colorCounter++;
                }

                else if (TwoVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][1], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][2], _tiledMapVertices[i][j][3], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][0], _tiledMapVertices[i][j][2], threshold) ||
                         TwoVerticesAbove(_tiledMapVertices[i][j][1], _tiledMapVertices[i][j][3], threshold))
                {
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        clusterColors.Add(_clusterColor);
                    }
                    IterateMultiCenterGaussianClusterAround(i, j);
                    colorCounter++;
                }
                else if(_tiledMapVertices[i][j][0].y + 0.002f > threshold || _tiledMapVertices[i][j][1].y + 0.002f > threshold ||
                        _tiledMapVertices[i][j][2].y + 0.002f > threshold || _tiledMapVertices[i][j][3].y + 0.002f > threshold)
                {
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        clusterColors.Add(_clusterColor);
                    }
                    IterateMultiCenterGaussianClusterAround(i, j);
                    colorCounter++;
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
        _extraVerticesCounter = new List<int>[150][];
        _extraVertices = new List<Vector3>[150][];
        _pathFinderChecked = new bool[150][];
        for(int i = 0; i < 150; i++)
        {
            _extraVerticesCounter[i] = new List<int>[150];
            _extraVertices[i] = new List<Vector3>[150];
            _pathFinderChecked[i] = new bool[150];
        }
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
        colorCounter = 0;
        ResetMesh();
        for(int a = 0; a< 150; a++)
        {
            for(int b=0; b< 150; b++)
            {
                _clustered[a][b] = false;
            }
        }
        for(int i=0; i< 150; i++)
        {
            for(int j=0; j< 150; j++)
            {
                _tile0 = _countersMatrix[i][j] * 4;
                _tile1 = _countersMatrix[i][j] * 4 + 1;
                _tile2 = _countersMatrix[i][j] * 4 + 2;
                _tile3 = _countersMatrix[i][j] * 4 + 3;

                //if tile already belongs to cluster or if it is below the threshold continue
                if (_clustered[i][j] || (_tiledMapVertices[i][j][0].y + 0.002f) < threshold) continue;
                else
                {
                    if (colorCounter < 20) _clusterColor = clusterColors[colorCounter];
                    else
                    {
                        _clusterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        clusterColors.Add(_clusterColor);
                    }
                    _additionalVerticesColor[_tile0] = _clusterColor;
                    _additionalVerticesColor[_tile1] = _clusterColor;
                    _additionalVerticesColor[_tile2] = _clusterColor;
                    _additionalVerticesColor[_tile3] = _clusterColor;
                    IterateMultiCenterSquaredWaveClusterAround(i, j);
                    colorCounter++;
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
                if (_pos0.y > _downPos2.y && _downPos2.y <= threshold)
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
                if (_pos0.y > _leftPos1.y && _leftPos1.y <= threshold)
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
                if (_pos1.y > _rightPos0.y && _rightPos0.y <= threshold)
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
                if(_pos2.y > _upPos0.y && _upPos0.y <= threshold)
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
