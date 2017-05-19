using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
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
public class DataSpaceHandler : MonoBehaviour
{

    public GameObject dataObject;
    private GameObject[] rotateObjects;
    private GameObject rotateObject;
    //TODO replace with something more sensible
    public Text countingTextList;

    //todo performance
    public List<Vector3> dataPositions;
    public List<string> dataClasses;

    [SerializeField]
    public TextAsset data;
    //counterData initialized to 0 if app is started for first time to load the data from begining
    private int counterData = 0;
    //FIXMEE performance
    //the solid material (selection)
    [SerializeField]
    public Material dataMappedMaterial;
    //the transparent material (rest) (and potential ordered wrong)
    [SerializeField]
    public Material dataMappedTransparent;

    private List<GameObject> childCat1;

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
        if (counterData == 0)
        {
            data = FindObjectOfType<datasetChooserWallScript>().datasets[0];
        }
        string[] lines = data.text.Split('\n');


        //variables for PCA
        double[,] dataset = new double[150, 4];
        double[,] normData = new double[150, 4];
        double[] eigenValueSym;
        double[,] eigenVectorSym;
        double[,] covarianceMatrix = new double[4, 4];
        double[,] eigenVectorMatrix = new double[4, 4];
        double[,] transposedEigenVector = new double[4, 4];
        double[,] reducedDataMatrix = new double[150, 4];
        double[] meanVector = new double[150];

