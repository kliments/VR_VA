using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextOfAlgorithmButton : MonoBehaviour {
    public GameObject algorithmText;
    public string thisText;
	// Use this for initialization
	void Start () {
        thisText = transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void ChangeText()
    {
        algorithmText.GetComponent<Text>().text = thisText;
    }
}
