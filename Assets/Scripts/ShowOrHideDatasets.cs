using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOrHideDatasets : MonoBehaviour {
    public GameObject datasetsParent, primaryMenuParent, responsiveMenu;
	// Use this for initialization
	void Start ()
    {
        primaryMenuParent = transform.parent.gameObject;
        responsiveMenu = primaryMenuParent.transform.parent.gameObject;
        foreach (Transform child in responsiveMenu.transform)
        {
            if (child.gameObject.name == "DatasetParent")
            {
                datasetsParent = child.gameObject;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ButtonPressed()
    {
        primaryMenuParent.SetActive(false);
        datasetsParent.SetActive(true);
    }
}
