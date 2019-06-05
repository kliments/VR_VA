using System.Collections;
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
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            denclue.multiCentered = false;
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
        if (hasAuthority) return;
        if (multiCentered)
        {
            multiCentered = false;
            denclue.multiCentered = true;
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            denclue.multiCentered = false;
            sprite.sprite = singleCenSpr;
        }
    }
}
