using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideDBSCANAlgorithm : MonoBehaviour {
    public List<GameObject> buttons;
    public GameObject parentButtons;
    public GameObject parentAlgorithms;
    public GameObject parentKMeans;
    public GameObject algorithms;
    public GameObject resetKMeans;
    public bool wasHit;
    private int sizeOfList;
    public GameObject algorithmText;
    public string thisText;
    // Use this for initialization
    void Start () {
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
	void Update () {
		
	}

    public void Toggle()
    {
        if (wasHit)
        {
            wasHit = false;
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(true);
            }
            //change the text of the Algorithms button to this one (either K-means or DBSCAN)
            algorithmText.GetComponent<Text>().text = thisText;
            algorithms.GetComponent<ShowHideAlgorithms>().wasHit = false;
            parentButtons.SetActive(true);
            parentAlgorithms.SetActive(false);
        }
        else
        {
            wasHit = true;
            for (int i = 0; i < sizeOfList; i++)
            {
                buttons[i].SetActive(false);
            }
            parentButtons.SetActive(false);
            parentAlgorithms.SetActive(true);
        }
        parentKMeans.SetActive(false);
        resetKMeans.GetComponent<KMeansAlgorithm>().ResetMe();
    }

    void OnEnable()
    {
        wasHit = true;
    }
}
