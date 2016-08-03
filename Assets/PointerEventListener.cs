﻿using UnityEngine;
using System.Collections;
using VRTK;


public class PointerEventListener : MonoBehaviour {

    private MENU_ACTION menuAction=MENU_ACTION.ADD;

    public LayerMask layersToIgnoreAdd = Physics.IgnoreRaycastLayer;
    public LayerMask layersToIgnoreModify = Physics.IgnoreRaycastLayer;

    public Color addColor;
    public Color deleteColor;
    public Color moveColor;
    public Color rotateColor;

    VRTK_SimplePointer pointer;
    VRTK_ControllerEvents controller;

    GameObject selection = null;

    /// <summary>
    /// Prevents the selected object from being changed
    /// </summary>
    bool fixSelection = false;

    /// <summary>
    /// Indicates whether the position of the selected object should be fixed
    /// </summary>
    bool fixPosition = false;

    /// <summary>
    /// The original position vector
    /// </summary>
    Vector3 originalPostion;
    /// <summary>
    /// The differential rotation vector
    /// </summary>
    Quaternion diffRotation;
    Vector3 eulerRotation;


    private void Start()
    {
        if (GetComponent<VRTK_SimplePointer>() == null || GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("PointerEventListener is required to be attached to a SteamVR Controller that has the VRTK_SimplePointer script attached to it");
            return;
        }

        pointer = GetComponent<VRTK_SimplePointer>();
        controller = GetComponent<VRTK_ControllerEvents>();

        //Setup pointer event listeners
        pointer.DestinationMarkerEnter += new DestinationMarkerEventHandler(selectObject);
        pointer.DestinationMarkerExit += new DestinationMarkerEventHandler(deselectObject);
        pointer.DestinationMarkerSet += new DestinationMarkerEventHandler(DoAction);

        //Setup controller button event Listener;
        controller.TriggerPressed += new ControllerInteractionEventHandler(triggerPressed);
        controller.TriggerReleased += new ControllerInteractionEventHandler(triggerReleased);

        //initial mode
        setAddMode();
    }

    private void Update()
    {
        //adapt rotation of the selection to track the parent
        if(fixPosition)
        {
            selection.transform.position = originalPostion;
            //apply the differential rotation
            Vector3 rotation = selection.transform.rotation.eulerAngles;
            rotation.y += gameObject.transform.rotation.eulerAngles.y - eulerRotation.y;
            selection.transform.rotation = Quaternion.Euler(rotation);
        }
    }

    private void DebugLogger(uint index, string action, Transform target, float distance, Vector3 tipPosition)
    {
        string targetName = (target ? target.name : "<NO VALID TARGET>");
        Debug.Log("Controller on index '" + index + "' is " + action + " at a distance of " + distance + " on object named " + targetName + " - the pointer tip position is/was: " + tipPosition);
    }

    private void selectObject(object sender, DestinationMarkerEventArgs e)
    {
        if (fixSelection)
        {
            return;
        }

        if(!e.target)
        {
            selection = null;
            return;
        }
        selection = e.target.gameObject;

    }

    private void deselectObject(object sender, DestinationMarkerEventArgs e)
    {
        if (fixSelection)
        {
            return;
        }

        selection = null;
    }

