using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SquareGaussian : NetworkBehaviour {
    public DenclueAlgorithm denclue;
    public SpriteRenderer sprite;
    public Sprite gaussian, square;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleSquareGaussian()
    {
        if (denclue.gaussianCalculation == true)
        {
            denclue.gaussianCalculation = false;
            sprite.sprite = square;
        }
        else
        {
            denclue.gaussianCalculation = true;
            sprite.sprite = gaussian;
        }
        denclue.ResetMe();
        denclue.CmdStartDenclue();
    }
}
