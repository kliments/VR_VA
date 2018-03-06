using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideButtons : MonoBehaviour {

    public List<GameObject> buttons;
    public GameObject parentData;
    public GameObject parentVis;
    public GameObject parentKmeans;
    public bool wasHit;
    private int sizeOfList;
    // Use this for initialization
    void Start()
    {
        if (buttons.Count == 0)
        {
            foreach (Transform child in parentKmeans.transform)
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
        }

        parentVis.GetComponent<Animator>().SetBool("selected", false);
        parentVis.GetComponent<ShowHideVisualizations>().wasHit = false;

        parentData.GetComponent<Animator>().SetBool("selected", false);
        parentData.GetComponent<ShowHideDatasets>().wasHit = false;
    }
}
