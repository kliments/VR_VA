using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonMenu : MonoBehaviour {
    public List<GameObject> previousMenus;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    private SwapBetweenMenus swapMenuScript;
    public GameObject responsiveMenu, kmeansControlsMenu;
    // Use this for initialization
    void Start ()
    {
        trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        previousMenus = new List<GameObject>();
        //Primary Menu Parent
        GameObject primaryMenu = new GameObject();
        foreach(Transform child in transform.GetChild(1))
        {
            if(child.name == "PrimaryMenuParent")
            {
                primaryMenu = child.gameObject;
                break;
            }
        }
        previousMenus.Add(primaryMenu);
        swapMenuScript = GetComponent<SwapBetweenMenus>();
    }
	
	// Update is called once per frame
	void Update () {
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            //in case we are in KMeansControlsMenu and we press back button, responsive menu should be the actual one
            if(!kmeansControlsMenu.GetComponent<ResponsiveMenuScript>().isActive)
            {
                kmeansControlsMenu.GetComponent<ResponsiveMenuScript>().isActive = true;
                kmeansControlsMenu.GetComponent<ResponsiveMenuScript>().moveAndResize = true;
                responsiveMenu.GetComponent<ResponsiveMenuScript>().isActive = false;
                responsiveMenu.GetComponent<ResponsiveMenuScript>().moveAndResize = true;
                responsiveMenu.transform.parent.GetComponent<SwapBetweenMenus>().primaryShown = true;
            }
            GoBackInMenu();
        }
	}

    public void GoBackInMenu()
    {//remove the last menu from the list and show the second last
        if (previousMenus.Count > 1)
        {
            previousMenus[previousMenus.Count - 1].SetActive(false);
            previousMenus.RemoveAt(previousMenus.Count - 1);
            previousMenus[previousMenus.Count - 1].SetActive(true);
            GetComponent<CoverflowScript>().AssignValues(previousMenus[previousMenus.Count - 1]);
            if (previousMenus.Count == 1)
            {
                swapMenuScript.dontShowControlsMenu = false;
                //set responsive menu as active one
                GetComponent<CoverflowScript>().menuToRotate = gameObject.transform.GetChild(1).gameObject;
            }

        }
    }
}
