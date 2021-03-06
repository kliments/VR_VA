﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ToggleSingleMultiCentered : NetworkBehaviour {
    public SpriteRenderer sprite;
    public Sprite singleCenSpr, multiCenSpr;
    public DenclueAlgorithm denclue;
    private bool multiCentered;
    // Use this for initialization
    void Start()
    {
        multiCentered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            CmdToggle();
        }
    }

    [Command]
    public void CmdToggle()
    {
        if (multiCentered)
        {
            multiCentered = false;
            denclue.multiCentered = true;
            if (sprite == null) sprite = GameObject.Find("SingleMultiCentered").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            denclue.multiCentered = false;
            if (sprite == null) sprite = GameObject.Find("SingleMultiCentered").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = singleCenSpr;
        }
        //Call same function on each client
        RpcToggle();
        //Start denclue on server -> followed up by clients
        denclue.CmdStartDenclue();
    }

    [ClientRpc]
    public void RpcToggle()
    {
        if (isServer || !isLocalPlayer) return;
        if (multiCentered)
        {
            multiCentered = false;
            denclue.multiCentered = true;
            if (sprite == null) sprite = GameObject.Find("SingleMultiCentered").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            denclue.multiCentered = false;
            if (sprite == null) sprite = GameObject.Find("SingleMultiCentered").transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = singleCenSpr;
        }
    }
}
