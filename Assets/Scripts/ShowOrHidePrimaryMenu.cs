using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOrHidePrimaryMenu : MonoBehaviour {

    public GameObject datasetsParent, primaryMenuParent, responsiveMenu;
    // Use this for initialization
    void Start () {
        datasetsParent = transform.parent.gameObject;
        responsiveMenu = GameObject.Find("ResponsiveMenu");
        foreach(Transform child in responsiveMenu.transform)
        {
            if(child.gameObject.name == "PrimaryMenuParent")
            {
                primaryMenuParent = child.gameObject;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ButtonPressed()
    {
        datasetsParent.SetActive(false);
        primaryMenuParent.SetActive(true);
    }
}
