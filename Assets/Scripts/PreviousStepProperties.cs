using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousStepProperties : MonoBehaviour {
    public List<Color> colorList;
    public Color originalColor;
	// Use this for initialization
	void Start ()
    {
        originalColor = GetComponent<MeshRenderer>().material.color;
        colorList = new List<Color>();
        colorList.Add(originalColor);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
