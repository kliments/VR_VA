using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverflowScriptForPermissionMenu : MonoBehaviour {
    public GameObject currentButton;
    private GameObject ptr;
    private Vector3 ptrOldPos, ptrNewPos, _actualPos;
    private Vector2 _oldPos, _newPos;
    private bool _slide;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    // Use this for initialization
    void Start () {
        ptr = GameObject.Find("Pointer");
        trackedObj = (SteamVR_TrackedObject)FindObjectOfType(typeof(SteamVR_TrackedObject));
        device = SteamVR_Controller.Input((int)trackedObj.index);
        _actualPos = new Vector3();
    }
	
	// Update is called once per frame
	void Update () {
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && _slide)
        {
            _oldPos = _newPos;
            _newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            Vector2 difference = _newPos - _oldPos;
            if (Mathf.Abs(difference.x) > 0.03f)
            {
                _actualPos.x = difference.x * 0.01f;

                _actualPos.z = 0;
                _actualPos.y = 0;
                if (ptr.transform.localPosition.x < -0.4f && difference.x < 0)
                {
                    _actualPos.x = 0;
                }

                if (ptr.transform.localPosition.x > 0.4f && difference.x > 0)
                {
                    _actualPos.x = 0;
                }
                ptr.transform.localPosition += _actualPos;
                _slide = false;
            }
        }
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            _slide = true;
            _oldPos = new Vector2(0, 0);
            _newPos = new Vector2(0, 0);
        }
        ptrOldPos = ptrNewPos;
        ptrNewPos = ptr.transform.localPosition;
        if(currentButton != null)
        {
            if (System.Math.Round(ptrOldPos.x, 2) > System.Math.Round(ptrNewPos.x, 2))
            {
                if (currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().leftSideButtons.Count != 0)
                {
                    currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().isHovered = false;
                    currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().leftSideButtons[currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().leftSideButtons.Count - 1].GetComponent<CoverflowPropertiesForPermissionMenu>().isHovered = true;
                    currentButton = currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().leftSideButtons[currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().leftSideButtons.Count - 1];
                }
            }
            else if (System.Math.Round(ptrOldPos.x, 2) < System.Math.Round(ptrNewPos.x, 2))
            {
                if (currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().rightSideButtons.Count != 0)
                {
                    currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().isHovered = false;
                    currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().rightSideButtons[0].GetComponent<CoverflowPropertiesForPermissionMenu>().isHovered = true;
                    currentButton = currentButton.GetComponent<CoverflowPropertiesForPermissionMenu>().rightSideButtons[0];
                }
            }
        }
    }
}
