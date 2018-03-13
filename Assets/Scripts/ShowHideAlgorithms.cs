using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideAlgorithms : MonoBehaviour {
    public List<GameObject> buttons;
    public GameObject parentVis;
    public GameObject parentData;
    public GameObject parentAlgorithms;
    public GameObject parentKmeans;
    public bool wasHit;
    private int sizeOfList;
    public string thisText;

    // Use this for initialization
    void Start ()
    {
        thisText = transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text;
        if (buttons.Count == 0)
        {
            foreach (Transform child in parentAlgorithms.transform)
            {
                buttons.Add(child.gameObject);
            }
            sizeOfList = buttons.Count;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (wasHit)
        {
        }
        else
        {
        }
    }

    public void Toggle()
    {
        if (!wasHit)
        {
            //whenever button is clicked, return the normal text
            transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = thisText;
            wasHit = true;
            GetComponent<Animator>().SetBool("selected", true);

            parentAlgorithms.SetActive(true);
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(true);
            }
        }
        else
        {
            //whenever button is clicked, return the normal text
            transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = thisText;
            wasHit = false;
            GetComponent<Animator>().SetBool("selected", false);
            
            parentAlgorithms.SetActive(false);
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(false);
            }
        }

        parentData.GetComponent<Animator>().SetBool("selected", false);
        parentData.GetComponent<ShowHideDatasets>().wasHit = false;

        parentVis.GetComponent<Animator>().SetBool("selected", false);
        parentVis.GetComponent<ShowHideVisualizations>().wasHit = false;
        
        parentKmeans.SetActive(false);

    }
}
