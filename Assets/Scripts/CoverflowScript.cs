using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverflowScript : MonoBehaviour {
    public GameObject menuToRotate, currentButton;
    private GameObject ptr;
    private Vector3 ptrOldPos, ptrNewPos;
	// Use this for initialization
	void Start () {
        menuToRotate = null;
        currentButton = null;
        ptr = GameObject.Find("Pointer");
	}
	
	// Update is called once per frame
	void Update () {

        ptrOldPos = ptrNewPos;
        ptrNewPos = ptr.transform.localPosition;
        if(menuToRotate != null && currentButton != null)
        {
            if (System.Math.Round(ptrOldPos.x, 2) > System.Math.Round(ptrNewPos.x, 2))
            {
                if (currentButton.GetComponent<RotateDatasetButtonProperties>().leftSideButtons.Count != 0)
                {
                    currentButton.GetComponent<RotateDatasetButtonProperties>().isHovered = false;
                    currentButton.GetComponent<RotateDatasetButtonProperties>().leftSideButtons[currentButton.GetComponent<RotateDatasetButtonProperties>().leftSideButtons.Count - 1].GetComponent<RotateDatasetButtonProperties>().isHovered = true;
                }
            }
            else if (System.Math.Round(ptrOldPos.x, 2) < System.Math.Round(ptrNewPos.x, 2))
            {
                if (currentButton.GetComponent<RotateDatasetButtonProperties>().rightSideButtons.Count != 0)
                {
                    currentButton.GetComponent<RotateDatasetButtonProperties>().isHovered = false;
                    currentButton.GetComponent<RotateDatasetButtonProperties>().rightSideButtons[0].GetComponent<RotateDatasetButtonProperties>().isHovered = true;
                }
            }
        }
    }

    public void AssignValues(GameObject menuParent)
    {
        menuToRotate = menuParent;
        currentButton = menuToRotate.transform.GetChild(0).gameObject;
    }
}
