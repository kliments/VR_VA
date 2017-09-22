using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1);
    }
}