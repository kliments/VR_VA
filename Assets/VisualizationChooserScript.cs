using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationChooserScript : MonoBehaviour {
    
    public Transform visualizationContainer;
    public Transform scatterplot;
    // Use this for initialization
    void Start() {
        string[] names = FindObjectNames(scatterplot.gameObject, "coordinatesData", 4);
        int i = 0;
        foreach (Transform child in visualizationContainer )
        {
            child.name = names[i] + "Button";
            if (child.name == "DataSpaceButton")
            {
                child.GetComponent<VisualizationChangerScript>().isSelected = true;
            }
            else
            {

            }
            //element.transform.localRotation = Quaternion.identity;
            //element.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            i++;
        }
        //visualizationContainer.transform.Rotate = Quaternion.Euler()
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public static string[] FindObjectNames(GameObject parent, string tag, int nrOfVis)
    {
        string[] list = new string[nrOfVis];
        int i = 0;
        Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
        foreach (Transform t in trs)
        {
            if (t.tag == tag)
            {
                list[i] = t.gameObject.name;
                i++;
            }
        }
        return list;
    }
}
