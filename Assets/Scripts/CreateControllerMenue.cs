using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CreateControllerMenue : NetworkBehaviour
{
    public GameObject menue;

    private Transform _cameraRig, _rightController;
    private GameObject menu;
    // Use this for initialization
    void Start()
    {
        if (isServer && !isLocalPlayer)
        {
            GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        }
        Invoke("CmdSpawn", 5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    

    [Command]
    void CmdSpawn()
    {
        if (!hasAuthority) return;
        _cameraRig = GameObject.Find("[CameraRig]").transform;
        foreach (Transform child in _cameraRig)
        {
            if (child.name == "Controller (right)")
            {
                _rightController = child;
                break;
            }
        }
        menu = Instantiate(menue, transform.position, Quaternion.identity, _rightController);
        NetworkServer.SpawnWithClientAuthority(menu, connectionToClient);
        menu.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        RpcSpawn();
    }

    [ClientRpc]
    void RpcSpawn()
    {
        if (!hasAuthority) return;
        _cameraRig = GameObject.Find("[CameraRig]").transform;
        foreach (Transform child in _cameraRig)
        {
            if (child.name == "Controller (right)")
            {
                _rightController = child;
                break;
            }
        }
        menu = Instantiate(menue, transform.position, Quaternion.identity, _rightController);
        menu.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
    }
}
