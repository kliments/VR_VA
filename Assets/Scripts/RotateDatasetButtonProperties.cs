using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDatasetButtonProperties : MonoBehaviour {
    public bool isHovered;
    private ResponsiveMenuDatasetsGenerator datasetGenerator;
    public List<GameObject> leftSideButtons, rightSideButtons;
    private List<GameObject> allButtons;
    private Quaternion originalRotation;
    private Vector3 mainPosition;
    public GameObject sprite, ptr, menusParent;
    private Vector3 ptrOldPos, ptrNewPos;
    public Sprite mySprite;
    // Use this for initialization
    void Start () {
        mainPosition = new Vector3(0, 0, 0);
        datasetGenerator = (ResponsiveMenuDatasetsGenerator)FindObjectOfType(typeof(ResponsiveMenuDatasetsGenerator));
        leftSideButtons = new List<GameObject>();
        rightSideButtons = new List<GameObject>();
        allButtons = datasetGenerator.datasetButtons;
        AssignAccordinglyButtons();
        originalRotation = transform.rotation;
        sprite = GameObject.Find("PCAProjectionSprite");
        ptr = GameObject.Find("Pointer");
        menusParent = GameObject.Find("MenusParent");
        ptrOldPos = ptr.transform.localPosition;
        ptrNewPos = ptr.transform.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
		if(isHovered)
        {
            menusParent.GetComponent<CoverflowScript>().currentButton = this.gameObject;
            //rotate to be visible
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            transform.localPosition = mainPosition;
            sprite.GetComponent<SpriteRenderer>().sprite = mySprite;
            if (leftSideButtons.Count != 0)
            {
                int count = leftSideButtons.Count-1;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in leftSideButtons)
                {
                    if(count == 0)
                    {
                        tempPos.x = mainPosition.x -0.06f;
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
            if(rightSideButtons.Count != 0)
            {
                int count = 0;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in rightSideButtons)
                {
                    if(count == 0)
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
        /*else if(!isHovered && toChange)
        {
            //rotate back to not visible
            //if we move the pointer to the left
            if(ptrOldPos.x < ptrNewPos.x)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 60);
            }
            //if we move the pointer to the right
            else
            {
                transform.localRotation = Quaternion.Euler(0, 0, -60);
            }
            
            if (leftSideButtons.Count != 0)
            {
                foreach (GameObject obj in leftSideButtons)
                {
                    obj.transform.localPosition += new Vector3(0.05f, 0f, 0f);
                }
            }
            if (rightSideButtons.Count != 0)
            {
                foreach (GameObject obj in rightSideButtons)
                {
                    obj.transform.localPosition += new Vector3(-0.05f, 0f, 0f);
                }
            }
            toChange = false;
            toChangeBack = true;
        }*/
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
