using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssignAuthorityOverObject : NetworkBehaviour {
    public enum Rights {data, viz, kMeans, dbScan, denclue};

    public Rights rights = new Rights();
    RaycastHit hit;
    Ray ray;
    GameObject dataChanger, vizChanger, kMeansContr, dbScanContr, denclueContr, silhouetteCoefContr, pseudoCodeContr;
    // Use this for initialization
    void Start () {
        FindAllObjects();
        rights = Rights.data;
	}
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (isServer && isLocalPlayer)
            {
                ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "Player")
                    {
                        switch(rights)
                        {
                            case Rights.data:
                                hit.collider.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(dataChanger.GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>());
                                break;
                            case Rights.viz:
                                hit.collider.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(vizChanger.GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>());
                                break;
                            case Rights.dbScan:
                                hit.collider.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(dbScanContr.GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>());
                                break;
                            case Rights.denclue:
                                hit.collider.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(denclueContr.GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>());
                                break;
                            case Rights.kMeans:
                                hit.collider.GetComponent<AssignAuthorityOverObject>().CmdAssignAuthority(kMeansContr.GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>());
                                break;
                        }
                        
                    }
                }
            }
        }
    }

    void FindAllObjects()
    {
        dataChanger = GameObject.Find("DatasetChanger(Clone)");
        vizChanger = GameObject.Find("VisualizationChanger(Clone)");
        kMeansContr = GameObject.Find("KMeansAlgorithmController(Clone)");
        dbScanContr = GameObject.Find("DBScanAlgorithmController(Clone)");
        denclueContr = GameObject.Find("DenclueAlgorithmController(Clone)");
        silhouetteCoefContr = GameObject.Find("SilhouetteCoefficient(Clone)");
        pseudoCodeContr = GameObject.Find("PseudoTextChanger(Clone)");
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
