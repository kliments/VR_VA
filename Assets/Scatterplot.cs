using UnityEngine;
using System.Collections;

/// <summary>
/// A scatterplot object which can be used to display 3D categorized scatterplots
/// </summary>
public class Scatterplot : MonoBehaviour {

    public Material material1;
    public Material material2;
    public Material material3;

    public Material neutralMaterial;

    public bool ignoreCategory = true;

    private DataSpaceHandler dataSpaceHandler;

    //setup materials and data objects
    void Awake()
    {
        //set the child data space handler and the correct materials and data objects
        dataSpaceHandler = GetComponentInChildren<DataSpaceHandler>();
        
        dataSpaceHandler.dataObject = (GameObject)Resources.Load("Objects/DataPointCube");
        dataSpaceHandler.material1 = material1;
        dataSpaceHandler.material2 = material2;
        dataSpaceHandler.material3 = material3;
        dataSpaceHandler.neutralMaterial = neutralMaterial;
        dataSpaceHandler.ignoreCategory = ignoreCategory;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
