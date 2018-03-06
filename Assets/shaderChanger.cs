using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class shaderChanger : MonoBehaviour {

    public SpriteRenderer alpha;
    private Shader currentShader, alphaShader;

	// Use this for initialization
	void Start () {
        currentShader = GetComponent<MeshRenderer>().material.shader;
        alphaShader = alpha.material.shader;
        ChangeToAlphaShader();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void ChangeToAlphaShader()
    {
        GetComponent<MeshRenderer>().material.shader = alpha.material.shader;
    }

    public void ChangeToCurrentShader()
    {
        GetComponent<MeshRenderer>().material.shader = currentShader;
    }
}
