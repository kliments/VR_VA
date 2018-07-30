using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideMirror : MonoBehaviour {
    public bool isVisible;
    public GameObject mirror;
    private int counter;
    public Sprite showMirror, hideMirror;
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
        if(isVisible)
        {
            counter = 0;
            isVisible = false;
            mirror.SetActive(false);
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = showMirror;
        }
        else if(!isVisible)
        {
            counter = 0;
            isVisible = true;
            mirror.SetActive(true);
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = hideMirror;
        }
    }
}
