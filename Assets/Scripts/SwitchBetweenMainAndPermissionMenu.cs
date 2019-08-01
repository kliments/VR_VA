using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SwitchBetweenMainAndPermissionMenu : MonoBehaviour {
    private SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    public GameObject mainMenu, permissionMenu, player;
	// Use this for initialization
	void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        //player = FindLocalPlayer();
    }
	
	// Update is called once per frame
	void Update () {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            if (mainMenu.activeSelf)
            {
                mainMenu.SetActive(false);
                permissionMenu.SetActive(true);
            }
            else
            {
                mainMenu.SetActive(true);
                permissionMenu.SetActive(false);
            }
        }
	}

    private GameObject FindLocalPlayer()
    {
        NetworkManager networkManager = NetworkManager.singleton;
        List<PlayerController> pcs = networkManager.client.connection.playerControllers;
        foreach(var pc in pcs)
        {
            GameObject obj = pc.gameObject;
            NetworkBehaviour netBhv = obj.GetComponent<NetworkBehaviour>();
            if(pc.IsValid && netBhv != null && netBhv.isLocalPlayer)
            {
                return obj;
            }
        }
        return null;
    }
}
