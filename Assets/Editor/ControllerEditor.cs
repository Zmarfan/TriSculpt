using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Controller controller = (Controller)target;
        if (GUILayout.Button("Refresh"))
            controller.Generate();
        DrawDefaultInspector();
    }
}
