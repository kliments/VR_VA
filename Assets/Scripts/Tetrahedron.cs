using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrahedron : MonoBehaviour
{

    public TetrahedronMesh tetrahedron;
    float[] data1 = new float[] { 0.1f, 0.1f, 0.1f };
    public TextAsset data;
    TetrahedronMesh[] listObjects;
    public Vector3 Positions;
    public GameObject dummy;
    public GameObject parent;
    private Vector3 dummyPos;


    // Use this for initialization
    void Start()
    {
        tetrahedron = dummy.AddComponent<TetrahedronMesh>();
        dummyPos.x = 0;
        dummyPos.y = 0;
        dummyPos.z = 0;
        tetrahedron.Init(data1, dummyPos);


        string[] lines = data.text.Split('\n');
        listObjects = new TetrahedronMesh[lines.Length];

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
            TetrahedronMesh bar = foo.GetComponent<TetrahedronMesh>();

            Color color = new Color(float.Parse(attributes[4]), float.Parse(attributes[5]), float.Parse(attributes[6]), 1f);

            bar.Init(dataPositions, Positions);
            bar.transform.localScale -= new Vector3(0.991F, 0.991F, 0.991F);
            bar.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            bar.GetComponent<Renderer>().material.color = color;



            listObjects[i] = bar;
        }
        dummy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
