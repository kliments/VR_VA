using UnityEngine;
using System.Collections;
using System;

public class datasetChangerScript : MonoBehaviour
{

    public TextAsset myDataset;
    public GameObject textPrefab;

    private float lastActivation = 0;
    private GameObject myText;


    public void startTargetedAction()
    {
        if (Time.time - lastActivation > 2)
        {
            lastActivation = Time.time;
            foreach(datasetChangerScript d in FindObjectsOfType<datasetChangerScript>())
            {
                d.GetComponent<Animator>().SetBool("selected", false);
            }
            this.GetComponent<Animator>().SetBool("selected", true);
            //FindObjectOfType<axisMenueScript>().resetMenue();
            //FindObjectOfType<pcLoaderScript>().resetMe();
            FindObjectOfType<DataSpaceHandler>().changeDatafile(myDataset);
            lastActivation = Time.time;
        }
    }

   
    // Use this for initialization
    void Start() {

    }

    public void initText() { 
        myText = Instantiate(textPrefab);
        myText.transform.parent = this.transform;
        myText.transform.localPosition = new Vector3(0f, 0f, -0.0f);
        myText.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        myText.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);

        String value = myDataset.name;
        //value = "bla";
        TextMesh mesher = myText.GetComponent<TextMesh>();
        mesher.text = value;

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