        //copy material and set selection parameter
        dataMappedMaterial = new Material(dataMappedMaterial);
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        dataMappedTransparent.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedTransparent.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        int count = 0;
        childCat1 = new List<GameObject>();
        //do this if the data is IRIS, preparing for PCA
        if (data.name == "IRIS")
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string[] attributes = lines[i].Split(',');
                for (int j = 0; j < attributes.Length - 1; j++)
                {
                    dataset[i, j] = Convert.ToDouble(attributes[j]);
                }
            }
            normData = normalization(dataset);
            alglib.covm(normData, out covarianceMatrix);
            alglib.smatrixevd(covarianceMatrix, 4, 1, true, out eigenValueSym, out eigenVectorSym);

            eigenVectorMatrix = eigenVectorMatrixs(eigenVectorSym);
            transposedEigenVector = transposeMatrix(eigenVectorMatrix);
            reducedDataMatrix = MultiplyMatrix(dataset, transposedEigenVector);

            for (int i = 0; i < reducedDataMatrix.GetLength(0); i++)
            {
                GameObject dataPoint = Instantiate(dataObject);
                dataPoint.transform.parent = gameObject.transform;
                Vector3 dataPosition = new Vector3();

                for (int j = 0; j < reducedDataMatrix.GetLength(1); j++)
                {
                    if (j == 0)
                    {
                        dataPosition.x = (float)reducedDataMatrix[i, j] / (-10);
                    }
                    else if (j == 1)
                    {
                        dataPosition.y = (float)reducedDataMatrix[i, j] / (-10);
                    }
                    else if (j == 2)
                    {
                        dataPosition.z = (float)reducedDataMatrix[i, j] / (10);
                    }
                    else
                    {
                        continue;
                    }
                }
                dataPoint.transform.localPosition = dataPosition;

                Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];
                for (int t = 0; t < vertices.Length; t++)
                {
                    if (i < 51)
                    {
                        colors[t] = new Color(1, 0, 0);
                        dataClasses.Add("setosa");
                    }
                    else if (i < 101)
                    {
                        colors[t] = new Color(0, 1, 0);
                        dataClasses.Add("versicolor");
                    }
                    else
                    {
                        colors[t] = new Color(0, 0, 1);
                        dataClasses.Add("virginica");
                    }
                }
                mesh.colors = colors;
                childCat1.Add(dataPoint);

                count++;
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
                dataClasses.Add(attributes[0]);

                //set vertex color
                Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];
                for (int t = 0; t < vertices.Length; t++)
                {
                    colors[t] = new Color(float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture));
                    //colors[t] = new Color(0.2f, 0.6f, 0.4f);
                }
                mesh.colors = colors;
                childCat1.Add(dataPoint);

                count++;
            }
        }

        //Debug.Log("Starting Creating Cubes");
        createTiledCube(childCat1);

        //set the rotation of cubes back to 0, when we rotate the whole scatterplot

        //Rotate Tiled Cube Object
        if (GameObject.FindGameObjectWithTag("DestroyTiledCube") != null)
        {
            rotateObject = GameObject.FindGameObjectWithTag("DestroyTiledCube");
            rotateObject.transform.localRotation = Quaternion.identity;
        }



        //Rotate Cubes when no Tiled Cube Exists
        else if (GameObject.FindGameObjectsWithTag("DestroyCubes") != null)
        {
            rotateObjects = GameObject.FindGameObjectsWithTag("DestroyCubes");
            foreach (var x in rotateObjects)
            {
                x.transform.localRotation = Quaternion.identity;
            }
        }
        //combine children

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
        // cube.SetActive(true);

        //"transparent object" since unity has some draw problems
        GameObject transCube = new GameObject("TransCubeCube");
        transCube.tag = "DestroyCubes";
        transCube.transform.parent = parent.transform;

        MeshFilter filterTrans = transCube.AddComponent<MeshFilter>();
        filterTrans.sharedMesh = filter.mesh;

        renderer = transCube.AddComponent<MeshRenderer>();
        renderer.material = dataMappedTransparent;

        transCube.transform.parent = parent.transform;
        transCube.transform.localPosition = new Vector3(0, 0, 0);
        transCube.transform.localScale = new Vector3(1, 1, 1);
        transCube.SetActive(true);

        dataMappedTransparent.SetFloat("_InverseSelection", -1.0f);
        dataMappedTransparent.SetFloat("_TargetAlpha", 0.2f);
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
                string dataClass = dataClasses[i];
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
        resetMe();
        this.Start();
    }

    public void resetMe()
    {
        //Cube of cubes created when there are more than 65000 vertices
        GameObject TiledCubeToDestroy;

        GameObject[] CubesToDestroy;
        //first destroy Tiled Cube (parent of Cubes)
        TiledCubeToDestroy = GameObject.FindWithTag("DestroyTiledCube");
        if (TiledCubeToDestroy != null)
            Destroy(TiledCubeToDestroy);
        // Destroy the cubes if tiledCube doesn't exist
        else
        {
            CubesToDestroy = GameObject.FindGameObjectsWithTag("DestroyCubes");
            for (var i = 0; i < CubesToDestroy.Length; i++)
                Destroy(CubesToDestroy[i]);
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
        double[] min = { x[0, 0], x[0, 0], x[0, 0], x[0, 0] };
        double[] max = { x[0, 0], x[0, 0], x[0, 0], x[0, 0] };


        //min and max in first column
        for (int i = 0; i < rows; i++)
        {
            if (min[0] > x[i, 0])
            {
                min[0] = x[i, 0];
            }
            if (max[0] < x[i, 0])
            {
                max[0] = x[i, 0];
            }
        }

        //min and max in second column

        for (int i = 0; i < rows; i++)
        {
            if (min[1] > x[i, 1])
            {
                min[1] = x[i, 1];
            }
            if (max[1] < x[i, 1])
            {
                max[1] = x[i, 1];
            }
        }

        //min and max in third column

        for (int i = 0; i < rows; i++)
        {
            if (min[2] > x[i, 2])
            {
                min[2] = x[i, 2];
            }
            if (max[2] < x[i, 2])
            {
                max[2] = x[i, 2];
            }
        }

        //min and max in fourth column

        for (int i = 0; i < rows; i++)
        {
            if (min[3] > x[i, 3])
            {
                min[3] = x[i, 3];
            }
            if (max[3] < x[i, 3])
            {
                max[3] = x[i, 3];
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
