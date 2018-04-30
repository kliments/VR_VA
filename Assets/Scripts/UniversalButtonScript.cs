using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalButtonScript : MonoBehaviour {
    public GameObject primaryParent, datasetParent, vizParent, algorithmParent, kmeansParent, dbscanParent;
    private Transform responsiveMenu;
    public bool isHover, isPress;

    private Color currentColor, highlightedColor;

    // Use this for initialization
	void Start () {
        FindParents();
        isHover = false;
        isPress = false;

        currentColor = GetComponent<MeshRenderer>().material.color;
        highlightedColor = Color.yellow;

    }
	
	// Update is called once per frame
	void Update () {
        if(isHover)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(currentColor, highlightedColor, 1);
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.Lerp(highlightedColor, currentColor, 1);
        }
	}

    private void OnEnable()
    {
        if(isPress)
        {
            GetComponent<Animator>().SetBool("selected", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("selected", false);
        }
    }

    void FindParents()
    {
        responsiveMenu = GameObject.Find("ResponsiveMenu").transform;
        foreach(Transform child in responsiveMenu)
        {
            if(child.name == "PrimaryMenuParent")
            {
                primaryParent = child.gameObject;
            }
            else if(child.name == "DatasetParent")
            {
                datasetParent = child.gameObject;
            }
            else if(child.name == "VisualizationsParent")
            {
                vizParent = child.gameObject;
            }
            else if(child.name == "AlgorithmsParent")
            {
                algorithmParent = child.gameObject;
            }
            else if(child.name == "KMeansParent")
            {
                kmeansParent = child.gameObject;
            }
            else if(child.name == "DBSCANParent")
            {
                dbscanParent = child.gameObject;
            }
        }
    }
    
    public void Press()
    {
        foreach (Transform child in gameObject.transform.parent)
        {
            child.gameObject.GetComponent<Animator>().SetBool("selected", false);
        }
        GetComponent<Animator>().SetBool("selected", true);
    }
    
}
