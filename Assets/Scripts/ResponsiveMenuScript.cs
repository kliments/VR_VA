using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuScript : MonoBehaviour {
    public bool isShown, increaseDecrease;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    public GameObject pointer, datasetParent;
    public bool isActive;
    private bool wasTouched, moveAndResize;
    private GameObject currentButton;
    private Vector3 inactivePos, activePos, inactiveScale, activeScale, actualPos;
    private Vector2 oldPos, newPos;
    // Use this for initialization
    void Start () {
        isShown = true;
        increaseDecrease = false;
        trackedObj = transform.parent.transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        inactiveScale = new Vector3(0.7f, 0.7f, 0.7f);
        activeScale = new Vector3(1, 1, 1);
        actualPos = new Vector3();
        AssignValues();
    }
	
	// Update is called once per frame
	void Update () {
        if (isActive)
        {
            if (moveAndResize)
            {
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, inactiveScale, Time.deltaTime * 10f);
                gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, inactivePos, Time.deltaTime * 10f);
                if (Vector3.Distance(gameObject.transform.localPosition, inactivePos) < 0.00005f)
                {
                    //no more moving or resizing needed
                    moveAndResize = false;
                    gameObject.transform.localPosition = inactivePos;
                }
            }
        }
        else
        {
            if (moveAndResize)
            {
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, activeScale, Time.deltaTime * 10f);
                gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, activePos, Time.deltaTime * 10f);
                if (Vector3.Distance(gameObject.transform.localPosition, activePos) < 0.00005f)
                {
                    //no more moving or resizing needed
                    moveAndResize = false;
                    gameObject.transform.localPosition = activePos;
                }
            }
        }
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            oldPos = newPos;
        }
        //for moving pointer around when touching the touchpad
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            currentButton = pointer.GetComponent<PointerScript>().collider;
            oldPos = newPos;
            newPos = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            Vector2 difference = newPos - oldPos;
            if(Mathf.Abs(difference.x) > 0.01f)
            {
                actualPos.x = difference.x * 0.1f;
                
                actualPos.z = 0;
                actualPos.y = 0;
                if (pointer.transform.localPosition.x < -0.28f && difference.x < 0)
                {
                    actualPos.x = 0;
                }

                if (pointer.transform.localPosition.x > 0.28f && difference.x > 0)
                {
                    actualPos.x = 0;
                }
                pointer.transform.localPosition += actualPos;
            }
            if (currentButton.tag == "increaseDecrease")
            {
                if (Mathf.Abs(difference.y) > 0.05f)
                {
                    currentButton.GetComponent<UniversalButtonScript>().difference = difference.y;
                    currentButton.GetComponent<UniversalButtonScript>().Press();
                    increaseDecrease = true;
                }
            }
        }
        else
        {
            if(increaseDecrease)
            {
                increaseDecrease = false;
                currentButton.GetComponent<UniversalButtonScript>().CancelAllCalls();
            }
        }
        //when touchpad press on button but not increasing or decreasing 
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && !increaseDecrease)
        {
            //only if pointer is at a button
            if (pointer.GetComponent<PointerScript>().pointerCollides)
            {
                currentButton = pointer.GetComponent<PointerScript>().collider;
                currentButton.GetComponent<UniversalButtonScript>().Press();
            }
        }

        /*else if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            //only if pointer is at a increase or Decrease button
            currentButton = pointer.GetComponent<PointerScript>().collider;
            if (currentButton.tag == "increaseDecrease")
            {
                currentButton.GetComponent<UniversalButtonScript>().Press();
            }
        }
        //cancel all invoke calls for increasing/decreasing
        else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (increaseDecrease)
            {
                increaseDecrease = false;
                currentButton.GetComponent<UniversalButtonScript>().CancelAllCalls();
            }
        }*/
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
            inactivePos = new Vector3(0f, -0.03f, 0.235f);
        }
        else
        {
            inactivePos = new Vector3(0f, -0.03f, 0.065f);
        }
        activePos = new Vector3(0, 0, 0.15f);
    }
}
