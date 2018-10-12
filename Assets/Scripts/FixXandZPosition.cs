using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixXandZPosition : MonoBehaviour {
    Vector3 oldPos, newPos;
    Quaternion rot;
    public bool isTaken;
    public GameObject denclue;
    public Text buttonText;
    // Use this for initialization
    void Start () {
        oldPos = transform.localPosition;
        rot = transform.localRotation;
        isTaken = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(isTaken && (decimal.Round((decimal)transform.position.y, 5) != (decimal)denclue.GetComponent<TiledmapGeneration>().threshold))
        {
            newPos = transform.position;
            newPos.x = oldPos.x;
            newPos.z = oldPos.z;
            transform.position = newPos;
            transform.rotation = rot;
            denclue.GetComponent<TiledmapGeneration>().threshold = transform.position.y;
            buttonText.text = "ξ: " + decimal.Round((decimal)transform.position.y).ToString();
        }
        else if(!isTaken && transform.parent!= null)
        {
            transform.parent = null;
        }
        if(transform.position.y < 0.0021f)
        {
            newPos = transform.position;
            newPos.y = 0.0021f;
            transform.position = newPos;
        }
	}
}
