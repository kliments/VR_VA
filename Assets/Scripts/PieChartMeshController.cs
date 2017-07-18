using System.Collections.Generic;
using UnityEngine;

public class PieChartMeshController : MonoBehaviour
{
    public PieChartMesh mPieChart;
    float[] mData;
    public TextAsset data;
    public Vector3 Positions;
    public GameObject dummy;
    public GameObject parent;
	private int timer = 0;
	PieChartMesh[] listObjects;
    public Camera myCamera;
    private Vector3 target;

    void Start()
    {
        //prepare data
        string[] lines = data.text.Split('\n');

        mPieChart = dummy.AddComponent<PieChartMesh>() as PieChartMesh;
        float[] x = { 1.0f, 1.0f, 1.0f };
        mPieChart.Init(x, 100, 0, 100, null, Positions);
        mPieChart.Draw(x);

		listObjects = new PieChartMesh[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            //split the lines
            string[] attributes = lines[i].Split(',');
            float[] dataPositions = {
                float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)
                };
            Positions.x = dataPositions[0];
            Positions.y = dataPositions[1];
            Positions.z = dataPositions[2];

            GameObject foo = Instantiate(dummy, Positions, Quaternion.AngleAxis(0, Vector3.up), parent.transform);
            PieChartMesh bar = foo.GetComponent<PieChartMesh>();
            bar.Init(dataPositions, 100, 0, 100, null, Positions);
            bar.Draw(dataPositions);
			bar.transform.localScale -= new Vector3(0.99F, 0.99F, 0.99F);
			//bar.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
			listObjects [i] = bar;
        }
		dummy.SetActive (false);
    }

    void Update()
    {
        for (int i = 0; i < listObjects.Length; i++) 
		{
            target = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y, myCamera.transform.position.z);
            listObjects[i].transform.LookAt(target);
        }

    }

}
