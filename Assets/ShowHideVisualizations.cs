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
            GetComponent<Animator>().SetBool("selected", false);
        }
        else
        {
            wasHit = true;
            GetComponent<Animator>().SetBool("selected", true);
            parentVis.SetActive(true);
        }

        data.GetComponent<Animator>().SetBool("selected", false);
        data.GetComponent<ShowHideDatasets>().wasHit = false;
        parentData.SetActive(false);

        algorithms.GetComponent<Animator>().SetBool("selected", false);
        algorithms.GetComponent<ShowHideAlgorithms>().wasHit = false;
        parentAlgorithms.SetActive(false);

        parentKmeans.SetActive(false);
    }
}
