using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMeansAlgorithm : MonoBehaviour {

    public Transform scatterplot;
    public GameObject resetDBScan;
    public GameObject sphere;
    private GameObject dataVisuals;
    public GameObject kMeansFinishedPlane;
    public List<Color> spheresColor;
    private List<GameObject> spheres;

    //offset for X and Z values, because the 0 coordinates of the local position from each data point is shifted from their parent to x=-0,581 and z=-0,63
    private float xOffset = 0.581f;
    private float zOffset = 0.63f;

    //array of lists for data points with different collors
    private List<GameObject>[] dataPoints;

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

    private int counter = 0;
    private int movedSteps = 0;

    //starting with 3 spheres, number gets increased/decreased if clicking + or -
    public int nrOfSpheres;

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
    public GameObject play;
    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        //will repeat if all the spheres are still not on the destination avgCenter
        if (!allInPlace)
        {
            allInPlace = true;

            for(int i = 0; i < nrOfSpheres; i++)
            {
                spheres[i].transform.localPosition = Vector3.Lerp(spheres[i].transform.localPosition, avgCenter[i], Time.deltaTime * 5f);
                
                if(System.Math.Abs(spheres[i].transform.localPosition.x - avgCenter[i].x) < 0.0005f)
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
        }

        //check whether best clustering is found and execute code inside if yes
        if (counter > 0 && !bestClusterFound)
        {
            for(int k = 0; k < nrOfSpheres; k++)
            {
                //compare old position with new position
                if(oldPos[k] == newPos[k])
                {
                    posOfSpheres[k] = true;
                }
                else
                {
                    posOfSpheres[k] = false;
                }

                //compare old number of data points belonging to spheres with new ones
                if(oldTotal[k] == newTotal[k])
                {
                    numberOfPoints[k] = true;
                }
                else
                {
                    numberOfPoints[k] = false;
                }
            }

            if(AllAreTrue(numberOfPoints) && AllAreTrue(posOfSpheres))
            {
                Debug.Log("K-means clustering is finished in " + counter.ToString() + " steps!");
                kMeansFinishedPlane.SetActive(true);
                kMeansFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Best clustering found in " + counter.ToString() + " steps!";
                bestClusterFound = true;
            }
        }
	}

    public void StartAlgorithm()
    {
        AssignDataToGameObjects();

        //only generate spheres the first time;
        if (counter == 0)
        {
            resetDBScan.GetComponent<DBScanAlgorithm>().ResetMe();
            SetSizeOfArrays(nrOfSpheres);
            GenerateRandomSpheres();
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

            foreach(Transform dataPoint in dataVisuals.transform)
            {
                dataPoint.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
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
        }
    }

    public void PreviousStep()
    {
        if (counter <= 1 || movedSteps < 0)
        {
            return;
        }
        if (prevChangeColors && allInPlace)
        {
            ChangePointsColors();
            changeColors = true;
            prevChangeColors = false;
            repositionSpheres = false;
            prevReposSpheres = true;
            counter--;
        }

        else if(prevReposSpheres)
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
            if (dataPoints[dP].Count > 0)
            {
                dataPoints[dP].Clear();
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

            if(changeColors)
            {
                child.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", spheres[smallestDistanceIndex].GetComponent<Renderer>().material.color);
                child.gameObject.GetComponent<PreviousStepProperties>().colorList.Add(spheres[smallestDistanceIndex].GetComponent<Renderer>().material.color);
                dataPoints[smallestDistanceIndex].Add(child.gameObject);
                newTotal[smallestDistanceIndex]++;
            }

            else if(prevChangeColors)
            {
                //if it has more than one color
                if (child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count >= 2)
                    {
                        child.gameObject.GetComponent<PreviousStepProperties>().colorList.RemoveAt(child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count - 1);
                        child.gameObject.GetComponent<MeshRenderer>().material.color = child.gameObject.GetComponent<PreviousStepProperties>().colorList[child.gameObject.GetComponent<PreviousStepProperties>().colorList.Count - 1];
                    for (int pC = 0; pC < nrOfSpheres; pC++)
                    {
                        if (child.gameObject.GetComponent<MeshRenderer>().material.color == spheres[pC].GetComponent<MeshRenderer>().material.color)
                            {
                                dataPoints[pC].Add(child.gameObject);
                                newTotal[pC]++;
                                break;
                            }
                        }
                    }
            }
        }
    }
    
    private void RepositionTheSpheres()
    {
        //reset coordinates to 0
        SetToZeroValues();

        Vector3[] copy2 = new Vector3[nrOfSpheres];
        //calculate center of each color data points
        for (int c = 0; c < nrOfSpheres; c++)
        {
            if (dataPoints[c].Count > 0)
            {
                for (int r = 0; r < dataPoints[c].Count; r++)
                {
                    xPos[c] += dataPoints[c][r].transform.localPosition.x;
                    yPos[c] += dataPoints[c][r].transform.localPosition.y;
                    zPos[c] += dataPoints[c][r].transform.localPosition.z;
                }
                xPos[c] = xPos[c] / dataPoints[c].Count;
                xPos[c] = xPos[c] - xOffset;

                yPos[c] = yPos[c] / dataPoints[c].Count;

                zPos[c] = zPos[c] / dataPoints[c].Count;
                zPos[c] = zPos[c] - zOffset;
            }
            else
            {
                xPos[c] = spheres[c].transform.localPosition.x;
                yPos[c] = spheres[c].transform.localPosition.y;
                zPos[c] = spheres[c].transform.localPosition.z;
            }
            if(repositionSpheres)
            {
                avgCenter[c] = new Vector3(xPos[c], yPos[c], zPos[c]);
                copy2[c] = avgCenter[c];
            }
            else if(prevReposSpheres)
            {
                avgCenter[c] = stepPositions[movedSteps][c];
            }
            oldPos[c] = spheres[c].transform.localPosition;
        }
        if(repositionSpheres)
        {
            stepPositions.Add(copy2);
        }
        //set new positions of all spheres
        allInPlace = false;
    }
    

    void SetSizeOfArrays(int i)
    {
        dataPoints = new List<GameObject>[i];
        for(int a=0; a<dataPoints.Length; a++)
        {
            dataPoints[a] = new List<GameObject>();
        }
        spheres = new List<GameObject>();
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

    void GenerateRandomSpheres()
    {
        for (int i = 0; i < nrOfSpheres; i++)
        {
            GameObject newSphere = Instantiate(sphere, scatterplot);

            spheres.Add(newSphere);
            //for X and Z it has to be between -0.6 and 0.6, cause coordinate 0.0 is in the center but not in the begining of the scatterplot (its shifted)
            Vector3 position = new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(-0.6f, 0.6f));
            newSphere.transform.localPosition = position;
            newSphere.GetComponent<MeshRenderer>().material.color = spheresColor[i];
            newSphere.name = "sphere";
            newSphere.tag = "sphere";
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
        for(int sZ = 0; sZ < nrOfSpheres; sZ++)
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
        play.GetComponent<PlayScript>().StopRoutine();
        movedSteps = 0;
        bestClusterFound = false;
        counter = 0;
        kMeansFinishedPlane.SetActive(false);
        for(int s = 0; s < nrOfSpheres; s++)
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
        if(dataVisuals != null)
        {
            foreach (Transform child in dataVisuals.transform)
            {
                child.GetComponent<MeshRenderer>().material.color = child.GetComponent<PreviousStepProperties>().originalColor;
                child.GetComponent<PreviousStepProperties>().colorList = new List<Color>();
            }
        }
        changeColors = true;
        repositionSpheres = false;
        prevChangeColors = false;
        prevReposSpheres = false;
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
}
