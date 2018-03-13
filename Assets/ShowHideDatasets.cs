using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideDatasets : MonoBehaviour {
    
    public List<GameObject> buttons;
    public GameObject parentData;
    public GameObject parentVis;
    public GameObject parentAlgorithms;
    public bool wasHit;
    private int sizeOfList;
	// Use this for initialization
	void Start () {
        //show only dataset buttons in the start of the application
        if(this.name == "Datasets")
        {
            //wasHit = true;
            GetComponent<Animator>().SetBool("selected", true);
        }
        if (buttons.Count == 0)
        {
            foreach (Transform child in parentData.transform)
            {
                buttons.Add(child.gameObject);
            }
            sizeOfList = buttons.Count;
        }
    }
	
	// Update is called once per frame
	void Update()
    {
		if(wasHit)
        {
            for(int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(false); 
            }
        }
	}

    public void Toggle()
    {
        if(wasHit)
        {
            wasHit = false;
            GetComponent<Animator>().SetBool("selected", false);
        }
        else
        {
            wasHit = true;
            GetComponent<Animator>().SetBool("selected", true);
        }

        parentVis.GetComponent<Animator>().SetBool("selected", false);
        parentVis.GetComponent<ShowHideVisualizations>().wasHit = false;

        parentAlgorithms.GetComponent<Animator>().SetBool("selected", false);
        parentAlgorithms.GetComponent<ShowHideAlgorithms>().wasHit = false;
    }
}
