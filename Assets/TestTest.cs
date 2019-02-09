using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTest : MonoBehaviour {
    public Transform point1, point2, point3, point4;
    public bool calculate;

    public Vector3 v1, v2, v3;
    public float res1 = 0;
    public float res2 = 0;
    // Use this for initialization
    void Start () {
        v1 = new Vector3();
        v2 = new Vector3();
        v3 = new Vector3();
    }
	
	// Update is called once per frame
	void Update () {
		if(calculate)
        {
            calculate = false;
            Calculate();
        }
	}

    void Calculate()
    {
        v1 = point1.position - point2.position;
        v2 = point3.position - point2.position;
        v3 = point4.position - point2.position;
        res1 = v1.x * v2.y - v1.y * v2.x;
        res2 = v1.x * v3.y - v1.y * v3.x;
        
        if (res1 < 0 && res2 < 0) Debug.Log("tilted");
        
    }

}
