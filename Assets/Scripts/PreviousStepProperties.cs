using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousStepProperties : MonoBehaviour {
    public List<Color> colorList;
    public Color originalColor;
	// Use this for initialization
	void Start () {
        colorList = new List<Color>();
        originalColor = GetComponent<MeshRenderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
