using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generator : MonoBehaviour {
    public TextAsset data;
    public Material material;
    private List<String> classes;
    private int count = 0;

	// Use this for initialization
	void Start () {
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

        count = 0;
        //perform PCA if there is substring "PCA" in the name of the dataset
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
                count++;
            }

            //adding specific color accodring to each class
            Color[] colorArray = new Color[classes.Count];
            float r, g, b;
            for (int k = 0; k < classes.Count; k++)
            {
                r = UnityEngine.Random.Range(0.0f, 1.0f);
                g = UnityEngine.Random.Range(0.0f, 1.0f);
                b = UnityEngine.Random.Range(0.0f, 1.0f);
                colorArray[k] = new Color(r, g, b);
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
                GameObject dataPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dataPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                dataPoint.transform.parent = gameObject.transform;
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
                dataPoint.AddComponent<Raycaster>();
                dataPoint.transform.localPosition = dataPosition;
                dataPoint.GetComponent<MeshRenderer>().material = material;
                Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];


                for (int t = 0; t < vertices.Length; t++)
                {
                    for (int z = 0; z < classes.Count; z++)
                    {
                        if (strDataset[i, cols - 1] == classes[z])
                        {
                            dataPoint.GetComponent<Renderer>().material.color = colorArray[z];
                            colors[t] = colorArray[z];
                        }
                    }

                }
                mesh.colors = colors;
            }
        }

    }

    // Update is called once per frame
    void Update () {
		
	}

    //converts the data eigen vector matrix to the proper one, since the function from the library does not do the proper work
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
