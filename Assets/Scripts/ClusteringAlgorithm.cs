using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClusteringAlgorithm : MonoBehaviour {
    public GameObject label;
    public CurrentAlgorithm current;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public abstract void StartAlgorithm();
    public abstract void ResetMe();

    public void ShowLabel()
    {
        label.SetActive(true);
    }

    public void HideLabel()
    {
        label.SetActive(false);
    }
}
