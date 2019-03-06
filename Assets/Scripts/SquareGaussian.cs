using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareGaussian : MonoBehaviour {
    public GameObject denclue;
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
        if (denclue.GetComponent<DenclueAlgorithm>().gaussianCalculation == true)
        {
            denclue.GetComponent<DenclueAlgorithm>().gaussianCalculation = false;
            sprite.sprite = square;
        }
        else
        {
            denclue.GetComponent<DenclueAlgorithm>().gaussianCalculation = true;
            sprite.sprite = gaussian;
        }
        denclue.GetComponent<DenclueAlgorithm>().ResetMe();
    }
}
