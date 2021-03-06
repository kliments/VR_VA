﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SilhouetteCoefficient : NetworkBehaviour {
    public bool calculate;
    public Transform dataContainer, averageLine, canvas, silhouetteObjects;
    public ClusteringAlgorithm currentAlgorithm;
    public Text text;
    public Material mat;

    private List<GameObject> _dataPoints;
    private Transform[] _list;
    private float[][] clustersValues;
    private Vector3[] _vertices;
    private Color[] _colors;
    private float _yScale;
    private float _yShift;
    private int[] _triangles;
    private Mesh _mesh;
    private GameObject _obj;
    // Use this for initialization
    void Start ()
    {
        FindAllObjects();
        _obj = new GameObject();
        _obj.transform.parent = transform;
        _obj.transform.localPosition = new Vector3(0,0,-0.007f);
        _obj.AddComponent<MeshFilter>();
        _obj.AddComponent<MeshRenderer>();
        _obj.GetComponent<MeshRenderer>().material = mat;
    }
	
	// Update is called once per frame
	void Update () {
		if(calculate)
        {
            calculate = false;
            Calculate();
        }
	}

    void FindAllObjects()
    {
        transform.parent = GameObject.Find("EventSystem").transform;
        dataContainer = GameObject.Find("ScatterplotElements").transform;
        silhouetteObjects = GameObject.Find("SilhouetteCoefficientFrame").transform;
        canvas = GameObject.Find("Canvas").transform;
        foreach (Transform child in silhouetteObjects)
        {
            if(child.name == "averageLine")
            {
                averageLine = child;
                break;
            }
        }
        foreach(Transform child in canvas)
        {
            if(child.name == "S")
            {
                text = child.GetComponent<Text>();
                break;
            }
        }
    }

    public void Calculate()
    {
        AssignData();
        text.text = "S = " + Coef();
        DrawGraph();
    }

    private float Coef()
    {
        int clusterID = 0;
        float count = 1;
        float coef = 0;
        float minAverage = 10;
        float ai, si;
        foreach(GameObject point in _dataPoints)
        {
            if (point.GetComponent<ClusterQualityValues>().clusterID == currentAlgorithm.NOISE) continue;
            minAverage = 10;
            clusterID = point.GetComponent<ClusterQualityValues>().clusterID;
            if(currentAlgorithm.GetType().Equals(typeof(KMeansAlgorithm)))
            {
                foreach (var sphere in currentAlgorithm.GetComponent<KMeansAlgorithm>().spheres)
                {
                    if (point.GetComponent<MeshRenderer>().material.color == sphere.GetComponent<MeshRenderer>().material.color)
                    {
                        point.GetComponent<ClusterQualityValues>().Ai = Vector3.Distance(point.transform.position, sphere.transform.position);
                        break;
                    }
                }

                foreach (var sphere in currentAlgorithm.GetComponent<KMeansAlgorithm>().spheres)
                {
                    if (point.GetComponent<MeshRenderer>().material.color == sphere.GetComponent<MeshRenderer>().material.color) continue;
                    if (minAverage > Vector3.Distance(point.transform.position, sphere.transform.position)) minAverage = Vector3.Distance(point.transform.position, sphere.transform.position);
                }
            }
            else
            {
                point.GetComponent<ClusterQualityValues>().Ai = AverageDistance(point, currentAlgorithm.clusters[clusterID]);
                for (int i = 0; i < currentAlgorithm.clusters.Count; i++)
                {
                    if (i == clusterID) continue;
                    if (minAverage > AverageDistance(point, currentAlgorithm.clusters[i])) minAverage = AverageDistance(point, currentAlgorithm.clusters[i]);
                }
            }
            point.GetComponent<ClusterQualityValues>().Bi = minAverage;
            point.GetComponent<ClusterQualityValues>().Si = (point.GetComponent<ClusterQualityValues>().Bi - point.GetComponent<ClusterQualityValues>().Ai) / Mathf.Max(point.GetComponent<ClusterQualityValues>().Ai, point.GetComponent<ClusterQualityValues>().Bi);
            coef += point.GetComponent<ClusterQualityValues>().Si;
            count++;
        }
        clustersValues = currentAlgorithm.SortClusters();
        return coef/count;
    }

    void AssignData()
    {
        _dataPoints = new List<GameObject>();
        _list = dataContainer.GetComponentsInChildren<Transform>(false);
        foreach (Transform child in _list)
        {
            if(child.gameObject.GetComponent<ClusterQualityValues>() != null)
            {
                _dataPoints.Add(child.gameObject);
            }
        }
    }

    float AverageDistance(GameObject point, List<GameObject> cluster)
    {
        float totalDist = 0;
        foreach (var p in cluster)
        {
            if (p == point) continue;
            totalDist += Vector3.Distance(point.transform.position, p.transform.position);
        }
        return totalDist / cluster.Count;
    }

    void DrawGraph()
    {
        int currentPoint = 0;
        Color _currentColor = new Color();
        Vector3 tempVertex = new Vector3();
        _yScale = (0.5f - (clustersValues.Length - 1) * 0.01f) / currentAlgorithm.clusteredPoints;
        _yShift = 1.7f;
        _vertices = new Vector3[currentAlgorithm.clusteredPoints * 4];
        _triangles = new int[currentAlgorithm.clusteredPoints * 6];
        _colors = new Color[currentAlgorithm.clusteredPoints * 4];
        //calculate vertices
        for(int i=0; i<clustersValues.Length; i++)
        {
            if (currentAlgorithm.clusters[i].Count == 0) continue;
            _currentColor = currentAlgorithm.clusters[i][0].GetComponent<MeshRenderer>().material.color;
            for(int j=clustersValues[i].Length -1; j>=0; j--)
            {
                #region Vertex0
                tempVertex.x = -1.5f;
                tempVertex.y = _yShift;
                tempVertex.z = 2.996f;
                _vertices[currentPoint * 4] = tempVertex;
                _colors[currentPoint * 4] = _currentColor;
                #endregion

                #region Vertex1
                tempVertex.x = -1.5f + clustersValues[i][j] * 0.4f;
                tempVertex.y = _yShift;
                tempVertex.z = 2.996f;
                _vertices[currentPoint * 4 + 1] = tempVertex;
                _colors[currentPoint * 4 + 1] = _currentColor;
                #endregion
                _yShift -= _yScale;
                #region Vertex2
                tempVertex.x = -1.5f;
                tempVertex.y = _yShift;
                tempVertex.z = 2.996f;
                _vertices[currentPoint * 4 + 2] = tempVertex;
                _colors[currentPoint * 4 + 2] = _currentColor;
                #endregion

                #region Vertex3
                tempVertex.x = -1.5f + clustersValues[i][j] * 0.4f;
                tempVertex.y = _yShift;
                tempVertex.z = 2.996f;
                _vertices[currentPoint * 4 + 3] = tempVertex;
                _colors[currentPoint * 4 + 3] = _currentColor;
                #endregion

                //switch vertices order if negative value, for proper drawing of triangle
                if (clustersValues[i][j] < 0)
                {
                    tempVertex = _vertices[currentPoint * 4 + 0];
                    _vertices[currentPoint * 4 + 0] = _vertices[currentPoint * 4 + 1];
                    _vertices[currentPoint * 4 + 1] = tempVertex;

                    tempVertex = _vertices[currentPoint * 4 + 2];
                    _vertices[currentPoint * 4 + 2] = _vertices[currentPoint * 4 + 3];
                    _vertices[currentPoint * 4 + 3] = tempVertex;
                }
                currentPoint++;
            }
            _yShift -= 0.01f;
        }
        currentPoint = 0;
        //create triangles
        for(int i=0; i< currentAlgorithm.clusteredPoints; i++)
        {
            _triangles[currentPoint * 6] = currentPoint*4;
            _triangles[currentPoint * 6 + 1] = currentPoint * 4 + 1;
            _triangles[currentPoint * 6 + 2] = currentPoint * 4 + 2;
            _triangles[currentPoint * 6 + 3] = currentPoint * 4 + 2;
            _triangles[currentPoint * 6 + 4] = currentPoint * 4 + 1;
            _triangles[currentPoint * 6 + 5] = currentPoint * 4 + 3;
            currentPoint++;
        }
        _mesh = new Mesh();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();

        _obj.GetComponent<MeshFilter>().mesh = _mesh;
        tempVertex.x = -1.5f + Coef() * 0.4f;
        tempVertex.y = 1.45f;
        tempVertex.z = 2.996f;
        averageLine.position = tempVertex;
        averageLine.gameObject.SetActive(true);
    }

    public void ResetMe()
    {
        text.text = "";
        _obj.GetComponent<MeshFilter>().mesh.Clear();
    }
}
