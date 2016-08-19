using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Scatterplot))]
[System.Serializable]
[CanEditMultipleObjects]
public class ScatterplotEditor : Editor
{
    [SerializeField]
    public float minX =0;
    [SerializeField]
    public float maxX =1.0f;

    [SerializeField]
    public float minY = 0;
    [SerializeField]
    public float maxY =1.0f;

    [SerializeField]
    public float minZ = 0.0f;
    [SerializeField]
    public float maxZ =1.0f;


    public override void OnInspectorGUI()
    {
        Scatterplot myTarget = (Scatterplot)target;

 //       if (!init)
 //       {
 //           init = true;
 //           minX = myTarget.dataSpaceHandler.SelectionMinX;
 //           maxX = myTarget.dataSpaceHandler.SelectionMaxX;

//            minY = myTarget.dataSpaceHandler.SelectionMinY;
 //           maxY = myTarget.dataSpaceHandler.SelectionMaxY;
 //
 //           minZ = myTarget.dataSpaceHandler.SelectionMinZ;
 //           maxZ = myTarget.dataSpaceHandler.SelectionMaxZ;

   //     }

        EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);

        myTarget.dataSpaceHandler.data = (TextAsset)EditorGUILayout.ObjectField("Data Set",myTarget.dataSpaceHandler.data,typeof(TextAsset),true);
        myTarget.dataSpaceHandler.dataMappedMaterial = (Material)EditorGUILayout.ObjectField("Material", myTarget.dataSpaceHandler.dataMappedMaterial, typeof(Material), true);

        EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("X");
        EditorGUILayout.MinMaxSlider(ref minX ,ref maxX, 0.0f, 1.0f);
        EditorGUILayout.LabelField("Y");
        EditorGUILayout.MinMaxSlider(ref minY, ref maxY, 0.0f, 1.0f);
        EditorGUILayout.LabelField("Z");
        EditorGUILayout.MinMaxSlider(ref minZ, ref maxZ, 0.0f, 1.0f);

        myTarget.dataSpaceHandler.SelectionMinX = minX;
        myTarget.dataSpaceHandler.SelectionMaxX = maxX;

        myTarget.dataSpaceHandler.SelectionMinY = minY;
        myTarget.dataSpaceHandler.SelectionMaxY = maxY;

        myTarget.dataSpaceHandler.SelectionMinZ = minZ;
        myTarget.dataSpaceHandler.SelectionMaxZ = maxZ;

        myTarget.dataSpaceHandler = (DataSpaceHandler)EditorGUILayout.ObjectField("Handler (you probably don't need to change this", myTarget.dataSpaceHandler, typeof(DataSpaceHandler), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}