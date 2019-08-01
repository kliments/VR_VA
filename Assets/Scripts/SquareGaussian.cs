using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SquareGaussian : NetworkBehaviour {
    public DenclueAlgorithm denclue;
    public SpriteRenderer sprite;
    public Sprite gaussian, square;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.T))
        {
            CmdToggleSquareGaussian();
        }
	}

    [Command]
    public void CmdToggleSquareGaussian()
    {
        if (denclue.gaussianCalculation == true)
        {
            denclue.gaussianCalculation = false;
            if (sprite == null) sprite = GameObject.Find("SquareGaussian").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = square;
        }
        else
        {
            denclue.gaussianCalculation = true;
            if (sprite == null) sprite = GameObject.Find("SquareGaussian").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = gaussian;
        }
        denclue.ResetMe();
        //call same function on each client
        RpcToggleSquareGaussian();
        //Start Denclue on server -> follows up by clients
        denclue.CmdStartDenclue();
    }

    [ClientRpc]
    public void RpcToggleSquareGaussian()
    {
        if (isServer || !isLocalPlayer) return;
        if (denclue.gaussianCalculation == true)
        {
            denclue.gaussianCalculation = false;
            if (sprite == null) sprite = GameObject.Find("SquareGaussian").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = square;
        }
        else
        {
            denclue.gaussianCalculation = true;
            if (sprite == null) sprite = GameObject.Find("SquareGaussian").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = gaussian;
        }
        denclue.ResetMe();
    }
}
