using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KMeansAlgorithm : ClusteringAlgorithm
{

    public Transform scatterplot, clusterCompactnessValues;
    public DBScanAlgorithm resetDBScan;
    public DenclueAlgorithm resetDenclue;
    public GameObject sphere;
    public GameObject kMeansFinishedPlane, canvas;
    public List<Color> spheresColor;
    public List<GameObject> spheres;
    public SilhouetteCoefficient silhouetteCoef;
    public List<Vector3> spheresStartPositions;
    //offset for X and Z values, because the 0 coordinates of the local position from each data point is shifted from their parent to x=-0,581 and z=-0,63
    private float xOffset = 0.581f;
    private float zOffset = 0.63f;

    //parent of all data points objects
    private GameObject dataVisuals;

    //average center position
    private Vector3[] avgCenter;

    //for checking if the best cluster is found
    public bool bestClusterFound = false;
    private bool[] numberOfPoints, posOfSpheres;

    //X,Y and Z coordinates for each color
    private float[] xPos, yPos, zPos;

    //array of distances to each sphere
    private float[] distance;
    private float tempD;
    private int smallestDistanceIndex;

    //Red, Green and Blue Materials
    public Material redMat;
    public Material greenMat;
    public Material blueMat;

    //new and old positions of spheres for comparing if k-means has finished
    private Vector3[] newPos, oldPos;

    //new and old total number of colored datapoints for comparing if k-means has finished
    private int[] newTotal, oldTotal;

    private int movedSteps = 0;

    
    //if true, then the next step will be to change the colors of data points
    private bool changeColors = true;

    //if true, then the previous step will be to change the colors of data points
    private bool prevChangeColors = false;

    //if true, then the next step will be to change the position of spheres
    private bool repositionSpheres = false;

    //if true, the previous step will be to change to previous position
    private bool prevReposSpheres = false;

    //if true, then reposition spheres in the center of the data points
    private bool[] hasArrived;

    //if true, keep repositioning spheres with animation
    private bool allInPlace = true;

    //each step positions are saved here in case to go back to previous position
    public List<Vector3[]> stepPositions = new List<Vector3[]>();

    public GameObject ground;

    //Used for reset, in case the Play button was pressed
    public PlayScript play;

    public bool nextStep, prevStep, previousStepClicked;

    public Text[] clusterCompactnessTexts;
    private string tempText;
    private int currentTextCounter;

    //current cluster ID
    private int clusterID = 0;

    //local variable for number of spheres that is updated on client
    //starting with 3 spheres, number gets increased/decreased if clicking + or -

    [SyncVar]
    //Variable that gets updated on server
    public int nrOfSpheres;

    [SyncVar]
    private int counter = 0;

    // Use this for initialization
    void Start()
    {
        FindAllObjects();
    }

    // Update is called once per frame
    void Update()
    {
        //will repeat if all the spheres are still not on the destination avgCenter
        if (!allInPlace)
        {
            allInPlace = true;

            for (int i = 0; i < nrOfSpheres; i++)
            {
                spheres[i].transform.localPosition = Vector3.Lerp(spheres[i].transform.localPosition, avgCenter[i], Time.deltaTime * 5f);

                if (System.Math.Abs(spheres[i].transform.localPosition.x - avgCenter[i].x) < 0.0005f)
                {
                    spheres[i].transform.localPosition = avgCenter[i];
                    hasArrived[i] = true;
                }
                else
                {
                    hasArrived[i] = false;
                }
                newPos[i] = spheres[i].transform.localPosition;
            }
            //if all are true, the IF statement won't happen again
            allInPlace = AllAreTrue(hasArrived);
            if (allInPlace && !bestClusterFound)
            {
                if (currentTextCounter > 3) return;
                if(!previousStepClicked)
                {
                    tempText = clusterCompactnessTexts[currentTextCounter].text;
                    tempText = tempText + "Step " + (counter - 1).ToString() + ": " + ClusteringCompactness().ToString() + System.Environment.NewLine;
                    clusterCompactnessTexts[currentTextCounter].text = tempText;
                }
                if (counter != 0 && (counter - 1) % 20 == 0) currentTextCounter++;
                silhouetteCoef.Calculate();
            }
        }

        //check whether best clustering is found and execute code inside if yes
        if (counter > 0 && !bestClusterFound)
        {
            for (int k = 0; k < nrOfSpheres; k++)
            {
                //compare old position with new position
                if (oldPos[k] == newPos[k])
                {
                    posOfSpheres[k] = true;
                }
                else
                {
                    posOfSpheres[k] = false;
                }

                //compare old number of data points belonging to spheres with new ones
                if (oldTotal[k] == newTotal[k])
                {
                    numberOfPoints[k] = true;
                }
                else
                {
                    numberOfPoints[k] = false;
                }
            }

            if (AllAreTrue(numberOfPoints) && AllAreTrue(posOfSpheres))
            {
                clusterID = 0;
                Debug.Log("K-means clustering is finished in " + counter.ToString() + " steps!");
                kMeansFinishedPlane.SetActive(true);
                kMeansFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Best clustering found in " + counter.ToString() + " steps!";
                bestClusterFound = true;
                foreach (var cluster in clusters)
                {
                    foreach (var point in cluster)
                    {
                        point.GetComponent<ClusterQualityValues>().clusterID = clusterID;
                    }
                    clusterID++;
                }

                //paint pseudo code text red
                pseudoCodeText.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                prevText.color = Color.black;
                prevText = pseudoCodeText.transform.GetChild(4).GetComponent<Text>();
            }
        }
        if (nextStep || Input.GetKeyDown(KeyCode.K))
        {
            nextStep = false;
            CmdStartAlgorithm();
        }
        if (prevStep)
        {
            prevStep = false;
            CmdPreviousStep();
        }
    }

    void FindAllObjects()
    {
        transform.parent = GameObject.Find("EventSystem").transform;
        silhouetteCoef = (SilhouetteCoefficient)FindObjectOfType(typeof(SilhouetteCoefficient));
        scatterplot = GameObject.Find("ScatterplotElements").transform;
        resetDBScan = (DBScanAlgorithm)FindObjectOfType(typeof(DBScanAlgorithm));
        resetDenclue = (DenclueAlgorithm)FindObjectOfType(typeof(DenclueAlgorithm));
        ground = GameObject.Find("Ground");
        play = GetComponent<PlayScript>();
        canvas = GameObject.Find("Canvas");
        clusterCompactnessValues = GameObject.Find("clusteringCompactnessValues").transform;

        foreach (Transform child in scatterplot)
        {
            if(child.name == "kMeansFinishedPlane")
            {
                kMeansFinishedPlane = child.gameObject;
                break;
            }
        }
        foreach(Transform child in canvas.transform)
        {
            if(child.name == "kMeansPseudo")
            {
                pseudoCodeText = child.gameObject;
                break;
            }
        }

        for(int i=1; i<5; i++)
        {
            clusterCompactnessTexts[i - 1] = clusterCompactnessValues.GetChild(i).GetComponent<Text>();
        }
    }

    [Command]
    public void CmdStartAlgorithm()
    {
        if (!isServer) return;
        if (bestClusterFound) return;

        AssignDataToGameObjects();
        previousStepClicked = false;
        //only generate spheres the first time;
        if (counter == 0)
        {
            resetDBScan.GetComponent<DBScanAlgorithm>().ResetMe();
            resetDenclue.GetComponent<DenclueAlgorithm>().ResetMe();
            silhouetteCoef.currentAlgorithm = this;
            pseudoCodeText.SetActive(true);
            SetSizeOfArrays(nrOfSpheres);
            CmdGenerateRandomSpheres();

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();
            //spheres.AddRange(GameObject.FindGameObjectsWithTag("sphere"));
            Vector3[] copy1 = new Vector3[nrOfSpheres];

            for (int oP = 0; oP < nrOfSpheres; oP++)
            {
                oldPos[oP] = new Vector3(0, 0, 0);
            }

            for (int nP = 0; nP < nrOfSpheres; nP++)
            {
                newPos[nP] = spheres[nP].transform.localPosition;
                copy1[nP] = newPos[nP];
            }
            stepPositions.Add(copy1);

            for (int nT = 0; nT < nrOfSpheres; nT++)
            {
                newTotal[nT] = 0;
            }
            counter++;

            foreach (Transform dataPoint in dataVisuals.transform)
            {
                if (dataVisuals.name == "PieChartCtrl")
                {
                    foreach (var mat in dataPoint.gameObject.GetComponent<MeshRenderer>().materials)
                    {
                        mat.color = Color.white;
                    }
                }
                else
                {
                    dataPoint.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                }
                clusteredPoints++;
            }
            foreach (var text in clusterCompactnessTexts)
            {
                text.text = "";
            }
        }

        //if the next step is to change the colors of the data points related to the closest sphere
        else if (changeColors && allInPlace)
        {
            ChangePointsColors();
            changeColors = false;
            prevChangeColors = true;
            repositionSpheres = true;
            prevReposSpheres = false;
            counter++;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();

            //cluster compactness
            if (currentTextCounter > 3) return;
            if (!previousStepClicked)
            {
                tempText = clusterCompactnessTexts[currentTextCounter].text;
                tempText = tempText + "Step " + (counter - 1).ToString() + ": " + ClusteringCompactness().ToString() + System.Environment.NewLine;
                clusterCompactnessTexts[currentTextCounter].text = tempText;
            }
            if (counter != 0 && (counter - 1) % 20 == 0) currentTextCounter++;
        }

        //if the next step is to reposition the spheres in the center of the data points
        else if (repositionSpheres)
        {
            movedSteps++;
            RepositionTheSpheres();
            changeColors = true;
            prevChangeColors = false;
            repositionSpheres = false;
            prevReposSpheres = true;
            allInPlace = false;
            counter++;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();
        }

        RpcStartAlgorithm();
    }

    [ClientRpc]
    void RpcStartAlgorithm()
    {
        if (!hasAuthority) return;
        if (bestClusterFound) return;
        AssignDataToGameObjects();
        previousStepClicked = false;
        //only generate spheres the first time;
        if (counter == 0)
        {
            resetDBScan.GetComponent<DBScanAlgorithm>().ResetMe();
            resetDenclue.GetComponent<DenclueAlgorithm>().ResetMe();
            silhouetteCoef.currentAlgorithm = this;
            pseudoCodeText.SetActive(true);
            SetSizeOfArrays(nrOfSpheres);
            ReconfigureSpheresFromServer();
            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            prevText = pseudoCodeText.transform.GetChild(1).GetComponent<Text>();
            //spheres.AddRange(GameObject.FindGameObjectsWithTag("sphere"));
            Vector3[] copy1 = new Vector3[nrOfSpheres];

            for (int oP = 0; oP < nrOfSpheres; oP++)
            {
                oldPos[oP] = new Vector3(0, 0, 0);
            }

            for (int nP = 0; nP < nrOfSpheres; nP++)
            {
                newPos[nP] = spheres[nP].transform.localPosition;
                copy1[nP] = newPos[nP];
            }
            stepPositions.Add(copy1);

            for (int nT = 0; nT < nrOfSpheres; nT++)
            {
                newTotal[nT] = 0;
            }
            counter++;

            foreach (Transform dataPoint in dataVisuals.transform)
            {
                if (dataVisuals.name == "PieChartCtrl")
                {
                    foreach (var mat in dataPoint.gameObject.GetComponent<MeshRenderer>().materials)
                    {
                        mat.color = Color.white;
                    }
                }
                else
                {
                    dataPoint.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                }
                clusteredPoints++;
            }
            foreach (var text in clusterCompactnessTexts)
            {
                text.text = "";
            }
        }

        //if the next step is to change the colors of the data points related to the closest sphere
        else if (changeColors && allInPlace)
        {
            ChangePointsColors();
            changeColors = false;
            prevChangeColors = true;
            repositionSpheres = true;
            prevReposSpheres = false;
            counter++;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();

            //cluster compactness

            if (!previousStepClicked)
            {
                tempText = clusterCompactnessTexts[currentTextCounter].text;
                tempText = tempText + "Step " + (counter - 1).ToString() + ": " + ClusteringCompactness().ToString() + System.Environment.NewLine;
                clusterCompactnessTexts[currentTextCounter].text = tempText;
            }
            if (counter != 0 && (counter - 1) % 20 == 0) currentTextCounter++;
        }

        //if the next step is to reposition the spheres in the center of the data points
        else if (repositionSpheres)
        {
            movedSteps++;
            RepositionTheSpheres();
            changeColors = true;
            prevChangeColors = false;
            repositionSpheres = false;
            prevReposSpheres = true;
            allInPlace = false;
            counter++;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();
        }
    }
    void ReconfigureSpheresFromServer()
    {
        GameObject[] sphereArray = GameObject.FindGameObjectsWithTag("sphere");
        for (int i = 0; i < sphereArray.Length; i++)
        {
            sphereArray[i].GetComponent<MeshRenderer>().material.color = spheresColor[i];
            spheresStartPositions.Add(sphereArray[i].transform.localPosition);
            sphereArray[i].name = "sphere";
            sphereArray[i].tag = "sphere";
            sphereArray[i].transform.parent = scatterplot;
            spheres.Add(sphereArray[i]);
        }
    }

    [Command]
    public void CmdPreviousStep()
    {
        if (!isServer) return;
        RpcPreviousStep();
    }
    [ClientRpc]
    public void RpcPreviousStep()
    {
        if (counter <= 1 || movedSteps < 0)
        {
            return;
        }
        previousStepClicked = true;
        if (prevChangeColors && allInPlace)
        {
            ChangePointsColors();
            changeColors = true;
            prevChangeColors = false;
            repositionSpheres = false;
            prevReposSpheres = true;
            counter--;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(2).GetComponent<Text>();
        }

        else if (prevReposSpheres)
        {
            stepPositions.RemoveAt(movedSteps);
            movedSteps--;
            RepositionTheSpheres();
            changeColors = false;
            prevChangeColors = true;
            repositionSpheres = true;
            prevReposSpheres = false;
            allInPlace = false;
            counter--;

            //paint pseudo code text red
            pseudoCodeText.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            prevText.color = Color.black;
            prevText = pseudoCodeText.transform.GetChild(3).GetComponent<Text>();
        }

        if (bestClusterFound)
        {
            kMeansFinishedPlane.SetActive(false);
            bestClusterFound = false;
            CmdPreviousStep();
        }
    }

    private void ChangePointsColors()
    {
        for (int oT = 0; oT < nrOfSpheres; oT++)
        {
            oldTotal[oT] = newTotal[oT];
        }

        for (int nT = 0; nT < nrOfSpheres; nT++)
        {
            newTotal[nT] = 0;
        }

        for (int dP = 0; dP < nrOfSpheres; dP++)
        {
            if (clusters[dP].Count > 0)
            {
                clusters[dP].Clear();
            }
        }
        foreach (Transform child in dataVisuals.transform)
        {
            for (int d = 0; d < nrOfSpheres; d++)
            {
                distance[d] = Vector3.Distance(child.transform.position, spheres[d].transform.position);
            }
            tempD = distance[0];
            smallestDistanceIndex = 0;
            //finding smallest distance
            for (int sD = 0; sD < nrOfSpheres; sD++)
            {
                if (distance[sD] < tempD)
                {
                    tempD = distance[sD];
                    smallestDistanceIndex = sD;
                }
            }

            if (changeColors)
            {
                //because PieChart mesh has 3 submeshes
                if (dataVisuals.name == "PieChartCtrl")
                {
                    foreach (var mat in child.gameObject.GetComponent<MeshRenderer>().materials)
                    {
                        mat.SetColor("_Color", spheres[smallestDistanceIndex].GetComponent<Renderer>().material.color);
                    }
                }
                else
                {
                    child.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", spheres[smallestDistanceIndex].GetComponent<Renderer>().material.color);
                }
                child.gameObject.GetComponent<PreviousStepProperties>().colorList.Add(spheres[smallestDistanceIndex].GetComponent<Renderer>().material.color);
                clusters[smallestDistanceIndex].Add(child.gameObject);
                newTotal[smallestDistanceIndex]++;
                child.gameObject.GetComponent<ClusterQualityValues>().clusterID = smallestDistanceIndex;
            }

            else if (prevChangeColors)
            {
                //if it has more than one color
                if (child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count >= 2)
                {
                    child.gameObject.GetComponent<PreviousStepProperties>().colorList.RemoveAt(child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count - 1);
                    if (dataVisuals.name == "PieChartCtrl")
                    {
                        foreach (var mat in child.gameObject.GetComponent<MeshRenderer>().materials)
                        {
                            mat.color = child.gameObject.GetComponent<PreviousStepProperties>().colorList[child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count - 1];
                        }
                    }
                    else
                    {
                        child.gameObject.GetComponent<MeshRenderer>().material.color = child.gameObject.GetComponent<PreviousStepProperties>().colorList[child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count - 1];
                    }
                    for (int pC = 0; pC < nrOfSpheres; pC++)
                    {
                        if (child.gameObject.GetComponent<MeshRenderer>().material.color == spheres[pC].GetComponent<MeshRenderer>().material.color)
                        {
                            child.gameObject.GetComponent<ClusterQualityValues>().clusterID = pC;
                            clusters[pC].Add(child.gameObject);
                            newTotal[pC]++;
                            break;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < newTotal.Length; i++)
        {
            allInPlace = false;
            if (oldTotal[i] != newTotal[i]) allInPlace = true;
        }
        if (!allInPlace)
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                oldPos[i] = spheres[i].transform.localPosition;
            }
        }
        silhouetteCoef.Calculate();
    }

    private void RepositionTheSpheres()
    {
        //reset coordinates to 0
        SetToZeroValues();

        Vector3[] copy2 = new Vector3[nrOfSpheres];
        //calculate center of each color data points
        for (int c = 0; c < nrOfSpheres; c++)
        {
            if (clusters[c].Count > 0)
            {
                for (int r = 0; r < clusters[c].Count; r++)
                {
                    xPos[c] += clusters[c][r].transform.localPosition.x;
                    yPos[c] += clusters[c][r].transform.localPosition.y;
                    zPos[c] += clusters[c][r].transform.localPosition.z;
                }
                xPos[c] = xPos[c] / clusters[c].Count;
                xPos[c] = xPos[c] - xOffset;

                yPos[c] = yPos[c] / clusters[c].Count;

                zPos[c] = zPos[c] / clusters[c].Count;
                zPos[c] = zPos[c] - zOffset;
            }
            else
            {
                xPos[c] = spheres[c].transform.localPosition.x;
                yPos[c] = spheres[c].transform.localPosition.y;
                zPos[c] = spheres[c].transform.localPosition.z;
            }
            if (repositionSpheres)
            {
                avgCenter[c] = new Vector3(xPos[c], yPos[c], zPos[c]);
                copy2[c] = avgCenter[c];
            }
            else if (prevReposSpheres)
            {
                avgCenter[c] = stepPositions[movedSteps][c];
            }
            oldPos[c] = spheres[c].transform.localPosition;
        }
        if (repositionSpheres)
        {
            stepPositions.Add(copy2);
        }
        //set new positions of all spheres
        allInPlace = false;
    }


    void SetSizeOfArrays(int i)
    {
        //clusters = new List<List<GameObject>>(i);
        clusters = new List<List<GameObject>>(new List<GameObject>[i]);
        for (int a = 0; a < clusters.Count; a++)
        {
            clusters[a] = new List<GameObject>();
        }
        spheres = new List<GameObject>();
        spheresStartPositions = new List<Vector3>();
        distance = new float[i];
        newPos = new Vector3[i];
        oldPos = new Vector3[i];
        newTotal = new int[i];
        oldTotal = new int[i];
        xPos = new float[i];
        yPos = new float[i];
        zPos = new float[i];
        avgCenter = new Vector3[i];
        hasArrived = new bool[i];
        numberOfPoints = new bool[i];
        posOfSpheres = new bool[i];
        stepPositions = new List<Vector3[]>();
    }

    [Command]
    void CmdGenerateRandomSpheres()
    {
        if (!isServer) return;

        for (int i = 0; i < nrOfSpheres; i++)
        {
            GameObject newSphere = Instantiate(sphere, scatterplot);
            newSphere.name = "sphere";
            newSphere.tag = "sphere";

            //put on random position if script is called from Master
            //for X and Z it has to be between -0.6 and 0.6, cause coordinate 0.0 is in the center but not in the begining of the scatterplot (its shifted)
            Vector3 position = new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(-0.6f, 0.6f));
            newSphere.transform.localPosition = position;
            spheresStartPositions.Add(position);
            newSphere.GetComponent<MeshRenderer>().material.color = spheresColor[i];
            spheres.Add(newSphere);

            //Spawn the object on server side
            NetworkServer.Spawn(newSphere);
        }
    }

    void AssignDataToGameObjects()
    {
        foreach (Transform child in scatterplot)
        {
            //Finding the current visualization
            if (child.gameObject.name == "DataSpace" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                return;
            }

            else if (child.gameObject.name == "PieChartCtrl" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                return;
            }

            else if (child.gameObject.name == "Triangle" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                return;
            }

            else if (child.gameObject.name == "Tetrahedron" && child.gameObject.activeSelf)
            {
                dataVisuals = child.gameObject;
                return;
            }
        }
    }

    void SetToZeroValues()
    {
        for (int sZ = 0; sZ < nrOfSpheres; sZ++)
        {
            xPos[sZ] = 0;
            yPos[sZ] = 0;
            zPos[sZ] = 0;
        }
    }

    public void ResetMe()
    {
        ground.GetComponent<SetToGround>().rigPosReset = true;
        ground.GetComponent<SetToGround>().RemoveParenthoodFromRig();
        play.StopRoutine();
        movedSteps = 0;
        bestClusterFound = false;
        counter = 0;
        clusterID = 0;
        clusteredPoints = 0;
        kMeansFinishedPlane.SetActive(false);
        for (int s = 0; s < nrOfSpheres; s++)
        {
            if (spheres == null || spheres.Count == 0)
            {
                break;
            }
            else
            {
                GameObject temp = spheres[0];
                spheres.RemoveAt(0);
                Destroy(temp);
            }
        }
        if (dataVisuals != null)
        {
            if (dataVisuals.name == "PieChartCtrl")
            {
                dataVisuals.GetComponent<PieChartMeshController>().ReturnOriginalColors();
            }
            else
            {
                foreach (Transform child in dataVisuals.transform)
                {
                    child.GetComponent<MeshRenderer>().material.color = child.GetComponent<PreviousStepProperties>().originalColor;
                    child.GetComponent<PreviousStepProperties>().colorList = new List<Color>();
                }
            }
        }
        changeColors = true;
        repositionSpheres = false;
        prevChangeColors = false;
        prevReposSpheres = false;

        if (prevText != null) prevText.color = Color.black;
        pseudoCodeText.SetActive(false);

        tempText = "";
        currentTextCounter = 0;
        currentTextCounter = 0;
        spheresStartPositions = new List<Vector3>();
        //NetworkScriptController.commandSender.algorithmStep = 0;
        silhouetteCoef.ResetMe();
    }

    public bool AllAreTrue(bool[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == false)
            {
                return false;
            }
        }
        return true;
    }

    private float ClusteringCompactness()
    {
        float compact = 0;
        int cluster = 0;
        foreach (Transform point in dataVisuals.transform)
        {
            cluster = point.GetComponent<ClusterQualityValues>().clusterID;
            compact += Mathf.Sqrt(Vector3.Distance(point.position, spheres[cluster].transform.position));
        }
        int roundedInt = (int)(compact * 10000);
        return (float)roundedInt / 10000;
    }
}
