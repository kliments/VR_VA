using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideVisualizations : MonoBehaviour{

    public List<GameObject> buttons;
    public GameObject parentVis;
    public GameObject data;
    public GameObject parentData;
    public GameObject algorithms;
    public GameObject parentAlgorithms;
    public GameObject parentKmeans;
    public GameObject parentDBScan;
    public bool wasHit;
    private int sizeOfList;
    // Use this for initialization
    void Start()
    {
        if (buttons.Count == 0)
        {
            foreach (Transform child in parentVis.transform)
            {
                buttons.Add(child.gameObject);
            }
            sizeOfList = buttons.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (wasHit)
        {
            for (int i = 0; i < sizeOfList; i++)
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
        if (wasHit)
        {
            wasHit = false;
        }
        else
        {
            wasHit = true;
            parentVis.SetActive(true);
        }
        
        data.GetComponent<ShowHideDatasets>().wasHit = false;
        parentData.SetActive(false);
        
        algorithms.GetComponent<ShowHideAlgorithms>().wasHit = false;
        algorithms.GetComponent<ShowHideAlgorithms>().buttonText.text = algorithms.GetComponent<ShowHideAlgorithms>().thisText;
        parentAlgorithms.SetActive(false);

        parentKmeans.SetActive(false);
        parentDBScan.SetActive(false);
    }
}
