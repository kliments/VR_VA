using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideMirror : MonoBehaviour {
    public bool isVisible;
    public GameObject mirror;
    public Text text;
	// Use this for initialization
	void Start () {
        isVisible = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleMirror()
    {
        if(isVisible)
        {
            isVisible = false;
            mirror.SetActive(false);
            text.text = "Show Mirror";
        }
        else
        {
            isVisible = true;
            mirror.SetActive(true);
            text.text = "Hide Mirror";
        }
    }
}
