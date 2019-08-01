using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralCoverflowProperties : MonoBehaviour {
    public bool isHovered, rotationFinished;
    public GameObject parent, highlight;
    public List<GameObject> leftSideButtons, rightSideButtons;
    public Transform menusParent;
    private List<GameObject> allButtons;
    private Quaternion originalRotation;
    private Vector3 mainPosition;

    private float speed=0.5f;
    // Use this for initialization
    void Start()
    {
        mainPosition = new Vector3(0, 0, 0);
        leftSideButtons = new List<GameObject>();
        rightSideButtons = new List<GameObject>();
        parent = gameObject.transform.parent.gameObject;
        //menusParent = GameObject.Find("MenusParent");
        menusParent = transform;
        while(menusParent.name != "MenusParent")
        {
            menusParent = menusParent.parent;
        }
        allButtons = new List<GameObject>();
        AssignAccordinglyButtons();
        originalRotation = transform.rotation;

        foreach (Transform child in transform)
        {
            if (child.name == "HighlightedButton")
            {
                highlight = child.gameObject;
                break;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isHovered)
        {
            menusParent.GetComponent<CoverflowScript>().currentButton = gameObject;
            //rotate to be visible
            transform.localRotation = Quaternion.Lerp(transform.localRotation,Quaternion.Euler(0, 0, 0),10f*Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition,mainPosition,10*Time.deltaTime);
            if (leftSideButtons.Count != 0)
            {
                int count = leftSideButtons.Count - 1;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in leftSideButtons)
                {
                    tempPos.x = mainPosition.x + (count * (-0.02f)) - 0.08f;
                    obj.transform.localPosition = Vector3.Lerp(obj.transform.localPosition, tempPos, 10f * Time.deltaTime);
                    count--;
                    obj.transform.localRotation = Quaternion.Lerp(obj.transform.localRotation, Quaternion.Euler(0, 0, -60), 1f);

                }
            }
            if (rightSideButtons.Count != 0)
            {
                int count = 0;
                Vector3 tempPos = mainPosition;
                foreach (GameObject obj in rightSideButtons)
                {
                    tempPos.x = mainPosition.x + (count * (0.02f)) + 0.08f;
                    obj.transform.localPosition = Vector3.Lerp(obj.transform.localPosition, tempPos, 10 * Time.deltaTime);
                    count++;
                    obj.transform.localRotation = Quaternion.Lerp(obj.transform.localRotation, Quaternion.Euler(0, 0, 60), 10 * Time.deltaTime);
                }
            }
        }
        if(isHovered && !highlight.activeSelf)
        {
            highlight.SetActive(true);
        }
        else if(!isHovered && highlight.activeSelf)
        {
            highlight.SetActive(false);
        }
    }

    public void SetLeftAndRightSideButtons()
    {
        if (leftSideButtons.Count != 0)
        {
            int count = leftSideButtons.Count - 1;
            Vector3 tempPos = mainPosition;
            foreach (GameObject obj in leftSideButtons)
            {
                tempPos.x = mainPosition.x + (count * (-0.02f)) - 0.08f;
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
                tempPos.x = mainPosition.x + (count * (0.02f)) + 0.08f;
                obj.transform.localPosition = tempPos;
                count++;
                obj.transform.localRotation = Quaternion.Euler(0, 0, 60);
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
