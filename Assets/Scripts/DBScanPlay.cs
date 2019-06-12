using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DBScanPlay : NetworkBehaviour {
    public DBScanAlgorithm dbScanButton;
    public bool play;
    private bool allClustersFound, nextStepPause;
    public Sprite playSprite, pauseSprite;
    public SpriteRenderer sprite;
    // Use this for initialization
    void Start ()
    {
        nextStepPause = false;
    }
	
	// Update is called once per frame
	void Update () {

    }


    public void TogglePlayPause()
    {
        if (!nextStepPause)
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
        //call the DBSCAN script every 1 seconds
        StartCoroutine("DBScanRepeat");
    }

    public void StopRoutine()
    {
        StopCoroutine("DBScanRepeat");
    }

    IEnumerator DBScanRepeat()
    {
        allClustersFound = dbScanButton.allClustersFound;

        while (!allClustersFound)
        {
            dbScanButton.CmdStartDBSCAN();
            yield return new WaitForSeconds(1f);
        }

    }
}
