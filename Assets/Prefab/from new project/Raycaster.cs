using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour {

    float m_MaxDistance;
    float m_Speed;
    float influence;
    bool m_HitDetect;

    Collider m_Collider;
    RaycastHit m_Hit;
    public bool raycast;
    void Start()
    {
        //Choose the distance the Box can reach to
        m_MaxDistance = 5f;
        m_Speed = 20.0f;
        m_Collider = GetComponent<Collider>();
        influence = 0.02f;
        raycast = false;
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (m_HitDetect)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, -transform.up * m_Hit.distance);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, -transform.up * m_MaxDistance);
        }
    }

    public void Raycast()
    {
        Debug.Log("button pressed");
        //Test to see if there is a hit using a BoxCast
        //Calculate using the center of the GameObject's Collider(could also just use the GameObject's position), half the GameObject's size, the direction, the GameObject's rotation, and the maximum distance as variables.
        //Also fetch the hit data   
        //m_HitDetect = Physics.BoxCast(m_Collider.bounds.center, gameObject.transform.lossyScale, -transform.up, out m_Hit, transform.rotation, m_MaxDistance);
        m_HitDetect = Physics.Raycast(transform.position, -transform.up, out m_Hit, 100f);
        if (m_Hit.collider != null)
        {
            //find the exact tile from the map where raycast hits
            Collider[] tile = Physics.OverlapBox(m_Hit.point, new Vector3(0.0001f, 0.0001f, 0.0001f));
            //find the neighbour tiles
            //Collider[] intersecting = Physics.OverlapBox(m_Hit.collider.transform.position, new Vector3(influence, 1f, influence));
            Collider[] intersecting = Physics.OverlapSphere(m_Hit.collider.transform.position, influence);
            if (intersecting.Length > 0)
            {
                for (int i = 0; i < intersecting.Length; i++)
                {
                    if (intersecting[i].tag == "Floor")
                    {
                        Vector3 size = intersecting[i].transform.localScale;
                        size.y += influence*2;
                        intersecting[i].transform.localScale = size;

                        Vector3 pos = intersecting[i].transform.localPosition;
                        pos.y += influence;
                        intersecting[i].transform.localPosition = pos;
                        intersecting[i].GetComponent<MeshRenderer>().material.color = GetComponent<MeshRenderer>().material.color;
                    }
                }
            }
        }
    }
}
