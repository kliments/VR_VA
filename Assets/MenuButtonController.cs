using UnityEngine;
using System.Collections;
using System;
using VRTK;
using UnityEngine.Events;

[System.Serializable]
public enum MENU_ACTION
{
    ADD,
    DELETE,
    MOVE,
    ROTATE,
    SCALE,
    SELECTDATA
}


public class MenuButtonController : MonoBehaviour {

    public UnityEvent addListeners;
    public UnityEvent deleteListeners;
    public UnityEvent moveListeners;
    public UnityEvent rotationModeListeners;
    public UnityEvent scalingModeListeners;
    public UnityEvent selectDataListeners;

    public void AddModeSelected ()
    {
        Debug.Log("Add Mode Selected");
        addListeners.Invoke();
    }

    public void RemoveModeSelected()
    {
        Debug.Log("Remove Mode Selected");
        deleteListeners.Invoke();
    }

    public void MoveModeSelected()
    {
        Debug.Log("Move Mode Selected");
        moveListeners.Invoke();
    }

    public void RotationModeSelected()
    {
        Debug.Log("Rotation Mode Selected");
        rotationModeListeners.Invoke();
    }

    public void ScalingModeSelected()
    {
        Debug.Log("Scaling Mode Selected");
        scalingModeListeners.Invoke();
    }

    public void SelectDataModeSelected()
    {
        Debug.Log("Select Mode Selected");
        selectDataListeners.Invoke();
    }

}
