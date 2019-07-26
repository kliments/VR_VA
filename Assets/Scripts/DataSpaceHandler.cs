using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// Needed because unity runs on an ancient version of c# as of 5.4. If you now something better pleace replace this. It's used below to sorte the classes of selected objects
/// </summary>
public class ListStringIntComparer : IComparer<KeyValuePair<string, int>>
{
    public int Compare(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
    {
        return b.Value - a.Value;
    }
}

/// <summary>
/// Implements the Data space handler of a categorical data space object
/// Splits objects into individual cubes (and minimizes the count of unity game objects while doing so)
/// </summary>
[System.Serializable]
public class DataSpaceHandler : GeneralVisualization
{
    public GameObject dataObject;
    public GameObject planeBox;
    public GameObject trackObj;
    //TODO replace with something more sensible
    public Text countingTextList;
    
    public List<Vector3> dataPositions;
    public List<string> classes;

    public DenclueAlgorithm denclue;

    [SerializeField]
    public TextAsset data;
    //counterData initialized to 0 if app is started for first time to load the data from begining
    private int counterData = 0;

    public int count;
    //FIXMEE performance
    //the solid material (selection)
    [SerializeField]
    public Material dataMappedMaterial;
    //the transparent material (rest) (and potential ordered wrong)
    [SerializeField]
    public Material dataMappedTransparent;

    private List<GameObject> childCat1;
    
    public List<Color> listOfColors = new List<Color>();
    private Color tempColor;

    public int ind = 0;
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

    // Use this for initialization
    void Start()
    {
        //TODO rework
        if (GameObject.FindGameObjectWithTag("SelectionText") != null)
        {
            countingTextList = GameObject.FindGameObjectWithTag("SelectionText").GetComponent<Text>();
        }

        //prepare data
        //
        /*if (counterData == 0)
        {
            data = FindObjectOfType<ResponsiveMenuDatasetsGenerator>().datasets[0];
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
        
        //copy material and set selection parameter
        dataMappedMaterial = new Material(dataMappedMaterial);
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        dataMappedTransparent.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedTransparent.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        count = 0;
        childCat1 = new List<GameObject>();
        //perform PCA if there is substring "PCA" in the name of the dataset
        if (data.name.Contains("PCA"))
        {
          for (int i = 0; i < lines.Length; i++)
            {
                string[] attributes = lines[i].Split(',');
				
				//remove the /r from the last attribute
                attributes[attributes.Length - 1] = attributes[attributes.Length - 1].Replace("\r", string.Empty);

                //strDataset is just an string matrix of whole data, while dataset matrix is double without last two colums
                for(int j = 0; j < attributes.Length; j++)
                {
                    strDataset[i, j] = attributes[j];
                }

                for (int j = 0; j < attributes.Length - 2; j++)
                {
                    dataset[i, j] = Convert.ToDouble(attributes[j]);
                }
                //adding the classes in List to give specific color to each class
                if(!classes.Contains(attributes[attributes.Length-1]))
                {
                    classes.Add(attributes[attributes.Length - 1]);
                }
                count++;
            }
          

            /*//adding specific color accodring to each class
            Color[] colorArray = new Color[classes.Count];
            float r,g,b;
            for (int k=0; k<classes.Count;k++)
            {
                r = UnityEngine.
                
            .Range(0.0f, 1.0f);
                g = UnityEngine.Random.Range(0.0f, 1.0f);
                b = UnityEngine.Random.Range(0.0f, 1.0f);
                colorArray[k] = new Color(r,g,b);
            }*/


            normData = normalization(dataset);
            alglib.covm(normData, out covarianceMatrix);
            alglib.smatrixevd(covarianceMatrix, cols-2, 1, true, out eigenValueSym, out eigenVectorSym);

            eigenVectorMatrix = eigenVectorMatrixs(eigenVectorSym);
            transposedEigenVector = transposeMatrix(eigenVectorMatrix);
            reducedDataMatrix = MultiplyMatrix(dataset, transposedEigenVector);
            reducedNormalizedData = normalization(reducedDataMatrix);


            for (int i = 0; i < reducedNormalizedData.GetLength(0); i++)
            {

                GameObject dataPoint = Instantiate(dataObject);
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
                dataPoint.transform.localPosition = dataPosition;
                dataPoint.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
                dataPoint.GetComponent<MeshRenderer>().material = dataMappedMaterial;
                dataPositions.Add(dataPosition);
                Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];

                for (int t = 0; t < vertices.Length; t++)
                {
                    for(int z=0;z< classes.Count;z++)
                    {
                        if (strDataset[i, cols-1] == classes[z])
                        {
                            dataPoint.GetComponent<Renderer>().material.color = listOfColors[z];
                            colors[t] = listOfColors[z];
                            tempColor = listOfColors[z];
                        }
                    }
                }
                mesh.colors = colors;
                childCat1.Add(dataPoint);
                
                dataPoint.AddComponent<PreviousStepProperties>();
                dataPoint.AddComponent<DBScanProperties>();
                dataPoint.GetComponent<DBScanProperties>().index = i;
                dataPoint.GetComponent<TiledCubeVariables>().trackObj = trackObj;
            }
        }

