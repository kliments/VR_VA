using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideKMeansButtons : MonoBehaviour {

    public List<GameObject> buttons;
    public GameObject parentButtons;
    public GameObject parentAlgorithms;
    public GameObject parentDBSCAN;
    public bool wasHit;
    private int sizeOfList;
    public GameObject algorithmText;
    public string thisText;
    // Use this for initialization
    void Start()
    {
        thisText = transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text;
        if (buttons.Count == 0)
        {
            foreach (Transform child in parentButtons.transform)
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
            parentAlgorithms.SetActive(false);
            parentDBSCAN.SetActive(false);
            
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
        for (int i = 0; i < sizeOfList; i++)
        {
            buttons[i].SetActive(true);
        }
        if (wasHit)
        {
            wasHit = false;
        }
        else
        {
            wasHit = true;
            parentAlgorithms.GetComponent<Animator>().SetBool("selected", true);
            //change the text of the Algorithms button to this one (either K-means or DBSCAN)
            algorithmText.GetComponent<Text>().text = thisText;
        }
    }

    void OnEnable()
    {
        wasHit = false;
    }
}
