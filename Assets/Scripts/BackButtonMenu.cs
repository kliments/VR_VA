using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonMenu : MonoBehaviour {
    public List<GameObject> previousMenus;
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device device;
    private SwapBetweenMenus swapMenuScript;
    // Use this for initialization
    void Start ()
    {
        trackedObj = transform.parent.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        previousMenus = new List<GameObject>();
        //Primary Menu Parent
        previousMenus.Add(GameObject.Find("PrimaryMenuParent"));
        swapMenuScript = (SwapBetweenMenus)FindObjectOfType(typeof(SwapBetweenMenus));
    }
	
	// Update is called once per frame
	void Update () {
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            GoBackInMenu();
        }
	}

    public void GoBackInMenu()
    {//remove the last menu from the list and show the second last
        if (previousMenus.Count > 1)
        {
            previousMenus[previousMenus.Count - 1].SetActive(false);
            previousMenus.RemoveAt(previousMenus.Count - 1);
            previousMenus[previousMenus.Count - 1].SetActive(true);
            if (previousMenus.Count == 1)
            {
                swapMenuScript.dontShowControlsMenu = false;
            }
        }
    }
}