        //Debug.Log("Starting Creating datapoints");
        //initialize points
        else
        {
            for (int i = 0; i < lines.Length; i++)
            {
                //split line
                string[] attributes = lines[i].Split(',');

                //prepare data point game objects
                GameObject dataPoint = Instantiate(dataObject);
                dataPoint.transform.parent = gameObject.transform;
                Vector3 dataPosition = new Vector3(
                    float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture));

                dataPoint.transform.localPosition = dataPosition;

                //add the data position
                dataPositions.Add(dataPosition);

                //set vertex color
                Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];
                for (int t = 0; t < vertices.Length; t++)
                {
                    /*colors[t] = new Color(float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture));*/
                    colors[t] = new Color(0.2f, 0.6f, 0.4f);
                }
                mesh.colors = colors;
                childCat1.Add(dataPoint);
                dataPoint.AddComponent<PreviousStepProperties>();
                dataPoint.AddComponent<DBScanProperties>();
                dataPoint.GetComponent<DBScanProperties>().index = i;
                dataPoint.GetComponent<TiledCubeVariables>().trackObj = trackObj;
                count++;
            }
        }
        if (denclue == null) denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        denclue.positions = new Vector3[dataPositions.Count];
        for(int p=0; p<dataPositions.Count; p++)
        {
            denclue.positions[p] = dataPositions[p];
        }
        denclue.gaussCoef = GetComponent<GaussianCoefficients>();
        denclue.ResetMe();
        denclue.gameObject.SetActive(true);

        //        createTiledCube(childCat1);

        //Debug.Log(count);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Create a cube with all objects of that type and subdivide if needed
    private GameObject createTiledCube(List<GameObject> objects)
    {
        GameObject ret = null;

        //calculate max objects per cube (unity vertices limit)
        int vertexCount = dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount * objects.Count;
        //Debug.Log("Total Vertices:" + vertexCount);

        //Unity limitation. we need to split the object list
        if (vertexCount > System.UInt16.MaxValue)
        {
            //
            //Debug.Log("Tiling cubes. Needed subcubes:"+ System.Math.Ceiling((double)vertexCount/ System.UInt16.MaxValue));
            GameObject tiledCube = new GameObject("tiledCube");
            tiledCube.tag = "DestroyTiledCube";
            tiledCube.transform.parent = gameObject.transform;
            tiledCube.transform.localPosition = new Vector3(0, 0, 0);
            tiledCube.transform.localScale = new Vector3(1, 1, 1);

            int objectsPerRun = (int)System.Math.Floor(System.UInt16.MaxValue / (double)dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);

            //iterate objects and create tiled cubes
            int index = 0;
            while (index < objects.Count)
            {
                createCubeObject(objects.GetRange(index, index + objectsPerRun >= objects.Count ? objects.Count - index : objectsPerRun), tiledCube);
                index += objectsPerRun;
            }
            ret = tiledCube;
            
        }
       else
        {
            ret = createCubeObject(objects, gameObject);
        }

        //destroy child objects
        foreach (GameObject o in objects)
        {
            Destroy(o);
        }

        return ret;
    }

    //create a single colored cube object out of the children
    private GameObject createCubeObject(List<GameObject> objects, GameObject parent)
    {
        //"realobject"
        GameObject cube = new GameObject("Cube");
        cube.tag = "DestroyCubes";
        cube.transform.parent = parent.transform;
        

        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();


        renderer.material = dataMappedMaterial;

        mergeChildren(cube, objects, filter);

        cube.transform.parent = parent.transform;
        cube.transform.localPosition = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(1, 1, 1);
        return cube;
    }


    private void mergeChildren(GameObject parent, List<GameObject> objects, MeshFilter target)
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
            objects[i].SetActive(false);
        }

        target.mesh.CombineMeshes(combine);
    }

    /// <summary>
    /// Set the selection by specifying a bounding box. TODO change to plane / sphere coliders
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="minZ"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="maxZ"></param>
    public void setSelection(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        SelectionMinX = minX;
        SelectionMaxX = maxX;

        SelectionMinY = minY;
        SelectionMaxY = maxY;

        SelectionMinZ = minZ;
        SelectionMaxZ = maxZ;
    }


    /// <summary>
    /// Set the selection by specifying a bounding box. TODO change to plane / sphere coliders
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="minZ"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="maxZ"></param>
    public void setSelectionSphere(Vector3 center, float radius)
    {
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", radius * radius);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", center);

        dataMappedTransparent.SetFloat("_SelectionSphereRadiusSquared", radius * radius);
        dataMappedTransparent.SetVector("_SelectionSphereCenter", center);


        //return;

        //update selected statistics TODO all that follows is not performant
        int count = 0;
        float squaredRad = radius * radius;

        Dictionary<string, int> selectedClasses = new Dictionary<string, int>();


        for (int i = 0; i < dataPositions.Count; i++)
        {
            Vector3 pos = dataPositions[i];
            Vector3 diff = pos - center;
            float squaredDistance = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
            if (squaredDistance < squaredRad)
            {
                count++;
                string dataClass = classes[i];
                //insert class count
                if (!selectedClasses.ContainsKey(dataClass))
                {
                    selectedClasses[dataClass] = 1;
                }
                else
                {
                    int oldCount = selectedClasses[dataClass];
                    selectedClasses[dataClass] = ++oldCount;
                }
            }
        }

        //set text
        if (countingTextList != null)
        {
            countingTextList.text = "Total: " + count + "\n\n";

            //workarounds because unity uses an ancient version of c#
            List<KeyValuePair<string, int>> sortedClasses = new List<KeyValuePair<string, int>>(selectedClasses);
            sortedClasses.Sort(new ListStringIntComparer());
            foreach (KeyValuePair<string, int> entry in sortedClasses)
            {
                countingTextList.text += entry.Key + ": " + entry.Value + "\n";
            }
        }

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
        ind = 0;
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

    void DrawPoint()
    {
        Vector3 pos = gameObject.transform.position;
        pos.x += 1;
        pos.y += 1;
        pos.z += 1;
        Graphics.DrawMesh(gameObject.GetComponent<MeshFilter>().mesh, pos, Quaternion.identity, gameObject.GetComponent<MeshRenderer>().material, gameObject.layer);
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

        for(int i = 0; i<cols;i++)
        {
            min[i] = x[0, 0];
            max[i] = x[0, 0];
        }

        for(int i = 0; i<cols;i++)
        {
            for(int j=0;j<rows;j++)
            {
                if(min[i]>x[j,i])
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
