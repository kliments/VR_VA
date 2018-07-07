using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMainMenu : MonoBehaviour {
    public GameObject menu;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    private bool toggle;
    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        toggle = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip) && toggle)
        {
            menu.SetActive(true);
            toggle = false;
        }
        else if(device.GetPressDown(SteamVR_Controller.ButtonMask.Grip) && !toggle)
        {
            menu.SetActive(false);
            toggle = true;
        }
    }
}
