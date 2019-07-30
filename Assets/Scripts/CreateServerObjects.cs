using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CreateServerObjects : NetworkBehaviour
{
    public GameObject datasetChanger, visualizationChanger, kMeansController, dbScanController, denclueController, silhouetteCoefficient, pseudoCodeController, eventSystem;
    // Use this for initialization
    void Start()
    {
        eventSystem = GameObject.Find("EventSystem");
        CmdCreateObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command]
    void CmdCreateObjects()
    {
        if (!isServer || !hasAuthority) return;
        GameObject dataChanger = Instantiate(datasetChanger);
        dataChanger.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(dataChanger);

        GameObject vizChanger = Instantiate(visualizationChanger);
        vizChanger.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(vizChanger);

        GameObject kMeansContr = Instantiate(kMeansController);
        kMeansContr.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(kMeansContr);

        GameObject dbscanContr = Instantiate(dbScanController);
        dbscanContr.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(dbscanContr);

        GameObject denclueContr = Instantiate(denclueController);
        denclueContr.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(denclueContr);

        GameObject silhouetteCoef = Instantiate(silhouetteCoefficient);
        silhouetteCoef.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(silhouetteCoef);

        GameObject pseudoCodeContr = Instantiate(pseudoCodeController);
        pseudoCodeContr.transform.parent = eventSystem.transform;
        NetworkServer.Spawn(pseudoCodeContr);
    }
}
