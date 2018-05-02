using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOrHideVisualizations : MonoBehaviour {
    public GameObject visualizationsParent, primaryMenuParent, responsiveMenu;

    // Use this for initialization
    void Start ()
    {
        primaryMenuParent = GameObject.Find("PrimaryMenuParent");
        responsiveMenu = GameObject.Find("ResponsiveMenu");
        foreach (Transform child in responsiveMenu.transform)
        {
            if (child.gameObject.name == "VisualizationsParent")
            {
                visualizationsParent = child.gameObject;
            }
        }

    }
	
	// Update is called once per frame
	void Update () {

    }

    public void ButtonPressed()
    {
        primaryMenuParent.SetActive(false);
        visualizationsParent.SetActive(true);
    }
}
