using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CreateServerObjects : NetworkBehaviour
{
    public GameObject datasetChanger;
    // Use this for initialization
    void Start()
    {
        CmdCreateObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command]
    void CmdCreateObjects()
    {
        if (!isServer || !hasAuthority) return;
        GameObject dataChanger = Instantiate(datasetChanger);
        NetworkServer.Spawn(dataChanger);
        RpcCreateObjects();
    }

    [ClientRpc]
    void RpcCreateObjects()
    {
        if (!hasAuthority || isServer) return;
        GameObject dataChanger = Instantiate(datasetChanger);
    }
}
