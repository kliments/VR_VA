using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssignAuthorityOverObject : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    public void CmdAssignAuthority(NetworkInstanceId objID, NetworkIdentity player)
    {
        GameObject localObj = NetworkServer.FindLocalObject(objID);
        NetworkIdentity networkIdentity = localObj.GetComponent<NetworkIdentity>();
        NetworkConnection otherOwner = networkIdentity.clientAuthorityOwner;

        if(otherOwner == player.connectionToClient)
        {
            return;
        }
        else
        {
            if(otherOwner != null)
            {
                networkIdentity.RemoveClientAuthority(otherOwner);
            }
            networkIdentity.AssignClientAuthority(player.connectionToClient);
        }
    }
}
