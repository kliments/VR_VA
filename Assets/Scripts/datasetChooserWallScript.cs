using UnityEngine;
using System.Collections;

public class datasetChooserWallScript : MonoBehaviour {

    public GameObject chooserPrefab;
    public GameObject chooserContainer;
    public TextAsset[] datasets;

    private int elementsPerRow = 4;

    // Use this for initialization
    void Start () {
        GameObject element;
	    for(int i = 0; i < datasets.Length; i++)
        {
            element = Instantiate(chooserPrefab);
            element.GetComponent<datasetChangerScript>().myDataset = datasets[i];
            Quaternion rot = element.transform.localRotation;
            Vector3 scale = element.transform.localScale;
            element.transform.parent = chooserContainer.transform;
            element.transform.localPosition = new Vector3(0.3f - 0.04f - 0.1f - ((i % elementsPerRow) * 0.18f), 0.5f - 0.16f - (Mathf.Floor(i / elementsPerRow) * 0.16f), 0.5f);
            element.transform.localRotation = rot;
            element.transform.localScale = scale;
            element.GetComponent<datasetChangerScript>().initText();
            element.GetComponent<datasetChangerScript>().spriteChanger(i + 1);
            if (i == 0)
            {
                element.GetComponent<Animator>().SetBool("selected", true);
                element.GetComponent<datasetChangerScript>().isSelected = true;
            }else
            {
                element.GetComponent<Animator>().SetBool("selected", false);
                element.GetComponent<datasetChangerScript>().isSelected = false;
            }
            //element.transform.localRotation = Quaternion.identity;
            //element.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        }
	}
	
	// Update is called once per frame
	void Update () {
        
    }
}
