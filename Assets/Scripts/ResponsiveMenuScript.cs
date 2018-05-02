using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuScript : MonoBehaviour {
    public bool isShown;
    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    public GameObject pointer;

    private bool wasTouched;
    public GameObject datasetParent;
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

        if (isShown)
        {
            //for moving pointer around when touching the touchpad
            if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                Vector3 newPos = NormalizedValues(touchPos);
                //limit the movement in Z position for every submenu except Datasets submenu
                if(!datasetParent.activeSelf)
                {
                    newPos.z = 0f;
                }
                pointer.transform.localPosition = newPos;
            }

            if(device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                if(pointer.GetComponent<PointerScript>().pointerCollides)
                {
                    GameObject currentButton = pointer.GetComponent<PointerScript>().collider;
                    currentButton.GetComponent<UniversalButtonScript>().Press();
                }
            }
        }
    }

    private void ToggleResponsiveMenu()
    {
        if(isShown)
        {
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            isShown = false;
        }
        else
        {
            //show the pointer and the primary menu
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            isShown = true;
        }
    }

    private Vector3 NormalizedValues(Vector2 value)
    {
        Vector3 pos = new Vector3();
        float min = -1f;
        float max = 1f;
        float endOfScaleX = -0.33f;
        float topOfScaleX = 0.33f;
        float endOfScaleZ = -0.15f;
        float topOfScaleZ = 0.4f;

        pos.x = (topOfScaleX - endOfScaleX) * ((value.x - min) / (max - min)) + endOfScaleX;
        pos.z = (topOfScaleZ - endOfScaleZ) * ((value.y - min) / (max - min)) + endOfScaleZ;
        return pos;
    }
}
