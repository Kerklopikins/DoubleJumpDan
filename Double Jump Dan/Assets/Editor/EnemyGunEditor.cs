using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyGun))]
[CanEditMultipleObjects]
public class EnemyGunEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Animator Parameters:\ntrigger 'Shoot'", MessageType.Info);
    }
}