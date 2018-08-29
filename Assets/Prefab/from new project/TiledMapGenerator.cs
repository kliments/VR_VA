using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledMapGenerator : MonoBehaviour {
    public List<GameObject> map;
    public Material material;
    private GameObject dataObject;
	// Use this for initialization
	void Start () {
        map = new List<GameObject>();
        Vector3 pos = new Vector3(0,-0.2f,0);
		for(int i=0; i<200; i++)
        {
            for(int j=0; j<200; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.005f, 0.0005f, 0.005f);
                cube.transform.localPosition = pos;
                pos.z += 0.005f;
                cube.transform.parent = transform;
                cube.tag = "Floor";
                map.Add(cube);
            }
            pos.x += 0.005f;
            pos.z = 0;
        }
        dataObject = map[0];
        
        Invoke("RaycastAndCombine", 1.0f);
    }

    // Update is called once per frame
    void Update () {

	}

    private void RaycastAndCombine()
    {
        Raycaster[] rays = (Raycaster[])FindObjectsOfType(typeof(Raycaster));
        for (int i = 0; i < rays.Length; i++)
        {
            rays[i].Raycast();
        }
        //createTiledCube(map);
    }

    public GameObject createTiledCube(List<GameObject> objects)
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
        cube.transform.parent = parent.transform;


        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();
        renderer.material = material;
        mergeChildren(cube, objects, filter);

        cube.transform.parent = parent.transform;
        cube.transform.localPosition = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(1, 1, 1);
        return cube;
    }


    private void mergeChildren(GameObject parent, List<GameObject> objects, MeshFilter target)
    {
        CombineInstance[] combine = new CombineInstance[objects.Count];
        for (int i = 0; i < objects.Count; i++)
        {
            Vector3 localPos = objects[i].transform.localPosition;
            objects[i].transform.parent = parent.transform;
            objects[i].transform.localPosition = localPos;
            combine[i].mesh = objects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objects[i].transform.localToWorldMatrix;
            objects[i].SetActive(false);
        }

        target.mesh.CombineMeshes(combine, true);
    }
}
