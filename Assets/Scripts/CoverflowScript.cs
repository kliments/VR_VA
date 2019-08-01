using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverflowScript : MonoBehaviour {
    public GameObject menuToRotate, currentButton;
    private GameObject ptr;
    private Vector3 ptrOldPos, ptrNewPos;
	// Use this for initialization
	void Start () {
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
                if (currentButton.GetComponent<GeneralCoverflowProperties>().leftSideButtons.Count != 0)
                {
                    currentButton.GetComponent<GeneralCoverflowProperties>().isHovered = false;
                    currentButton.GetComponent<GeneralCoverflowProperties>().leftSideButtons[currentButton.GetComponent<GeneralCoverflowProperties>().leftSideButtons.Count - 1].GetComponent<GeneralCoverflowProperties>().isHovered = true;
                    currentButton = currentButton.GetComponent<GeneralCoverflowProperties>().leftSideButtons[currentButton.GetComponent<GeneralCoverflowProperties>().leftSideButtons.Count - 1];
                }
            }
            else if (System.Math.Round(ptrOldPos.x, 2) < System.Math.Round(ptrNewPos.x, 2))
            {
                if (currentButton.GetComponent<GeneralCoverflowProperties>().rightSideButtons.Count != 0)
                {
                    currentButton.GetComponent<GeneralCoverflowProperties>().isHovered = false;
                    currentButton.GetComponent<GeneralCoverflowProperties>().rightSideButtons[0].GetComponent<GeneralCoverflowProperties>().isHovered = true;
                    currentButton = currentButton.GetComponent<GeneralCoverflowProperties>().rightSideButtons[0];
                }
            }
        }
    }

    public void AssignValues(GameObject menuParent)
    {
        menuToRotate = menuParent;
        currentButton = menuToRotate.transform.GetChild(0).gameObject;
    }

    public void DeselectAllButtons()
    {
        foreach(Transform obj in menuToRotate.transform)
        {
            if(menuToRotate.name == "ResponsiveMenu")
            {
                if (obj.gameObject.activeSelf)
                {
                    foreach (Transform child in obj)
                    {
                        if (child.GetComponent<GeneralCoverflowProperties>().isHovered)
                        {
                            child.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            child.transform.localPosition = Vector3.zero;
                            child.GetComponent<GeneralCoverflowProperties>().SetLeftAndRightSideButtons();
                        }
                        child.GetComponent<GeneralCoverflowProperties>().isHovered = false;
                    }
                }
            }

            else
            {
                if (obj.GetComponent<GeneralCoverflowProperties>().isHovered)
                {
                    obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    obj.transform.localPosition = Vector3.zero;
                    obj.GetComponent<GeneralCoverflowProperties>().SetLeftAndRightSideButtons();
                }
                obj.GetComponent<GeneralCoverflowProperties>().isHovered = false;
            }
        }
    }

    public void SetCurrentButton()
    {
        bool isHover = false;
        foreach (Transform obj in menuToRotate.transform)
        {
            if (menuToRotate.name == "ResponsiveMenu")
            {
                if (obj.gameObject.activeSelf)
                {
                    foreach (Transform child in obj)
                    {
                        if(decimal.Round((decimal)child.localPosition.x , 2) == 0)
                        {
                            currentButton = child.gameObject;
                            currentButton.GetComponent<GeneralCoverflowProperties>().isHovered = true;
                        }
                    }
                }
            }

            else
            {
                isHover = obj.GetComponent<GeneralCoverflowProperties>().isHovered;
                if(decimal.Round((decimal)obj.localPosition.x, 2) == 0)
                {
                    currentButton = obj.gameObject;
                    currentButton.GetComponent<GeneralCoverflowProperties>().isHovered = true;
                }
            }
        }
    }
}
