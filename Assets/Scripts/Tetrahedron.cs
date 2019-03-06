using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tetrahedron : MonoBehaviour
{

    public TetrahedronMesh tetrahedron;
    float[] data1 = new float[] { 0.1f, 0.1f, 0.1f };
    public TextAsset data;
    List<TetrahedronMesh> listObjects;
    public Vector3 Positions;
    public List<Vector3> dataPositions;
    public DenclueAlgorithm tiledMap;
    public GameObject dummy;
    private Vector3 dummyPos;

    public List<Color> listOfColors = new List<Color>();
    public GameObject planeBox;
    public GameObject trackObj;
    //the solid material (selection)
    [SerializeField]
    public Material dataMappedMaterial;
    //the transparent material (rest) (and potential ordered wrong)
    [SerializeField]
    public Material dataMappedTransparent;

    private float minX = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinX
    {
        get
        {
            return minX;
        }

        set
        {
            minX = value;
            dataMappedMaterial.SetFloat("_SelectionMinX", minX);
        }
    }
    
    private float maxX = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxX
    {
        get
        {
            return maxX;
        }

        set
        {
            maxX = value;
            dataMappedMaterial.SetFloat("_SelectionMaxX", maxX);
        }
    }

    private float minY = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinY
    {
        get
        {
            return minY;
        }

        set
        {
            minY = value;
            dataMappedMaterial.SetFloat("_SelectionMinY", minY);
        }
    }

    private float maxY = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxY
    {
        get
        {
            return maxY;
        }

        set
        {
            maxY = value;
            dataMappedMaterial.SetFloat("_SelectionMaxY", maxY);
        }
    }
    
    private float minZ = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinZ
    {
        get
        {
            return minZ;
        }

        set
        {
            minZ = value;
            dataMappedMaterial.SetFloat("_SelectionMinZ", minZ);
        }
    }
    
    private float maxZ = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxZ
    {
        get
        {
            return maxZ;
        }

        set
        {
            maxZ = value;
            dataMappedMaterial.SetFloat("_SelectionMaxZ", maxZ);
        }
    }

    private GameObject[] rotateObjects;
    private GameObject rotateObject;
    //counterData initialized to 0 if app is started for first time to load the data from begining
    private int counterData = 0;
    public List<string> classes;

    // Use this for initialization
    void Start()
    {
        //prepare data
        //
        dummy.SetActive(true);
        if(dummy.GetComponent<TetrahedronMesh>() == null)
        {
            dummy.AddComponent<TetrahedronMesh>();
        }
        /*if (counterData == 0)
        {
            data = FindObjectOfType<datasetChooserWallScript>().datasets[0];
        }*/

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

        tetrahedron = dummy.GetComponent<TetrahedronMesh>();
        dummyPos.x = 0;
        dummyPos.y = 0;
        dummyPos.z = 0;
        tetrahedron.Init(data1, dummyPos);

        listObjects = new List<TetrahedronMesh>();

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
                float[] dataPos = { dataPosition.x, dataPosition.y, dataPosition.z };

                GameObject tetrahedron = Instantiate(dummy, Positions, Quaternion.AngleAxis(0, Vector3.up), transform);
                TetrahedronMesh bar = tetrahedron.GetComponent<TetrahedronMesh>();
                tetrahedron.AddComponent<PreviousStepProperties>();
                tetrahedron.AddComponent<DBScanProperties>();
                tetrahedron.GetComponent<MeshRenderer>().material = dataMappedMaterial;

                

                bar.Init(dataPos, Positions);
                bar.transform.localScale -= new Vector3(0.985F, 0.985F, 0.985F);

                Mesh mesh = bar.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];


                for (int t = 0; t < vertices.Length; t++)
                {
                    for (int z = 0; z < classes.Count; z++)
                    {
                        if (strDataset[i, cols - 1] == classes[z])
                        {
                            tetrahedron.GetComponent<MeshRenderer>().material.color = colorArray[z];
                            bar.GetComponent<TetrahedronBehaviour>().color = colorArray[z];
                        }
                    }
                    colors[t] = bar.GetComponent<MeshRenderer>().material.color;
                }
                mesh.colors = colors;
                listOfColors.Add(bar.GetComponent<MeshRenderer>().material.color);
                listObjects.Add(bar);
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

                GameObject tetrahedron = Instantiate(dummy, Positions, Quaternion.AngleAxis(0, Vector3.up), transform);
                TetrahedronMesh bar = tetrahedron.GetComponent<TetrahedronMesh>();
                tetrahedron.AddComponent<PreviousStepProperties>();
                tetrahedron.AddComponent<DBScanProperties>();
                Color color = new Color(float.Parse(attributes[4]), float.Parse(attributes[5]), float.Parse(attributes[6]), 1f);

                bar.Init(dataPositions, Positions);
                tetrahedron.transform.localScale -= new Vector3(0.991F, 0.991F, 0.991F);
                tetrahedron.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
                tetrahedron.GetComponent<Renderer>().material.color = color;


                listOfColors.Add(color);

                listObjects.Add(bar);
            }
        }

        tiledMap.positions = new Vector3[dataPositions.Count];
        for (int p = 0; p < dataPositions.Count; p++)
        {
            tiledMap.positions[p] = dataPositions[p];
        }
        tiledMap.gaussCoef = GetComponent<GaussianCoefficients>();
        tiledMap.ResetMe();
        tiledMap.gameObject.SetActive(true);
        //        createCombinedTetrahedron(listObjects);
        dummy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private GameObject createCombinedTetrahedron(List<TetrahedronMesh> objects)
    {
        GameObject ret = null;

        //calculate max objects per cube (unity vertices limit)
        int vertexCount = dummy.GetComponent<MeshFilter>().sharedMesh.vertexCount * objects.Count;
        //Debug.Log("Total Vertices:" + vertexCount);

        //Unity limitation. we need to split the object list
        if (vertexCount > System.UInt16.MaxValue)
        {
            //
            //Debug.Log("Tiling cubes. Needed subcubes:"+ System.Math.Ceiling((double)vertexCount/ System.UInt16.MaxValue));
            GameObject combinedTetrahedron = new GameObject("combinedTetrahedron");
            combinedTetrahedron.tag = "DestroyCombinedTetrahedron";
            combinedTetrahedron.transform.parent = gameObject.transform;
            combinedTetrahedron.transform.localPosition = new Vector3(0, 0, 0);
            combinedTetrahedron.transform.localRotation = Quaternion.identity;
            combinedTetrahedron.transform.localScale = new Vector3(1, 1, 1);

            int objectsPerRun = (int)System.Math.Floor(System.UInt16.MaxValue / (double)dummy.GetComponent<MeshFilter>().sharedMesh.vertexCount);

            //iterate objects and create tiled cubes
            int index = 0;
            while (index < objects.Count)
            {
                createTetrahedronObject(objects.GetRange(index, index + objectsPerRun >= objects.Count ? objects.Count - index : objectsPerRun), combinedTetrahedron);
                index += objectsPerRun;
            }
            ret = combinedTetrahedron;

        }
        else
        {
            ret = createTetrahedronObject(objects, gameObject);
            ret.AddComponent<TiledTetrahedronVariables>();
            ret.GetComponent<TiledTetrahedronVariables>().plane = planeBox;
            ret.GetComponent<TiledTetrahedronVariables>().trackObj = trackObj;
            ret.GetComponent<TiledTetrahedronVariables>().scatterplot = gameObject;
        }
        
        //destroy child objects
        foreach (TetrahedronMesh o in objects)
        {
            Destroy(o.gameObject);
        }

        return ret;
    }

    //create a single colored tetrahedron object out of the children
    private GameObject createTetrahedronObject(List<TetrahedronMesh> objects, GameObject parent)
    {
        //"realobject"
        GameObject tetrahedron = new GameObject("Tetrahedron");
        tetrahedron.tag = "DestroyTetrahedron";
        tetrahedron.transform.parent = parent.transform;
        tetrahedron.GetComponent<MeshRenderer>().material = dataMappedMaterial;

        MeshFilter filter = tetrahedron.AddComponent<MeshFilter>();
        MeshRenderer renderer = tetrahedron.AddComponent<MeshRenderer>();
        
        renderer.material = dataMappedMaterial;

        mergeChildren(tetrahedron, objects, filter);

        tetrahedron.transform.parent = parent.transform;
        tetrahedron.transform.localPosition = new Vector3(0, 0, 0);
        tetrahedron.transform.localScale = new Vector3(1, 1, 1);
        tetrahedron.transform.localRotation = Quaternion.identity;

        return tetrahedron;
    }


    private void mergeChildren(GameObject parent, List<TetrahedronMesh> objects, MeshFilter target)
    {
        CombineInstance[] combine = new CombineInstance[objects.Count];
        //        System.Random rnd = new System.Random();
        for (int i = 0; i < objects.Count; i++)
        {
            //make sure the points are aligned with the scatterplot
            Vector3 localPos = objects[i].transform.localPosition;
            objects[i].transform.parent = parent.transform;
            objects[i].transform.localPosition = localPos;
            combine[i].mesh = objects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objects[i].transform.localToWorldMatrix;
            objects[i].gameObject.SetActive(false);
        }

        target.mesh.CombineMeshes(combine);
    }


    public void changeDatafile(TextAsset newData)
    {
        //increase counterData so it wont load the first dataset in data
        counterData++;
        data = newData;
        dataPositions = new List<Vector3>();
        resetMe();
        listOfColors.Clear();
        this.Start();
    }

    public void resetMe()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

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