    /// <summary>
    /// Performs an action (e.g. creating/deleting an data object)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DoAction(object sender, DestinationMarkerEventArgs e)
    {
        Transform target = e.target;
        if(!target)
        {
            Debug.LogError("NO VALID TArget");
        }

        //decide what to do
        switch(menuAction)
        {
            case MENU_ACTION.ADD:
                addScatterplot(e.destinationPosition);
                break;
            case MENU_ACTION.DELETE:
                deleteScatterplot(e.target.gameObject);
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// Determines what happens when the trigger button is pressed outside of a destination event.
    /// </summary>
    private void triggerPressed(object sender,ControllerInteractionEventArgs e)
    {
        if(selection == null)
        {
            return;
        }

        switch(menuAction)
        {
            case MENU_ACTION.MOVE:
                moveScatterplot(selection, true);
                break;
            case MENU_ACTION.ROTATE:
                rotateScatterplot(selection, true);
                break;
            default:
                break;
        }
    }

    private void triggerReleased(object sender,ControllerInteractionEventArgs e)
    {
        if(selection == null)
        {
            return;
        }

        switch(menuAction)
        {
            case MENU_ACTION.MOVE:
                moveScatterplot(selection, false);
                break;
            case MENU_ACTION.ROTATE:
                rotateScatterplot(selection, false);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Adds a new Scatterplot to the world
    /// </summary>
    /// <param name="position">The world position of where to add the scatterplot</param>
    private void addScatterplot(Vector3 position)
    {
        Instantiate(Resources.Load("Objects/Scatterplot", typeof(GameObject)),position+new Vector3(0,0.3f,0),Quaternion.identity);
    }

    /// <summary>
    /// Deletes a scatterplot from the world
    /// </summary>
    /// <param name="scatterplot">The scatterplot to remove</param>
    private void deleteScatterplot(GameObject scatterplot)
    {
        Destroy(scatterplot);

    }

    /// <summary>
    /// Starts or Ends the movement of a scatterplot by parenting it to the controller
    /// </summary>
    /// <param name="scatterplot"> The scatterplot to move</param>
    /// <param name="start">if true the object is parented to the controller. if false it's unparented again</param>
    private void moveScatterplot(GameObject scatterplot,bool start)
    {
        if (start)
        {
            scatterplot.transform.parent = gameObject.transform;
            //disallow the changing of the selection while the move is progress
            fixSelection = true;
        }
        else
        {
            scatterplot.transform.parent = null;
            //free selection again
            fixSelection = false;
        }
    }

    /// <summary>
    /// Starts or Ends the rotation of a scatterplot by parenting to controller but storing the 
    /// </summary>
    /// <param name="scatterplot"> The scatterplot to move</param>
    /// <param name="start">if true the object is parented to the controller. if false it's unparented again</param>
    private void rotateScatterplot(GameObject scatterplot, bool start)
    {
        if (start)
        {
            //


            //disallow the changing of the selection while the move is progress
            fixSelection = true;
            //enable updating the rotation
            fixPosition = true;

            originalPostion = scatterplot.transform.position;
            diffRotation = Quaternion.Inverse(scatterplot.transform.rotation) * gameObject.transform.rotation;
            eulerRotation = scatterplot.transform.rotation.eulerAngles - gameObject.transform.rotation.eulerAngles;
        }
        else
        {
            //free selection again
            fixSelection = false;
            //disable updatin the roation
            fixPosition = false;

        }
    }

    public void setAddMode()
    {
        menuAction = MENU_ACTION.ADD;
        pointer.layersToIgnore = layersToIgnoreAdd;
        pointer.pointerHitColor = addColor;
        pointer.pointerMissColor = new Color(addColor.r, addColor.g, addColor.b, 0.3f);
    }

    public void setDeleteMode()
    {
        menuAction = MENU_ACTION.DELETE;
        pointer.layersToIgnore = layersToIgnoreModify;
        pointer.pointerHitColor = deleteColor;
        pointer.pointerMissColor = new Color(deleteColor.r, deleteColor.g, deleteColor.b, 0.3f);
    }

    public void setMoveMode()
    {
        menuAction = MENU_ACTION.MOVE;
        GetComponent<VRTK_SimplePointer>().layersToIgnore = layersToIgnoreModify;
        pointer.pointerHitColor = moveColor;
        pointer.pointerMissColor = new Color(moveColor.r, moveColor.g, moveColor.b, 0.3f);
    }

    public void setRotationMode()
    {
        menuAction = MENU_ACTION.ROTATE;
        GetComponent<VRTK_SimplePointer>().layersToIgnore = layersToIgnoreModify;
        pointer.pointerHitColor = rotateColor;
        pointer.pointerMissColor = new Color(rotateColor.r, rotateColor.g, rotateColor.b, 0.3f);
    }
}
