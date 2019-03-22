using UnityEngine;
using System.Collections;

// For use with Photon and SteamVR
public class NetworkedPlayer : Photon.MonoBehaviour
{
    public GameObject avatar;

    public Transform playerGlobal;
    public Transform playerLocal;

    void Start()
    {
        Debug.Log("Player instantiated");

        if (photonView.isMine)
        {
            Debug.Log("Player is mine");

            playerGlobal = GameObject.Find("[CameraRig]").transform;
            playerLocal = playerGlobal.Find("Camera (head)");

            this.transform.SetParent(playerLocal);
            this.transform.localPosition = Vector3.zero;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(playerGlobal.position);
            stream.SendNext(playerGlobal.rotation);
            stream.SendNext(playerLocal.localPosition);
            stream.SendNext(playerLocal.localRotation);
        }
        else
        {
            this.transform.position = (Vector3)stream.ReceiveNext();
            this.transform.rotation = (Quaternion)stream.ReceiveNext();
            avatar.transform.localPosition = (Vector3)stream.ReceiveNext();
            avatar.transform.localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}