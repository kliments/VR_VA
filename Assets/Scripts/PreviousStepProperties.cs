using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousStepProperties : MonoBehaviour {
    public List<Color> colorList;
	// Use this for initialization
	void Start () {
        colorList = new List<Color>();
        colorList.Add(GetComponent<Renderer>().material.color);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
