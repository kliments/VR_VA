using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveMenuDatasetsGenerator : MonoBehaviour {
    public GameObject chooserPrefab;
    public TextAsset[] datasets;

    private float x, y, z;
    private int counter;
    // Use this for initialization
    void Start () {
        x = -0.24f;
        y = 0.1691731f;
        z = 0.0286f;
        counter = 0;
        GameObject element;
        for (int i = 0; i < datasets.Length; i++)
        {
            element = Instantiate(chooserPrefab);
            element.GetComponent<datasetChangerScript>().myDataset = datasets[i];
            Quaternion rot = element.transform.localRotation;
            Vector3 scale = element.transform.localScale;
            element.transform.parent = gameObject.transform;
            element.transform.localPosition = new Vector3(x, y, z);
            x += 0.12f;
            counter++;
            if(counter == 5)
            {
                counter = 0;
                x = -0.24f;
                z -= 0.12f;
            }
            element.transform.localRotation = rot;
            element.transform.localScale = scale;
            element.GetComponent<datasetChangerScript>().ChangeText(datasets[i].name);
            //element.GetComponent<datasetChangerScript>().spriteChanger(i + 1);
            if (i == 0)
            {
                element.GetComponent<Animator>().SetBool("selected", true);
                element.GetComponent<datasetChangerScript>().isSelected = true;
            }
            else
            {
                element.GetComponent<Animator>().SetBool("selected", false);
                element.GetComponent<datasetChangerScript>().isSelected = false;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
