using UnityEngine;
using System.Collections;

/// <summary>
/// A scatterplot object which can be used to display 3D categorized scatterplots
/// </summary>
[System.Serializable] 
public class Scatterplot : MonoBehaviour {
    public bool colorAsAttributes = true;

    [SerializeField]
    public DataSpaceHandler dataSpaceHandler;

    //setup materials and data objects
    void Awake()
    {
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
