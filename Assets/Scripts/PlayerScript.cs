using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class PlayerScript : NetworkBehaviour {
    public Transform mainCamera;
    public GameObject test;
	// Use this for initialization
	void Start () {
        mainCamera = Camera.main.transform;
        if(Camera.main.transform.name.Contains("head"))
        {
            mainCamera = Camera.main.transform.GetChild(0);
        }
        if(hasAuthority)
        {
            StartCoroutine(TrackHeadMovement());
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if(isLocalPlayer)
            {
                CmdSendCallToServer();
            }
        }
	}

    IEnumerator TrackHeadMovement()
    {
        while(true)
        {
            transform.position = mainCamera.position;
            transform.rotation = mainCamera.rotation;
            yield return null;
        }
    }

    [Command]
    void CmdSendCallToServer()
    {
        Debug.Log("This is server");
        GameObject cube = Instantiate(test);
        NetworkServer.Spawn(cube);
        //RpcServerToClient(cube);
    }

    [ClientRpc]
    void RpcServerToClient(GameObject obj)
    {
        Debug.Log("This is client" + netId);
        if(!isLocalPlayer)
        {
            GameObject cube = Instantiate(obj);
        }
    }
}
