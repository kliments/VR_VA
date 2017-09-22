using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMeansAlgorithm : MonoBehaviour {

    public Transform scatterplot;
    public GameObject sphere;
    private GameObject dataVisuals;
    public GameObject kMeansFinishedPlane;

    private GameObject[] spheres;

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

    //starting with 3 spheres, number gets increased/decreased if clicking + or -
    public int nrOfSpheres;

    //if true, then the next step will be to change the colors of data points
    private bool changeColors = true;

    //if true, then reposition spheres in the center of the data points
    private bool repositionSpheres = false;
    private bool[] hasArrived;

    //if true, keep repositioning spheres with animation
    private bool allInPlace = true;

    public void StartAlgorithm()
    {
        AssignDataToGameObjects();

        //only generate spheres the first time;
        if (counter == 0)
        {
            SetSizeOfArrays(nrOfSpheres);
            GenerateRandomSpheres();
            spheres = GameObject.FindGameObjectsWithTag("sphere");

            for (int oP = 0; oP < nrOfSpheres; oP++)
            {
                oldPos[oP] = new Vector3(0, 0, 0);
            }

            for(int nP = 0; nP < nrOfSpheres; nP++)
            {
                newPos[nP] = spheres[nP].transform.localPosition;
            }

            for(int nT = 0; nT < nrOfSpheres; nT++)
            {
                newTotal[nT] = 0;
            }
            
        }

        for(int oT = 0; oT < nrOfSpheres; oT++)
        {
            oldTotal[oT] = newTotal[oT];
        }

        counter++;

        //if the next step is to change the colors of the data points related to the closest sphere
        if (changeColors)
        {
            for(int nT= 0; nT < nrOfSpheres; nT++)
            {
                newTotal[nT] = 0;
            }

            for(int dP = 0; dP < nrOfSpheres; dP++)
            {
                if(dataPoints[dP].Count>0)
                {
                    dataPoints[dP].Clear();
                }
            }

            foreach (Transform child in dataVisuals.transform)
            {
                for(int d = 0; d < nrOfSpheres; d++)
                {
                    distance[d] = Vector3.Distance(child.transform.position, spheres[d].transform.position);
                }

                tempD = distance[0];
                smallestDistanceIndex = 0;
                //finding smallest distance
                for(int sD = 0; sD < nrOfSpheres; sD++)
                {
                    if(distance[sD] < tempD)
                    {
                        tempD = distance[sD];
                        smallestDistanceIndex = sD;
                    }
                }

                child.gameObject.GetComponent<Renderer>().material = spheres[smallestDistanceIndex].GetComponent<Renderer>().material;
                dataPoints[smallestDistanceIndex].Add(child.gameObject);
                newTotal[smallestDistanceIndex]++;
                
            }


            changeColors = false;
            repositionSpheres = true;
        }

        //if the next step is to reposition the spheres in the center of the data points
        else if(repositionSpheres)
        {
            //reset coordinates to 0

            SetToZeroValues();

            //calculate center of each color data points
            for(int c = 0; c < nrOfSpheres; c++)
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

                avgCenter[c] = new Vector3(xPos[c], yPos[c], zPos[c]);

                oldPos[c] = spheres[c].transform.localPosition;
            }
            
            //set new positions of all spheres
            allInPlace = false;
            
            repositionSpheres = false;
            changeColors = true;
        }

    }

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
                spheres[i].transform.localPosition = Vector3.Lerp(spheres[i].transform.localPosition, avgCenter[i], Time.deltaTime * 5);

                if (spheres[i].transform.localPosition == avgCenter[i])
                {
                    hasArrived[i] = true;
                }
                else
                {
                    hasArrived[i] = false;
                }

                newPos[i] = spheres[i].transform.localPosition;
            }

            //if all are true, the IF statement won't happen again
            allInPlace = allAreTrue(hasArrived);
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

            if(allAreTrue(numberOfPoints) && allAreTrue(posOfSpheres))
            {
                Debug.Log("K-means clustering is finished in " + counter.ToString() + " steps!");
                kMeansFinishedPlane.SetActive(true);
                kMeansFinishedPlane.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Best clustering found in " + counter.ToString() + " steps!";
                bestClusterFound = true;
            }
        }

	}

    void SetSizeOfArrays(int i)
    {
        dataPoints = new List<GameObject>[i];
        for(int a=0; a<dataPoints.Length; a++)
        {
            dataPoints[a] = new List<GameObject>();
        }
        spheres = new GameObject[i];
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
    }

    void GenerateRandomSpheres()
    {
        float r, g, b;
        Color color;
        for (int i = 0; i < nrOfSpheres; i++)
        {
            GameObject newSphere = Instantiate(sphere, scatterplot);

            spheres[i] = newSphere;
            //for X and Z it has to be between -0.6 and 0.6, cause coordinate 0.0 is in the center but not in the begining of the scatterplot (its shifted)
            Vector3 position = new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(-0.6f, 0.6f));
            newSphere.transform.localPosition = position;
            r = UnityEngine.Random.Range(0.0f, 1.0f);
            g = UnityEngine.Random.Range(0.0f, 1.0f);
            b = UnityEngine.Random.Range(0.0f, 1.0f);
            color = new Color(r, g, b);
            newSphere.GetComponent<MeshRenderer>().material.color = color;
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

    public void resetMe()
    {
        bestClusterFound = false;
        counter = 0;
        kMeansFinishedPlane.SetActive(false);
        for(int s = 0; s < nrOfSpheres; s++)
        {
            if (spheres == null)
            {
                break;
            }
            else
            {
                //if(spheres[s] != null)
                //{
                    Destroy(spheres[s]);
                //}
            }
        }

        changeColors = true;
        repositionSpheres = false;
    }

    public bool allAreTrue(bool[] array)
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
