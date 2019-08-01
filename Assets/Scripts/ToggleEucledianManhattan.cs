using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ToggleEucledianManhattan : NetworkBehaviour {
    public SpriteRenderer sprite;
    public Sprite euclideanSpr, mannhattanSpr;
    private DBScanAlgorithm distance;

    [SyncVar]
    private bool eucledian;
	// Use this for initialization
	void Start () {
        distance = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        eucledian = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    public void CmdToggle()
    {
        if (!isServer) return;
        //reset DBSCAN algorithm
        distance.ResetMe();
        if (eucledian)
        {
            eucledian = false;
            distance.euclDist = true;
            if (sprite == null) sprite = GameObject.Find("EucledianManhattan").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = euclideanSpr;
        }
        else
        {
            eucledian = true;
            distance.euclDist = false;
            if (sprite == null) sprite = GameObject.Find("EucledianManhattan").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = mannhattanSpr;
        }
        RpcToggle();
    }

    [ClientRpc]
    void RpcToggle()
    {
        if (isServer || !isLocalPlayer) return;
        //reset DBSCAN algorithm
        distance.ResetMe();
        if (eucledian)
        {
            eucledian = false;
            distance.euclDist = true;
            if (sprite == null) sprite = GameObject.Find("EucledianManhattan").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = euclideanSpr;
        }
        else
        {
            eucledian = true;
            distance.euclDist = false;
            if (sprite == null) sprite = GameObject.Find("EucledianManhattan").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = mannhattanSpr;
        }
    }
}
