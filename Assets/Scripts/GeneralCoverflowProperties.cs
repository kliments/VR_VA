using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralCoverflowProperties : MonoBehaviour {
    public bool isHovered;
    public GameObject parent;
    public List<GameObject> leftSideButtons, rightSideButtons;
    private List<GameObject> allButtons;
    private Quaternion originalRotation;
    private Vector3 mainPosition, ptrOldPos, ptrNewPos;
    public GameObject ptr, menusParent;
    // Use this for initialization
    void Start()
    {
        mainPosition = new Vector3(0, 0, 0);
        leftSideButtons = new List<GameObject>();
        rightSideButtons = new List<GameObject>();
        parent = gameObject.transform.parent.gameObject;
        ptr = GameObject.Find("Pointer");
        menusParent = GameObject.Find("MenusParent");
        allButtons = new List<GameObject>();
        AssignAccordinglyButtons();
        originalRotation = transform.rotation;
        ptrOldPos = ptr.transform.localPosition;
        ptrNewPos = ptr.transform.localPosition;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isHovered)
        {
            menusParent.GetComponent<CoverflowScript>().currentButton = this.gameObject;
            //rotate to be visible
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            transform.localPosition = mainPosition;
            if (leftSideButtons.Count != 0)
            {
                int count = leftSideButtons.Count - 1;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in leftSideButtons)
                {
                    if (count == 0)
                    {
                        tempPos.x = mainPosition.x - 0.06f;
                    }
                    else
                    {
                        tempPos.x = mainPosition.x + (count * (-0.02f)) - 0.06f;
                    }
                    obj.transform.localPosition = tempPos;
                    count--;
                    obj.transform.localRotation = Quaternion.Euler(0, 0, -60);
                }
            }
            if (rightSideButtons.Count != 0)
            {
                int count = 0;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in rightSideButtons)
                {
                    if (count == 0)
                    {
                        tempPos.x = mainPosition.x + 0.06f;
                    }
                    else
                    {
                        tempPos.x = mainPosition.x + (count * (0.02f)) + 0.06f;
                    }
                    obj.transform.localPosition = tempPos;
                    count++;
                    obj.transform.localRotation = Quaternion.Euler(0, 0, 60);
                }
            }
        }
    }

    void AssignAccordinglyButtons()
    {
        foreach(Transform obj in parent.transform)
        {
            allButtons.Add(obj.gameObject);
        }
        if (gameObject == allButtons[0])
        {
            for (int i = 1; i < allButtons.Count; i++)
            {
                rightSideButtons.Add(allButtons[i]);
            }
        }
        else if (gameObject == allButtons[allButtons.Count - 1])
        {
            for (int i = 0; i < allButtons.Count - 1; i++)
            {
                leftSideButtons.Add(allButtons[i]);
            }
        }
        else
        {
            for (int i = 0; i < allButtons.Count; i++)
            {
                if (i < allButtons.IndexOf(gameObject))
                {
                    leftSideButtons.Add(allButtons[i]);
                }
                else if (i == allButtons.IndexOf(gameObject))
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
