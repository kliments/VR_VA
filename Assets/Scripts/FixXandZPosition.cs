using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixXandZPosition : MonoBehaviour {
    Vector3 oldPos, newPos;
    Quaternion rot;
    public bool isTaken;
    public DenclueAlgorithm denclue;
    public Text buttonText;
    // Use this for initialization
    void Start () {
        oldPos = transform.position;
        newPos = transform.position;
        rot = transform.localRotation;
        isTaken = false;
        denclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.Alpha8))
        {
            transform.position += new Vector3(0, 0.1f, 0);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            transform.position -= new Vector3(0, 0.1f, 0);
        }

        if(isTaken && (decimal.Round((decimal)transform.position.y, 5) != (decimal)denclue.GetComponent<DenclueAlgorithm>().threshold))
        {
            newPos = transform.position;
            newPos.x = oldPos.x;
            newPos.z = oldPos.z;
            transform.position = newPos;
            transform.rotation = rot;
            denclue.threshold = transform.position.y;
            buttonText.text = "ξ: " + decimal.Round((decimal)transform.position.y).ToString();
        }
        else if(!isTaken && transform.parent!= null)
        {
            transform.parent = null;
        }
        if(transform.position.y < 0.0022f)
        {
            newPos = transform.position;
            newPos.y = 0.0022f;
            transform.position = newPos;
        }
        oldPos = newPos;
        newPos = transform.position;
        if(oldPos != newPos)
        {
            denclue.threshold = transform.position.y;
            if (denclue.gaussianCalculation)
            {
                if (denclue.multiCentered) denclue._multiCenteredGaussian = true;
                else denclue._singleCenteredGaussian = true;
            }
            else
            {
                if (denclue.multiCentered) denclue._multiCenteredSquareWave = true;
                else denclue._singleCenteredSquaredWave = true;
            }
        }
	}
}
