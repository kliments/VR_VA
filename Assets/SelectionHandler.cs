using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the collision selection objects and sets the parameters
/// </summary>
public class SelectionHandler : MonoBehaviour
{

    DataSpaceHandler handler;
    //TODO handle multiple selections here and in shader
    //TODO switch from box to plane equations to allow rotation invariants
    Collider selection;

    void Awake()
    {
        handler = GetComponent<DataSpaceHandler>();
    }

    void OnTriggerEnter(Collider other)
    {
        selection = other;
    }

    void OnTriggerExit(Collider other)
    {
        selection = null;
        handler.setSelection(0, 0, 0, 1, 1, 1);
    }

    //    use this for initialization

    //   void start()
    //    {

    //    }

    /// <summary>
    /// Update the selection parameter if there is a selection
    /// </summary>
    void Update()
    {
        //no selection no paramter
        if(!selection)
        {
            return;
        }

        //get collider bounds
        Bounds bounds = selection.bounds;

        //convert to local coordinates
        Vector3 minTransformed = gameObject.transform.InverseTransformPoint(bounds.min);
        Vector3 maxTransformed = gameObject.transform.InverseTransformPoint(bounds.max);

        //Debug.Log(minTransformed +" "+ maxTransformed);

        //set selection
        handler.setSelection(minTransformed.x, minTransformed.y, minTransformed.z, maxTransformed.x, maxTransformed.y, maxTransformed.z);
    }

}