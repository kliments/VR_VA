using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayScript : NetworkBehaviour {

    public KMeansAlgorithm nextStepObj;
    public Sprite playSprite, pauseSprite;
    public SpriteRenderer sprite;
    private bool nextStepPause;

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
            sprite.sprite = pauseSprite;
        }
        else
        {
            StopRoutine();
            nextStepPause = false;
            //change label
            sprite.sprite = playSprite;
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
        while (!nextStepObj.bestClusterFound)
        {
            nextStepObj.StartAlgorithm();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
}
