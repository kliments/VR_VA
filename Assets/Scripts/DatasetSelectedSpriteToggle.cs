using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetSelectedSpriteToggle : MonoBehaviour {

    public GameObject spriteObj, datasetSelectedSprite;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSprite()
    {
        if(spriteObj==null || datasetSelectedSprite==null)
        {
            datasetSelectedSprite = gameObject;
            //checkmark that dataset has been selected
            while(datasetSelectedSprite.name != "MenusParent")
            {
                datasetSelectedSprite = datasetSelectedSprite.transform.parent.gameObject;
            }
            foreach(Transform obj in datasetSelectedSprite.transform)
            {
                if(obj.name == "DatasetSelected")
                {
                    datasetSelectedSprite = obj.gameObject;
                    break;
                }
            }
            spriteObj = transform.parent.parent.GetChild(0).GetChild(0).GetChild(0).gameObject;
        }
        spriteObj.SetActive(true);
        datasetSelectedSprite.SetActive(true);
        Invoke("HideSprite", 3f);
    }
    private void HideSprite()
    {
        datasetSelectedSprite.SetActive(false);
    }
}
