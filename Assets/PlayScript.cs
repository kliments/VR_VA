using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScript : MonoBehaviour {

    public GameObject nextStepObj;
    public bool buttonWasPressed = false;
    private bool iterationHasFinished;

	// Use this for initialization
	void Start () {

    }

	
	// Update is called once per frame
	void Update () {
		if(buttonWasPressed)
        {
            StartRoutine();
            buttonWasPressed = false;
        }
	}

    void StartRoutine()
    {
        //call the KMeansAlgorithm script every 1 seconds from the nextStepObj
        StartCoroutine("KMeansRepeat");
    }
    
    public void StopRoutine()
    {
        StopCoroutine("KMeansRepeat");
    }

    IEnumerator KMeansRepeat()
    {
        iterationHasFinished = nextStepObj.GetComponent<KMeansAlgorithm>().bestClusterFound;

        while (!iterationHasFinished)
        {
            nextStepObj.GetComponent<KMeansAlgorithm>().StartAlgorithm();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
}
