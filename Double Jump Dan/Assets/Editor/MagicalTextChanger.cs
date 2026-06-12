using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public enum _fontStyle { Normal, Bold, Italic, BoldAndItalic }

public class MagicalTextChanger : EditorWindow
{
    public _fontStyle fontStyle;
    private Text[] texts;
    private bool changeFont;
    private Object font;
    private bool changeFontStyle;
    private int fontSize;
    private bool changeFontSize;
    private Color color;
    private bool changeFontColor;
    private bool changeRaycast;
    private bool raycastTarget;

    [MenuItem("Window/Magical Text Changer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MagicalTextChanger));
    }

    void Awake()
    {
        
    }

    void OnGUI()
    {
        texts = FindObjectsByType<Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        GUILayout.Label("Welcome to the Magical Text Changer tool!", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("All this tool does is change properties on UI text components that are not disabled.", MessageType.Info);

        changeFont = EditorGUILayout.BeginToggleGroup("Change Font", changeFont);
            font = EditorGUILayout.ObjectField(font, typeof(Font), true);
        EditorGUILayout.EndToggleGroup();

        changeFontStyle = EditorGUILayout.BeginToggleGroup("Change Font Style", changeFontStyle);
            fontStyle = (_fontStyle)EditorGUILayout.EnumPopup("Font Style", fontStyle);
        EditorGUILayout.EndToggleGroup();

        changeFontSize = EditorGUILayout.BeginToggleGroup("Change Font Size", changeFontSize);
            fontSize = EditorGUILayout.IntField(fontSize);
        EditorGUILayout.EndToggleGroup();

        changeFontColor = EditorGUILayout.BeginToggleGroup("Change Font Color", changeFontColor);
            color = EditorGUILayout.ColorField(color);
        EditorGUILayout.EndToggleGroup();

        changeRaycast = EditorGUILayout.BeginToggleGroup("Change Raycast", changeRaycast);
            raycastTarget = EditorGUILayout.ToggleLeft("Raycast Target", raycastTarget);
        EditorGUILayout.EndToggleGroup();

        if(GUILayout.Button("Change Texts!"))
        {
            for(int i = 0; i < texts.Length; i++)
            {
                if(changeFont)
                    texts[i].font = (Font)font;

                if(changeFontStyle)
                {
                    switch(fontStyle)
                    {
                        case _fontStyle.Normal:
                            texts[i].fontStyle = FontStyle.Normal;
                            break;
                        case _fontStyle.Bold:
                            texts[i].fontStyle = FontStyle.Bold;
                            break;
                        case _fontStyle.Italic:
                            texts[i].fontStyle = FontStyle.Italic;
                            break;
                        case _fontStyle.BoldAndItalic:
                            texts[i].fontStyle = FontStyle.BoldAndItalic;
                            break;
                    }
                }

                if(changeFontSize)
                    texts[i].fontSize = fontSize;

                if(changeFontColor)
                    texts[i].color = color;

                if(changeRaycast)
                    texts[i].raycastTarget = raycastTarget;

                Undo.RecordObject(texts[i], "Text Change");
                EditorUtility.SetDirty(texts[i]);
                EditorSceneManager.MarkSceneDirty(texts[i].gameObject.scene);
            }
        }
    }
}