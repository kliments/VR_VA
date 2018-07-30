using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideKMeansControlsMenu : MonoBehaviour {
    public GameObject kMeansMenu, kMeansControls;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(kMeansMenu.activeSelf && !kMeansControls.activeSelf)
        {
            kMeansControls.SetActive(true);
        }
        else if(!kMeansMenu.activeSelf && kMeansControls.activeSelf)
        {
            kMeansControls.SetActive(false);
        }
	}
}
