using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuDatasetsGenerator : MonoBehaviour {
    public GameObject chooserPrefab;
    public List<GameObject> datasetButtons;
    public TextAsset[] datasets;
    public GameObject primaryMenu;

    public Sprite spr;
    private float x, y, z;
    // Use this for initialization
    void Start () {
        spr = new Sprite();
        int s = 1;
        x = -0.24f;
        y = 0;
        z = 0;
        datasetButtons = new List<GameObject>();
        GameObject element;
        for (int i = 0; i < datasets.Length; i++)
        {
            element = Instantiate(chooserPrefab);
            element.GetComponent<datasetChangerScript>().myDataset = datasets[i];
            Vector3 scale = element.transform.localScale;
            Quaternion rot = element.transform.rotation;
            element.transform.parent = gameObject.transform;
            element.transform.localPosition = new Vector3(x, y, z);
            x += 0.024f;
            element.transform.localRotation = rot;
            element.transform.localScale = scale;
            //element.GetComponent<datasetChangerScript>().ChangeText(datasets[i].name);
            datasetButtons.Add(element);
            element.GetComponent<DatasetSpriteChanger>().actualSprite = Resources.Load<Sprite>(s.ToString());
            //add text sprite on buttons
            Sprite spr = Resources.Load<Sprite>("Sprites/ButtonTexts/DatasetTexts/" + s.ToString());
            element.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = spr;
            s++;
            if (i == 0)
            {
                element.GetComponent<GeneralCoverflowProperties>().isHovered = true;
            }

        }

    }
	
	// Update is called once per frame
	void Update () {

	}
    
}
