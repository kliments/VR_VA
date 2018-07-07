using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScript : MonoBehaviour {

    public GameObject nextStepObj;
    public Sprite playSprite, pauseSprite;
    public SpriteRenderer sprite;
    private bool iterationHasFinished, nextStepPause;

	// Use this for initialization
	void Start () {
        nextStepPause = false;
    }

	
	// Update is called once per frame
	void Update () {

	}

    public void TogglePlayPause()
    {
        if(!nextStepPause)
        {
            StartRoutine();
            nextStepPause = true;
            //change label
            sprite.sprite = playSprite;
        }
        else
        {
            StopRoutine();
            nextStepPause = false;
            //change label
            sprite.sprite = pauseSprite;
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
