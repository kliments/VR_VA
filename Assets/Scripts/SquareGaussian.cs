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
        if (denclue.GetComponent<TiledmapGeneration>().gaussianCalculation == true)
        {
            denclue.GetComponent<TiledmapGeneration>().gaussianCalculation = false;
            sprite.sprite = square;
        }
        else
        {
            denclue.GetComponent<TiledmapGeneration>().gaussianCalculation = true;
            sprite.sprite = gaussian;
        }
        denclue.GetComponent<TiledmapGeneration>().ResetMe();
    }
}
