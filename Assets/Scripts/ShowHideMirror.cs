using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideMirror : MonoBehaviour {
    public bool isVisible;
    public GameObject mirror;
    public Text text;
    private int counter;
	// Use this for initialization
	void Start () {
        isVisible = false;
        counter = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleMirror()
    {
        //need the counter, because for some reason this function was being called twice with one press
        counter++;
        if(isVisible && counter==2)
        {
            isVisible = false;
            mirror.SetActive(false);
            text.text = "Show Mirror";
            counter = 0;
        }
        else if(!isVisible && counter==2)
        {
            isVisible = true;
            mirror.SetActive(true);
            text.text = "Hide Mirror";
            counter = 0;
        }
    }
}
