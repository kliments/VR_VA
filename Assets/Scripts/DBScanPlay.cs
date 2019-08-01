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


    [Command]
    public void CmdTogglePlayPause()
    {
        if (!isServer) return;
        if (!nextStepPause)
        {
            StartRoutine();
            nextStepPause = true;
            //change label
            if (sprite == null) sprite = GameObject.Find("Play").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = playSprite;
        }
        else
        {
            StopRoutine();
            nextStepPause = false;
            //change label
            if (sprite == null) sprite = GameObject.Find("Play").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = pauseSprite;
        }
        RpcTogglePlayPause();
    }
    [ClientRpc]
    void RpcTogglePlayPause()
    {
        if (isServer || !isLocalPlayer) return;
        if (!nextStepPause)
        {
            StartRoutine();
            nextStepPause = true;
            //change label
            if (sprite == null) sprite = GameObject.Find("Play").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = playSprite;
        }
        else
        {
            StopRoutine();
            nextStepPause = false;
            //change label
            if (sprite == null) sprite = GameObject.Find("Play").transform.GetChild(0).GetComponent<SpriteRenderer>();
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
