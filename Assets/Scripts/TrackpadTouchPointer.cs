using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackpadTouchPointer : MonoBehaviour {
    public GameObject trackpadTouch;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    public Material mat;
    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if(trackpadTouch.GetComponent<MeshRenderer>()!=null) trackpadTouch.GetComponent<MeshRenderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touchpadPos = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            trackpadTouch.SetActive(true);
            trackpadTouch.transform.localRotation = Quaternion.Euler(-5.509f, 0, 0);
            trackpadTouch.transform.localPosition = NormalizedValue(touchpadPos);
        }
        else
        {
            trackpadTouch.SetActive(false);
        }
    }

    private Vector3 NormalizedValue(Vector2 value)
    {
        Vector3 pos = new Vector3();
        float min = -1f;
        float max = 1f;
        float endOfScaleX = -0.0185f;
        float topOfScaleX = 0.0185f;
        float endOfScaleY = 0.0034f;
        float topOfScaleY = 0.0069f;
        float endOfScaleZ = -0.0185f;
        float topOfScaleZ = 0.0185f;

        pos.x = (topOfScaleX - endOfScaleX) * ((value.x - min) / (max - min)) + endOfScaleX;
        pos.y = (topOfScaleY - endOfScaleY) * ((value.y - min) / (max - min)) + endOfScaleY;
        pos.z = (topOfScaleZ - endOfScaleZ) * ((value.y - min) / (max - min)) + endOfScaleZ;
        return pos;
    }
}
