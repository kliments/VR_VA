using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBScanPlay : MonoBehaviour {
    public GameObject dbScanButton;
    public bool play;
    private bool allClustersFound;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (play)
        {
            StartRoutine();
            play = false;
        }
    }
    
    void StartRoutine()
    {
        //call the DBSCAN script every 1 seconds
        StartCoroutine("DBScanRepeat");
    }

    public void StopRoutine()
    {
        StopCoroutine("DBScanRepeat");
    }

    IEnumerator DBScanRepeat()
    {
        allClustersFound = dbScanButton.GetComponent<DBScanAlgorithm>().allClustersFound;

        while (!allClustersFound)
        {
            dbScanButton.GetComponent<DBScanAlgorithm>().StartDBSCAN();
            yield return new WaitForSeconds(1f);
        }

    }
}
