using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Implements the Data space handler of a categorical data space object
/// Splits objects into individual cubes (and minimizes the count of unity game objects while doing so)
/// </summary>
public class DataSpaceHandler : MonoBehaviour {

    public GameObject dataObject;
    public Material material1;
    public Material material2;
    public Material material3;

    public TextAsset data;
    public Material dataMappedMaterial;

    public string category1;
    public string category2;
    public string category3;


    public bool colorAsAttributes = true;

    // Use this for initialization
    void Start () {

        //prepare data
        string[] lines =data.text.Split('\n');


        int count = 0;

        List<GameObject> childCat1 = new List<GameObject>();
        List<GameObject> childCat2 = new List<GameObject>();
        List<GameObject> childCat3 = new List<GameObject>();

        Debug.Log("Starting Creating datapoints");
        //initialize points
        for (int i = 0;i<lines.Length;i++)
        {
            //split line
            string[] attributes = lines[i].Split(',');

            //prepare data point game objects
            GameObject dataPoint = Instantiate(dataObject);
            dataPoint.transform.parent = gameObject.transform;
            dataPoint.transform.localPosition = new Vector3(
                float.Parse(attributes[1],System.Globalization.CultureInfo.InvariantCulture) ,
                float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture));



            //parse category
            if (!colorAsAttributes)
            {
                string cat = attributes[0].Substring(1, attributes[0].Length - 2);
                if (cat == category1)
                {
                    childCat1.Add(dataPoint);
                }

                else if (cat == category2)
                {
                    childCat2.Add(dataPoint);
                }

                else if (cat == category3)
                {
                    childCat3.Add(dataPoint);
                }
            }
            else
            {
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
            }

            count++;

        }

        Debug.Log("Starting Creating Cubes");


        //create colored cube objects
        if (!colorAsAttributes)
        {
            createTiledCube(material1, childCat1);
            createTiledCube(material2, childCat2);
            createTiledCube(material3, childCat3);
        }
        else
        {
            createTiledCube(dataMappedMaterial, childCat1);
        }

        //combine children

        Debug.Log(count);
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
        Debug.Log("Total Vertices:" + vertexCount);

        //Unity limitation. we need to split the object list
        if(vertexCount> System.UInt16.MaxValue)
        {
            //
            Debug.Log("Tiling cubes. Needed subcubes:"+ System.Math.Ceiling((double)vertexCount/ System.UInt16.MaxValue));
            GameObject tiledCube = new GameObject("tiledCube");
            tiledCube.transform.parent = gameObject.transform;

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
        cube.SetActive(true);
        return cube;
    }


    private void mergeChildren(GameObject parent,List<GameObject> objects,MeshFilter target)
    {
        CombineInstance[] combine = new CombineInstance[objects.Count];
//        System.Random rnd = new System.Random();
        for (int i = 0;i<objects.Count;i++)
        {
            objects[i].transform.parent = parent.transform;
            combine[i].mesh = objects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objects[i].transform.localToWorldMatrix;
            objects[i].SetActive(false);
        }

        target.mesh.CombineMeshes(combine);
    }
}
