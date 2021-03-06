﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class PieChartMeshController : GeneralVisualization
{
    public PieChartMesh mPieChart;
    float[] mData;
    public TextAsset data;
    public Vector3 Positions;
    public List<Vector3> dataPositions;
    public GameObject dummy;
	private int timer = 0;
	PieChartMesh[] listObjects;
    public Camera myCamera;
    public DenclueAlgorithm denclue;
    private Vector3 target;

    private GameObject[] rotateObjects;
    private GameObject rotateObject;
    //counterData initialized to 0 if app is started for first time to load the data from begining
    private int counterData = 0;
    public List<string> classes;

    void Start()
    {
        //prepare data
        //
        dummy.SetActive(true);
        if (dummy.GetComponent<PieChartMesh>() == null)
        {
            dummy.AddComponent<PieChartMesh>();
        }

        string[] lines = data.text.Split('\n');
        string[] attr = lines[0].Split(',');
        int rows = lines.Length;
        int cols = attr.Length;
        classes = new List<string>();

        //variables for PCA
        // cols-2 because the last 2 colums are number of classes and the actual classes
        double[,] dataset = new double[rows, cols - 2];
        string[,] strDataset = new string[rows, cols];
        double[,] normData = new double[rows, cols - 2];
        double[,] reducedNormalizedData = new double[rows, cols - 2];
        double[] eigenValueSym;
        double[,] eigenVectorSym;
        double[,] covarianceMatrix = new double[cols - 2, cols - 2];
        double[,] eigenVectorMatrix = new double[cols - 2, cols - 2];
        double[,] transposedEigenVector = new double[cols - 2, cols - 2];
        double[,] reducedDataMatrix = new double[rows, cols - 2];

        mPieChart = dummy.GetComponent<PieChartMesh>();
        float[] x = { 1.0f, 1.0f, 1.0f };
        mPieChart.Init(x, 100, 0, 100, null, Positions);
        mPieChart.Draw(x);

		listObjects = new PieChartMesh[lines.Length];

        if (data.name.Contains("PCA"))
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string[] attributes = lines[i].Split(',');

                //remove the /r from the last attribute
                attributes[attributes.Length - 1] = attributes[attributes.Length - 1].Replace("\r", string.Empty);

                //strDataset is just an string matrix of whole data, while dataset matrix is double without last two colums
                for (int j = 0; j < attributes.Length; j++)
                {
                    strDataset[i, j] = attributes[j];
                }

                for (int j = 0; j < attributes.Length - 2; j++)
                {
                    dataset[i, j] = Convert.ToDouble(attributes[j]);
                }
                //adding the classes in List to give specific color to each class
                if (!classes.Contains(attributes[attributes.Length - 1]))
                {
                    classes.Add(attributes[attributes.Length - 1]);
                }
            }

            normData = normalization(dataset);
            alglib.covm(normData, out covarianceMatrix);
            alglib.smatrixevd(covarianceMatrix, cols - 2, 1, true, out eigenValueSym, out eigenVectorSym);

            eigenVectorMatrix = eigenVectorMatrixs(eigenVectorSym);
            transposedEigenVector = transposeMatrix(eigenVectorMatrix);
            reducedDataMatrix = MultiplyMatrix(dataset, transposedEigenVector);
            reducedNormalizedData = normalization(reducedDataMatrix);

            for (int i = 0; i < reducedNormalizedData.GetLength(0); i++)
            {

                Vector3 dataPosition = new Vector3();
                for (int j = 0; j < reducedNormalizedData.GetLength(1); j++)
                {
                    if (j == 0)
                    {
                        dataPosition.x = (float)reducedNormalizedData[i, j];
                    }
                    else if (j == 1)
                    {
                        dataPosition.y = (float)reducedNormalizedData[i, j];
                    }
                    else if (j == 2)
                    {
                        dataPosition.z = (float)reducedNormalizedData[i, j];
                    }
                    else
                    {
                        continue;
                    }
                }
                dataPositions.Add(dataPosition);
                float[] dataPos = {dataPosition.x, dataPosition.y,dataPosition.z};

                GameObject pieChart = Instantiate(dummy, dataPosition, Quaternion.AngleAxis(0, Vector3.up), transform);
                PieChartMesh bar = pieChart.GetComponent<PieChartMesh>();
                pieChart.AddComponent<PreviousStepProperties>();
                pieChart.AddComponent<DBScanProperties>();
                pieChart.GetComponent<DBScanProperties>().index = i;
                bar.Init(dataPos, 100, 0, 100, null, dataPosition);
                bar.Draw(dataPos);
                bar.transform.localScale -= new Vector3(0.985F, 0.985F, 0.985F);
                //bar.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
                listObjects[i] = bar;
                

                if (dataPosition.x > dataPosition.y && dataPosition.x > dataPosition.z)
                {
                    bar.GetComponent<PieChartBehaviour>().color = Color.red;
                }
                else if( dataPosition.y > dataPosition.x && dataPosition.y > dataPosition.z)
                {
                    bar.GetComponent<PieChartBehaviour>().color = Color.blue;
                }
                else
                {
                    bar.GetComponent<PieChartBehaviour>().color = Color.green;
                }
            }
        }
        else
        {
            for (int i = 0; i < lines.Length; i++)
            {
                //split the lines
                string[] attributes = lines[i].Split(',');
                float[] dataPositions = {
                float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)
                };
                Positions.x = dataPositions[0];
                Positions.y = dataPositions[1];
                Positions.z = dataPositions[2];

                GameObject pieChart = Instantiate(dummy, Positions, Quaternion.AngleAxis(0, Vector3.up), transform);
                pieChart.AddComponent<PreviousStepProperties>();
                pieChart.AddComponent<DBScanProperties>();
                pieChart.GetComponent<DBScanProperties>().index = i;
                PieChartMesh bar = pieChart.GetComponent<PieChartMesh>();
                bar.Init(dataPositions, 100, 0, 100, null, Positions);
                bar.Draw(dataPositions);
                bar.transform.localScale -= new Vector3(0.99F, 0.99F, 0.99F);
                //bar.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
                listObjects[i] = bar;
            }
        }

        if (denclue == null) denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        denclue.positions = new Vector3[dataPositions.Count];
        for (int p = 0; p < dataPositions.Count; p++)
        {
            denclue.positions[p] = dataPositions[p];
        }
        denclue.gaussCoef = GetComponent<GaussianCoefficients>();
        denclue.ResetMe();
        denclue.gameObject.SetActive(true);
        dummy.SetActive (false);
    }

    void Update()
    {

    }
    public void changeDatafile(TextAsset newData)
    {
        //increase counterData so it wont load the first dataset in data
        counterData++;
        data = newData;
        dataPositions = new List<Vector3>();
        resetMe();
        this.Start();
    }

    public void resetMe()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ReturnOriginalColors()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().materials[0].color = Color.red;
            child.GetComponent<MeshRenderer>().materials[1].color = Color.blue;
            child.GetComponent<MeshRenderer>().materials[2].color = Color.green;
        }
    }

    //converts the IRIS eigen vector matrix to the proper one, since the function from the library does not do the proper work
    static double[,] eigenVectorMatrixs(double[,] m)
    {
        int rows = m.GetLength(0);
        int cols = m.GetLength(1);
        double[,] x = new double[rows, cols];
        int i = 0;
        int j = 0;
        for (int i1 = 0; i1 < rows; i1++)
        {
            for (int j1 = cols - 1; j1 >= 0; j1--)
            {
                //only the last column is with the proper numbers, the other ones are multiplied by -1
                if (j1 == 3)
                {
                    x[i, j] = m[i1, j1];
                    j++;
                }
                else
                {

                    x[i, j] = m[i1, j1] * (-1);
                    j++;
                }
            }
            j = 0;
            i++;
        }

        return x;
    }

    //function for transposing the Eigen Vectors Matrix
    static double[,] transposeMatrix(double[,] m)
    {
        int rows = m.GetLength(0);
        int cols = m.GetLength(1);
        double[,] x = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                x[i, j] = m[j, i];
            }
        }

        return x;
    }


    //function for multiplication of 2 Matrices
    static double[,] MultiplyMatrix(double[,] a, double[,] b)
    {
        double[,] c = new double[a.GetLength(0), b.GetLength(1)];
        if (a.GetLength(1) == b.GetLength(0))
        {
            for (int i = 0; i < c.GetLength(0); i++)
            {
                for (int j = 0; j < c.GetLength(1); j++)
                {
                    c[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
                        c[i, j] = c[i, j] + a[i, k] * b[k, j];
                }
            }
        }
        else
        {
            Console.WriteLine("\n Number of columns in First Matrix should be equal to Number of rows in Second Matrix.");
            Console.WriteLine("\n Please re-enter correct dimensions.");
            Environment.Exit(-1);
        }
        return c;
    }

    static double[,] normalization(double[,] x)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        double[,] normData = new double[rows, cols];
        double[] min = new double[cols];
        double[] max = new double[cols];

        for (int i = 0; i < cols; i++)
        {
            min[i] = x[0, 0];
            max[i] = x[0, 0];
        }

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (min[i] > x[j, i])
                {
                    min[i] = x[j, i];
                }
                if (max[i] < x[j, i])
                {
                    max[i] = x[j, i];
                }
            }
        }



        //normalize the data

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                normData[i, j] = (x[i, j] - min[j]) / (max[j] - min[j]);
            }
        }

        return normData;
    }
}
