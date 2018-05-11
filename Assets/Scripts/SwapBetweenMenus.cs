using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBetweenMenus : MonoBehaviour {
    public GameObject menu1, menu2;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    private float threshold;
    private Vector2 oldPos, newPos;
    private bool primaryShown;
    public bool menuIsActive;
    // Use this for initialization
    void Start () {
        trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        oldPos = new Vector2(0,0);
        newPos = new Vector2();
        threshold = 0.1f;
        primaryShown = true;
        menuIsActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            oldPos = newPos;
            newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            if (newPos.y > oldPos.y + threshold && primaryShown)
            {
                Debug.Log("primary menu shown");
                menu1.GetComponent<ResponsiveMenuScript>().Reposition();
                menu2.GetComponent<ResponsiveMenuScript>().Reposition();
                //oldPos = newPos;
                primaryShown = false;
            }
            else if (newPos.y < oldPos.y - threshold && !primaryShown)
            {
                Debug.Log("secondary menu shown");
                menu1.GetComponent<ResponsiveMenuScript>().Reposition();
                menu2.GetComponent<ResponsiveMenuScript>().Reposition();
                //oldPos = newPos;
                primaryShown = true;
            }
        }
    }
}
