using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class shaderChanger : MonoBehaviour {

    public SpriteRenderer alpha;

	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().material.shader = alpha.material.shader;
        alpha.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
