using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendCallForPermissionRights : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;

    private bool isAllowed;
    // Use this for initialization
    void Start () {
        trackedObj = (SteamVR_TrackedObject)FindObjectOfType(typeof(SteamVR_TrackedObject));
        device = SteamVR_Controller.Input((int)trackedObj.index);
        isAllowed = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && GetComponent<CoverflowPropertiesForPermissionMenu>().isHovered  && isAllowed)
        {
            isAllowed = false;
           

        }
        if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger) && !isAllowed)
        {
            isAllowed = true;
        }
	}
}
