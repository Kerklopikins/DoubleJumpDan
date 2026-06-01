using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BasicEnemyAI))]
[CanEditMultipleObjects]
public class BasicEnemyAIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Animator Parameters:\nIf JUMPER: bool 'Grounded'\nIf STOPPER: bool 'Player In Range' & bool 'Stopped'", MessageType.Info);
    }
}