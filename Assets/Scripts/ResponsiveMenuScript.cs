using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuScript : MonoBehaviour {
    public bool isShown;
    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
	// Use this for initialization
	void Start () {
        isShown = true;
        trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
    }
	
	// Update is called once per frame
	void Update () {
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            //show or hide responsive menu
            ToggleResponsiveMenu();
        }
        //if(device.GetPress())
	}

    private void ToggleResponsiveMenu()
    {
        if(isShown)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            isShown = false;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            isShown = true;
        }
    }
}
