using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputRebindingButton))]
public class InputRebindingButtonEditor : Editor
{
    SerializedProperty m_ActionProperty;
    SerializedProperty m_BindingIdProperty;
    SerializedProperty m_ActionLabelProperty;
    SerializedProperty m_BindingTextProperty;
    SerializedProperty m_ControlImageProperty;
    SerializedProperty m_CompositeControlImages;
    SerializedProperty m_RebindTextProperty;
    SerializedProperty rebindCancelTimerTextProperty;
    SerializedProperty m_RebindStartEventProperty;
    SerializedProperty m_RebindStopEventProperty;
    SerializedProperty m_UpdateBindingUIEventProperty;
    SerializedProperty m_DisplayStringOptionsProperty;

    GUIContent m_BindingLabel = new GUIContent("Binding");
    GUIContent m_DisplayOptionsLabel = new GUIContent("Display Options");
    GUIContent m_UILabel = new GUIContent("UI");
    GUIContent m_EventsLabel = new GUIContent("Events");
    GUIContent[] m_BindingOptions;
    string[] m_BindingOptionValues;
    int m_SelectedBindingOption;

    protected void OnEnable()
    {
        m_ActionProperty = serializedObject.FindProperty("m_Action");
        m_BindingIdProperty = serializedObject.FindProperty("m_BindingId");
        m_ActionLabelProperty = serializedObject.FindProperty("m_ActionLabel");
        m_BindingTextProperty = serializedObject.FindProperty("m_BindingText");
        m_ControlImageProperty = serializedObject.FindProperty("m_ControlImage");
        m_CompositeControlImages = serializedObject.FindProperty("m_CompositeControlImages");
        m_RebindTextProperty = serializedObject.FindProperty("m_RebindText");
        rebindCancelTimerTextProperty = serializedObject.FindProperty("rebindCancelTimerText");
        m_UpdateBindingUIEventProperty = serializedObject.FindProperty("m_UpdateBindingUIEvent");
        m_RebindStartEventProperty = serializedObject.FindProperty("m_RebindStartEvent");
        m_RebindStopEventProperty = serializedObject.FindProperty("m_RebindStopEvent");
        m_DisplayStringOptionsProperty = serializedObject.FindProperty("m_DisplayStringOptions");

        RefreshBindingOptions();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        InputRebindingButton script = (InputRebindingButton)target;
        script.buttonClickSound = (AudioClip)EditorGUILayout.ObjectField("Button Click Sound", script.buttonClickSound, typeof(AudioClip), false);
        script.isJoystickRebind = EditorGUILayout.Toggle("Is Joystick Rebind", script.isJoystickRebind);

        // Binding section.
        EditorGUILayout.LabelField(m_BindingLabel, Styles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(m_ActionProperty);

            var newSelectedBinding = EditorGUILayout.Popup(m_BindingLabel, m_SelectedBindingOption, m_BindingOptions);
            if (newSelectedBinding != m_SelectedBindingOption)
            {
                var bindingId = m_BindingOptionValues[newSelectedBinding];
                m_BindingIdProperty.stringValue = bindingId;
                m_SelectedBindingOption = newSelectedBinding;
            }

            var optionsOld = (InputBinding.DisplayStringOptions)m_DisplayStringOptionsProperty.intValue;
            var optionsNew = (InputBinding.DisplayStringOptions)EditorGUILayout.EnumFlagsField(m_DisplayOptionsLabel, optionsOld);
            if (optionsOld != optionsNew)
                m_DisplayStringOptionsProperty.intValue = (int)optionsNew;
        }

        // UI section.
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(m_UILabel, Styles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(m_ActionLabelProperty);
            EditorGUILayout.PropertyField(m_BindingTextProperty);
            EditorGUILayout.PropertyField(m_ControlImageProperty);
            EditorGUILayout.PropertyField(m_CompositeControlImages);
            EditorGUILayout.PropertyField(m_RebindTextProperty);
            EditorGUILayout.PropertyField(rebindCancelTimerTextProperty);
        }

        // Events section.
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(m_EventsLabel, Styles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(m_RebindStartEventProperty);
            EditorGUILayout.PropertyField(m_RebindStopEventProperty);
            EditorGUILayout.PropertyField(m_UpdateBindingUIEventProperty);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            RefreshBindingOptions();
        }

        if(GUI.changed)
            EditorUtility.SetDirty(script);
    }

    protected void RefreshBindingOptions()
    {
        var actionReference = (InputActionReference)m_ActionProperty.objectReferenceValue;
        var action = actionReference?.action;

        if (action == null)
        {
            m_BindingOptions = new GUIContent[0];
            m_BindingOptionValues = new string[0];
            m_SelectedBindingOption = -1;
            return;
        }

        var bindings = action.bindings;
        var bindingCount = bindings.Count;

        m_BindingOptions = new GUIContent[bindingCount];
        m_BindingOptionValues = new string[bindingCount];
        m_SelectedBindingOption = -1;

        var currentBindingId = m_BindingIdProperty.stringValue;
        for (var i = 0; i < bindingCount; ++i)
        {
            var binding = bindings[i];
            var bindingId = binding.id.ToString();
            var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

            // If we don't have a binding groups (control schemes), show the device that if there are, for example,
            // there are two bindings with the display string "A", the user can see that one is for the keyboard
            // and the other for the gamepad.
            var displayOptions =
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
            if (!haveBindingGroups)
                displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

            // Create display string.
            var displayString = action.GetBindingDisplayString(i, displayOptions);
            
            // If binding is part of a composite, include the part name.
            if (binding.isPartOfComposite)
                displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

            // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
            // by instead using a backlash.
            displayString = displayString.Replace('/', '\\');

            // If the binding is part of control schemes, mention them.
            if (haveBindingGroups)
            {
                var asset = action.actionMap?.asset;
                if (asset != null)
                {
                    var controlSchemes = string.Join(", ",
                        binding.groups.Split(InputBinding.Separator)
                            .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                    displayString = $"{displayString} ({controlSchemes})";
                }
            }

            m_BindingOptions[i] = new GUIContent(displayString);
            m_BindingOptionValues[i] = bindingId;

            if (currentBindingId == bindingId)
                m_SelectedBindingOption = i;
        }
    }

    static class Styles
    {
        public static GUIStyle boldLabel = new GUIStyle("MiniBoldLabel");
    }
}