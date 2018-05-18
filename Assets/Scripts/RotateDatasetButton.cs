using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDatasetButton : MonoBehaviour {
    public bool isHovered, toChange, toChangeBack;
    private ResponsiveMenuDatasetsGenerator datasetGenerator;
    public List<GameObject> leftSideButtons, rightSideButtons;
    private List<GameObject> allButtons;
    // Use this for initialization
    void Start () {
        datasetGenerator = (ResponsiveMenuDatasetsGenerator)FindObjectOfType(typeof(ResponsiveMenuDatasetsGenerator));
        leftSideButtons = new List<GameObject>();
        rightSideButtons = new List<GameObject>();
        allButtons = datasetGenerator.datasetButtons;
        AssignAccordinglyButtons();
        toChange = false;
        toChangeBack = true;
    }
	
	// Update is called once per frame
	void Update () {
		if(isHovered && toChangeBack)
        {
           transform.Rotate(new Vector3(0, 0, 0));
            if(leftSideButtons.Count != 0)
            {
                foreach (GameObject obj in leftSideButtons)
                {
                    obj.transform.localPosition += new Vector3(-0.04f, 0f, 0f);
                }
            }
            if(rightSideButtons.Count != 0)
            {
                foreach (GameObject obj in rightSideButtons)
                {
                    obj.transform.localPosition += new Vector3(0.04f, 0f, 0f);
                }
            }
            toChange = true;
            toChangeBack = false;
        }
        else if(!isHovered && toChange)
        {
            transform.Rotate(new Vector3(0, 0, -90));
            if (leftSideButtons.Count != 0)
            {
                foreach (GameObject obj in leftSideButtons)
                {
                    obj.transform.localPosition += new Vector3(0.04f, 0f, 0f);
                }
            }
            if (rightSideButtons.Count != 0)
            {
                foreach (GameObject obj in rightSideButtons)
                {
                    obj.transform.localPosition += new Vector3(-0.04f, 0f, 0f);
                }
            }
            toChange = false;
            toChangeBack = true;
        }
	}

    void AssignAccordinglyButtons()
    {
        if(gameObject == allButtons[0])
        {
            for(int i=1; i<allButtons.Count; i++)
            {
                rightSideButtons.Add(allButtons[i]);
            }
        }
        else if(gameObject == allButtons[allButtons.Count-1])
        {
            for (int i = 0; i < allButtons.Count-1; i++)
            {
                leftSideButtons.Add(allButtons[i]);
            }
        }
        else
        {
            for(int i = 0; i < allButtons.Count; i++)
            {
                if(i<allButtons.IndexOf(gameObject))
                {
                    leftSideButtons.Add(allButtons[i]);
                }
                else if(i == allButtons.IndexOf(gameObject))
                {
                    continue;
                }
                else
                {
                    rightSideButtons.Add(allButtons[i]);
                }
            }
        }
    }
}
