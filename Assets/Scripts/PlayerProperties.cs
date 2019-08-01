using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerProperties : NetworkBehaviour {
    
    [SyncVar]
    public Color color;
	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().material.color = color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
