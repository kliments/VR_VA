using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSingleMultiCentered : MonoBehaviour {
    public SpriteRenderer sprite;
    public Sprite singleCenSpr, multiCenSpr;
    private DenclueAlgorithm tiledmap;
    private bool multiCentered;
    // Use this for initialization
    void Start()
    {
<<<<<<< Updated upstream
        tiledmap = (TiledmapGeneration)FindObjectOfType(typeof(TiledmapGeneration));
        multiCentered = true;
=======
        tiledmap = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        multiCentered = false;
>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Toggle()
    {
        //reset DBSCAN algorithm
        tiledmap.ResetMe();
        if (multiCentered)
        {
            multiCentered = false;
            tiledmap.multiCentered = true;
            sprite.sprite = multiCenSpr;
        }
        else
        {
            multiCentered = true;
            tiledmap.multiCentered = false;
            sprite.sprite = singleCenSpr;
        }
    }
}
