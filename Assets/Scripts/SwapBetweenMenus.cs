﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBetweenMenus : MonoBehaviour {
    public GameObject menu1, menu2;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    public bool dontShowControlsMenu;
    private float threshold;
    private Vector2 oldPos, newPos;
    private bool primaryShown;
    // Use this for initialization
    void Start () {
        trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        oldPos = new Vector2(0,0);
        newPos = new Vector2();
        threshold = 0.1f;
        primaryShown = false;
        dontShowControlsMenu = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(menu2.activeSelf)
        {

            if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                oldPos = newPos;
            }
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
        //hide Controls Menu if user in Responsive menu
        if (dontShowControlsMenu && menu2.activeSelf)
        {
            menu2.SetActive(false);
        }
        else if(!dontShowControlsMenu && !menu2.activeSelf)
        {
            menu2.SetActive(true);
        }
    }
    
}
