using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianCoefficients : MonoBehaviour {

    public float[][] gaussianCoef;
    public Vector3[][] gaussianPositionMatrix;
    public int matrixRowLength, floorTileCounter;
    private Vector3[][] normalizedPositions;
    public bool valuesCalculated;
    // Use this for initialization
    void Start () {
        valuesCalculated = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GaussianCoefCalculator()
    {
        NormalizedValues();
        int x = 0;
        int y = 0;
        gaussianCoef = new float[matrixRowLength][];
        Vector3 center = new Vector3(0.5f, 0.5f, 0.5f);
        for (int i = 0; i < floorTileCounter; i++)
        {
            if (i % matrixRowLength == 0)
            {
                y = 0;
                gaussianCoef[x] = new float[matrixRowLength];
                x++;
            }
            gaussianCoef[x - 1][y] = Mathf.Exp(-(Mathf.Pow((normalizedPositions[x - 1][y].x - center.x), 2) + Mathf.Pow((normalizedPositions[x - 1][y].z - center.z), 2)) / 2 * (Mathf.Pow(4, 2)));
            y++;
        }
    }
    
    private void NormalizedValues()
    {
        normalizedPositions = new Vector3[matrixRowLength][];
        int x = 0;
        int y = 0;
        float minX = gaussianPositionMatrix[0][0].x;
        float maxX = gaussianPositionMatrix[matrixRowLength - 1][0].x;
        float minZ = gaussianPositionMatrix[0][0].z;
        float maxZ = gaussianPositionMatrix[0][matrixRowLength - 1].z;
        for (int i = 0; i < floorTileCounter; i++)
        {
            if (i % matrixRowLength == 0)
            {
                y = 0;
                normalizedPositions[x] = new Vector3[matrixRowLength];
                x++;
            }
            normalizedPositions[x - 1][y] = NormalizedVector(gaussianPositionMatrix[x - 1][y], minX, maxX, minZ, maxZ);
            y++;
        }
    }

    private Vector3 NormalizedVector(Vector3 vector, float minX, float maxX, float minZ, float maxZ)
    {
        Vector3 norm = new Vector3();
        norm.x = (vector.x - minX) / (maxX - minX);
        norm.z = (vector.z - minZ) / (maxZ - minZ);
        return norm;
    }
}
