using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuScript : MonoBehaviour {
    public bool isShown, increaseDecrease;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    public GameObject pointer, datasetParent;
    public bool isActive, moveAndResize;

    private bool _wasTouched, _slide;
    private GameObject _currentButton;
    private Vector3 _inactivePos, _activePos, _inactiveScale, _activeScale, _actualPos;
    private Vector2 _oldPos, _newPos;
    private int _frameCounter;
    // Use this for initialization
    void Start () {
        _slide = true;
        isShown = true;
        trackedObj = transform.parent.transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        _inactiveScale = new Vector3(0.7f, 0.7f, 0.7f);
        _activeScale = new Vector3(1, 1, 1);
        _actualPos = new Vector3();
        AssignValues();
        _frameCounter = 0;
    }
	
	// Update is called once per frame
	void Update () {
        _currentButton = transform.parent.gameObject.GetComponent<CoverflowScript>().currentButton;
        if (isActive)
        {
            if (moveAndResize)
            {
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, _inactiveScale, Time.deltaTime * 10f);
                gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, _inactivePos, Time.deltaTime * 10f);
                if (Vector3.Distance(gameObject.transform.localPosition, _inactivePos) < 0.00005f)
                {
                    //no more moving or resizing needed
                    moveAndResize = false;
                    gameObject.transform.localPosition = _inactivePos;
                }
            }
        }
        else
        {
            if (moveAndResize)
            {
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, _activeScale, Time.deltaTime * 10f);
                gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, _activePos, Time.deltaTime * 10f);
                if (Vector3.Distance(gameObject.transform.localPosition, _activePos) < 0.00005f)
                {
                    //no more moving or resizing needed
                    moveAndResize = false;
                    gameObject.transform.localPosition = _activePos;
                }
            }
        }
        /*if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            _newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            _oldPos = _newPos;
        }*/
        //for moving pointer around when touching the touchpad
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && _slide)
        {
            _currentButton = transform.parent.gameObject.GetComponent<CoverflowScript>().currentButton;
            _oldPos = _newPos;
            _newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            Vector2 difference = _newPos - _oldPos;

            if (_currentButton.tag == "increaseDecrease" && Mathf.Abs(difference.y) > 0.05f)
            {
                _currentButton.GetComponent<UniversalButtonScript>().difference = difference.y;
                _currentButton.GetComponent<UniversalButtonScript>().Press();
                _slide = false;
            }

            else if (Mathf.Abs(difference.x) > 0.03f)
            {
                _actualPos.x = difference.x * 0.005f;
                
                _actualPos.z = 0;
                _actualPos.y = 0;
                if (pointer.transform.localPosition.x < -0.28f && difference.x < 0)
                {
                    _actualPos.x = 0;
                }

                if (pointer.transform.localPosition.x > 0.28f && difference.x > 0)
                {
                    _actualPos.x = 0;
                }
                pointer.transform.localPosition += _actualPos;
                _slide = false;
            }
        }

        if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            _slide = true;
            _frameCounter = 0;
            _oldPos = new Vector2(0, 0);
            _newPos = new Vector2(0, 0);
            _currentButton.GetComponent<UniversalButtonScript>().CancelAllCalls();
        }

        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && gameObject.name == "ResponsiveMenu")
        {
            _currentButton.GetComponent<UniversalButtonScript>().Press();
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

    public void Reposition()
    {
        if(isActive)
        {
            isActive = false;
            moveAndResize = true;
        }
        else
        {
            isActive = true;
            moveAndResize = true;
        }
    }

    public void AssignValues()
    {
        if(gameObject.name == "ResponsiveMenu")
        {
            _inactivePos = new Vector3(0f, -0.03f, 0.235f);
        }
        else
        {
            _inactivePos = new Vector3(0f, -0.03f, 0.065f);
        }
        _activePos = new Vector3(0, 0, 0.15f);
    }
}
