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
    public GameObject parentDBScan;
    public Text buttonText;
    public bool wasHit;
    private int sizeOfList;
    public string thisText;

    // Use this for initialization
    void Start ()
    {
        thisText = buttonText.text;
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
    }

    public void Toggle()
    {
        if (!wasHit)
        {
            wasHit = true;

            parentAlgorithms.SetActive(true);
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(true);
            }
        }
        else
        {
            wasHit = false;
            
            parentAlgorithms.SetActive(false);
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(false);
            }
        }

        //change button text to "Algorithms"
        buttonText.text = thisText;
        
        parentData.GetComponent<ShowHideDatasets>().wasHit = false;
        
        parentVis.GetComponent<ShowHideVisualizations>().wasHit = false;
        
        parentKmeans.SetActive(false);
        parentDBScan.SetActive(false);

    }
}
