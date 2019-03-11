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
    GameObject selector;

    Transform oldSelectorParent;

    void Awake()
    {
        handler = GetComponent<DataSpaceHandler>();
    }

    void OnTriggerEnter(Collider other)
    {
        //check if a desired parent can be set

        selector = other.gameObject;
        oldSelectorParent = selector.transform.parent;

        if(selector.GetComponent<DesiredParent>()!=null)
        {
            selector.GetComponent<DesiredParent>().desiredParent = gameObject;
        }
        //handler.dataMappedMaterial.SetFloat("_Mode", 2);
    }

    void OnTriggerExit(Collider other)
    {


        if (selector.GetComponent<DesiredParent>() != null)
        {
            selector.GetComponent<DesiredParent>().desiredParent = null;
        }

        //selector.transform.parent = oldSelectorParent;

        selector = null;
        //handler.setSelectionSphere(new Vector3(0.5f, 0.5f, 0.5f), 25);
        //handler.setSelection(0, 0, 0, 1, 1, 1);

        //FIXME set rendering mode opaque for now since the transparent one is not working correctly when there are 
        //handler.dataMappedMaterial.SetFloat("_Mode", 0);


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
        if(!selector)
        {
            return;
        }

        //convert to local coordinates
        Vector3 centerTransformed = gameObject.transform.InverseTransformPoint(selector.transform.TransformPoint(new Vector3(0, 0, 0)));
        //Vector3 maxTransformed = gameObject.transform.InverseTransformPoint(bounds.max);

        float localSphereMagnitude = selector.transform.TransformVector(new Vector3(1,0,0)).magnitude;
        float selectorSize = gameObject.transform.InverseTransformVector(new Vector3(localSphereMagnitude, 0, 0)).magnitude;
        //Debug.Log(minTransformed +" "+ maxTransformed);

        //set selection
        //handler.setSelectionSphere(centerTransformed, selectorSize/2.0f);
    }

}