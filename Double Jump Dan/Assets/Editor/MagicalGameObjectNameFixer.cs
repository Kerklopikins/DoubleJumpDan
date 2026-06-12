using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text.RegularExpressions;

public class MagicalGameObjectNameFixer : EditorWindow
{
    GameObject[] gameObjects;

    [MenuItem("Window/Magical GameObject Name Fixer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MagicalGameObjectNameFixer));
    }

    void Awake()
    {
        gameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
    }
    
    void OnGUI()
    {
        GUILayout.Label("Welcome to the Magical Game Object Name Fixer tool!", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("All this tool does is get rid of the irritating '(number)' at the end of all duplicated enabled game objects in Unity 5x.", MessageType.Info);

        if(GUILayout.Button("Fix Game Object Names!"))
        {
            for(int i = 0; i < gameObjects.Length; i++)
            {
                if(gameObjects[i].name.Contains("("))
                {
                    Undo.RecordObject(gameObjects[i], "Rename Object");
                    gameObjects[i].name = Regex.Replace(gameObjects[i].name, @" \([0-9]+\)", "");
                    EditorUtility.SetDirty(gameObjects[i]);
                    EditorSceneManager.MarkSceneDirty(gameObjects[i].scene);
                }
            }
        }
    }
}