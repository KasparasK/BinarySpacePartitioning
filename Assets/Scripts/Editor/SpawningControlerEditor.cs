using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SpawningControler))]
public class SpawningControlerEditor : Editor
{
    private SpawningControler spawningControler;

    public override void OnInspectorGUI()
    {
        spawningControler = (SpawningControler)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Spawn"))
        {
            spawningControler.Spawn();
        }
    }
}
