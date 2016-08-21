using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Needed because unity runs on an ancient version of c# as of 5.4. If you now something better pleace replace this. It's used below to sorte the classes of selected objects
/// </summary>
public class ListStringIntComparer : IComparer<KeyValuePair<string,int>>
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
public class DataSpaceHandler : MonoBehaviour {

    public GameObject dataObject;

    //TODO replace with something more sensible
    public Text countingTextList;

    //todo performance
    public List<Vector3> dataPositions;
    public List<string> dataClasses;

    [SerializeField]
    public TextAsset data;
    [SerializeField]
    public Material dataMappedMaterial;


    private float minX = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinX
    {
        get {
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


    private float minY =0.0f;
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
    void Start () {

        //TODO rework
        countingTextList = GameObject.FindGameObjectWithTag("SelectionText").GetComponent<Text>();

        //prepare data
        string[] lines =data.text.Split('\n');

        //copy material and set selection parameter
        dataMappedMaterial = new Material(dataMappedMaterial);
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        int count = 0;

        List<GameObject> childCat1 = new List<GameObject>();

        //Debug.Log("Starting Creating datapoints");
        //initialize points
        for (int i = 0;i<lines.Length;i++)
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
                }
                mesh.colors = colors;
                childCat1.Add(dataPoint);

            count++;

        }

        //Debug.Log("Starting Creating Cubes");
        createTiledCube(dataMappedMaterial, childCat1);


        //combine children

        //Debug.Log(count);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //Create a cube with all objects of that type and subdivide if needed
    private GameObject createTiledCube(Material mat,List<GameObject> objects)
    {
        GameObject ret = null;

        //calculate max objects per cube (unity vertices limit)
        int vertexCount = dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount*objects.Count;
        //Debug.Log("Total Vertices:" + vertexCount);

        //Unity limitation. we need to split the object list
        if(vertexCount> System.UInt16.MaxValue)
        {
            //
          //Debug.Log("Tiling cubes. Needed subcubes:"+ System.Math.Ceiling((double)vertexCount/ System.UInt16.MaxValue));
            GameObject tiledCube = new GameObject("tiledCube");
            tiledCube.transform.parent = gameObject.transform;
            tiledCube.transform.localPosition = new Vector3(0, 0, 0);
            tiledCube.transform.localScale = new Vector3(1, 1, 1);

            int objectsPerRun = (int)System.Math.Floor(System.UInt16.MaxValue / (double)dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);

            //iterate objects and create tiled cubes
            int index = 0;
            while(index<objects.Count)
            {
                createCubeObject(mat, objects.GetRange(index, index+objectsPerRun>=objects.Count?objects.Count-index:objectsPerRun),tiledCube);
                index += objectsPerRun;
            }
           ret=tiledCube;
        }
        else
        {
           ret=createCubeObject(mat, objects,gameObject);
        }

        //destroy child objects
        foreach(GameObject o in objects)
        {
            Destroy(o);
        }

        return ret;
    }

    //create a single colored cube object out of the children
    private GameObject createCubeObject(Material mat,List<GameObject>objects,GameObject parent)
    {

        GameObject cube = new GameObject("Cube");
        cube.transform.parent = parent.transform;


        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();

        renderer.material = mat;
        mergeChildren(cube,objects, filter);

        cube.transform.parent = parent.transform;
        cube.transform.localPosition = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(1, 1, 1); 
        cube.SetActive(true);
        return cube;
    }


    private void mergeChildren(GameObject parent,List<GameObject> objects,MeshFilter target)
    {
        CombineInstance[] combine = new CombineInstance[objects.Count];
//        System.Random rnd = new System.Random();
        for (int i = 0;i<objects.Count;i++)
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
    public void setSelectionSphere(Vector3 center,float radius)
    {
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", radius);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", center);





        //update selected statistics TODO all that follows is not performant
        int count = 0;
        float squaredRad = radius * radius;

        Dictionary<string, int> selectedClasses = new Dictionary<string, int>();


        for (int i =0;i<dataPositions.Count;i++)
        {
            Vector3 pos = dataPositions[i];
            Vector3 diff = pos - center;
            float squaredDistance = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
            if(squaredDistance<squaredRad)
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
            List< KeyValuePair < string, int>> sortedClasses = new List<KeyValuePair<string, int>>(selectedClasses);
            sortedClasses.Sort(new ListStringIntComparer());
            foreach (KeyValuePair<string, int> entry in sortedClasses)
            {
                countingTextList.text += entry.Key + ": " + entry.Value + "\n";
            }
        }

    }
}
