using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem.Controls;
////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes
public class InputRebindingButton : MonoBehaviour
{
    public bool isJoystickRebind;
    public AudioClip buttonClickSound;
    [SerializeField] InputActionReference m_Action;
    [SerializeField] string m_BindingId;
    [SerializeField] InputBinding.DisplayStringOptions m_DisplayStringOptions;
    [SerializeField] Text m_ActionLabel;
    [SerializeField] Text m_BindingText;
    public Image m_ControlImage;
    public List<Image> m_CompositeControlImages = new List<Image>();
    [SerializeField] Text m_RebindText;
    [SerializeField] Text rebindCancelTimerText;
    [SerializeField] UpdateBindingUIEvent m_UpdateBindingUIEvent;
    [SerializeField] InteractiveRebindEvent m_RebindStartEvent;
    [SerializeField] InteractiveRebindEvent m_RebindStopEvent;
    [SerializeField] InputActionRebindingExtensions.RebindingOperation m_RebindOperation;
    [SerializeField] static List<InputRebindingButton> s_RebindActionUIs;

    GameInputManager gameInputManager;
    public int publicBindingIndex { get; private set; }
    bool rebinding;
    bool displayingRebindText;
    float rebindCancelTimer;
    string deviceLayoutName;
    string cancelRebindButtonName;

    List<string> GetCompositePartNames(InputAction action, int compositeIndex)
    {
        List<string> partNames = new List<string>();

        for(int i = compositeIndex + 1; i < action.bindings.Count; i++)
        {
            if(!action.bindings[i].isPartOfComposite)
                break;
            
            partNames.Add(GetControlDisplayName(action.bindings[i].effectivePath, deviceLayoutName));
        }

        return partNames;
    }

    protected void OnEnable()
    {
        UpdateBindingDisplay();

        if(s_RebindActionUIs == null)
            s_RebindActionUIs = new List<InputRebindingButton>();
        
        s_RebindActionUIs.Add(this);
        
        if(s_RebindActionUIs.Count == 1)
            InputSystem.onActionChange += OnActionChange;
    }

    protected void OnDisable()
    {
        m_RebindOperation?.Dispose();
        m_RebindOperation = null;

        s_RebindActionUIs.Remove(this);

        if(s_RebindActionUIs.Count == 0)
        {
            s_RebindActionUIs = null;
            InputSystem.onActionChange -= OnActionChange;
        }
    }

    void Start()
    {
        gameInputManager = GameInputManager.Instance;    
    }

    void Update()
    {
        if(!displayingRebindText)
            return;

        rebindCancelTimer -= Time.unscaledDeltaTime;
        
        if(rebindCancelTimer > 0)
        {
            if(deviceLayoutName != null)
            {
                if(deviceLayoutName.Contains("DualShock") || deviceLayoutName.Contains("DualSense"))
                    cancelRebindButtonName = "Touchpad";
                else if(deviceLayoutName.Contains("XInput") || deviceLayoutName.Contains("Xbox") || deviceLayoutName.Contains("Gamepad"))
                    cancelRebindButtonName = "View";
                else
                    cancelRebindButtonName = "Escape";                
            }
            else
            {
                cancelRebindButtonName = "Escape";
            }

            rebindCancelTimerText.text = "Press " + cancelRebindButtonName + " to cancel - Resets in " + Mathf.CeilToInt(rebindCancelTimer) + " seconds";        
        }
        else
        {
            displayingRebindText = false;
            m_RebindOperation?.Cancel();
        }
    }

    #if UNITY_EDITOR
    protected void OnValidate()
    {
        UpdateActionLabel();
        UpdateBindingDisplay();
    }
    
    #endif

    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
    }

    public string bindingId
    {
        get => m_BindingId;
        set
        {
            m_BindingId = value;
            UpdateBindingDisplay();
        }
    }

    public InputBinding.DisplayStringOptions displayStringOptions
    {
        get => m_DisplayStringOptions;
        set
        {
            m_DisplayStringOptions = value;
            UpdateBindingDisplay();
        }
    }

    public Text actionLabel
    {
        get => m_ActionLabel;
        set
        {
            m_ActionLabel = value;
            UpdateActionLabel();
        }
    }

    public Text bindingText
    {
        get => m_BindingText;
        set
        {
            m_BindingText = value;
            UpdateBindingDisplay();
        }
    }

    public Text rebindPrompt
    {
        get => m_RebindText;
        set => m_RebindText = value;
    }

    public UpdateBindingUIEvent updateBindingUIEvent
    {
        get
        {
            if(m_UpdateBindingUIEvent == null)
                m_UpdateBindingUIEvent = new UpdateBindingUIEvent();

            return m_UpdateBindingUIEvent;
        }
    }

    public InteractiveRebindEvent startRebindEvent
    {
        get
        {
            if(m_RebindStartEvent == null)
                m_RebindStartEvent = new InteractiveRebindEvent();

            return m_RebindStartEvent;
        }
    }

    public InteractiveRebindEvent stopRebindEvent
    {
        get
        {
            if(m_RebindStopEvent == null)
                m_RebindStopEvent = new InteractiveRebindEvent();

            return m_RebindStopEvent;
        }
    }

    public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;

        action = m_Action?.action;

        if(action == null)
            return false;

        if(string.IsNullOrEmpty(m_BindingId))
            return false;

        // Look up binding index.
        Guid bindingId = new Guid(m_BindingId);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);

        if(bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }

    public void UpdateBindingDisplay()
    {
        string displayString = string.Empty;
        string deviceLayoutName = default(string);
        string controlPath = default(string);
        string partEffectivePath = default(string);
        int angleOpen;
        int angleClose;

        InputAction action = m_Action?.action;

        if(action != null)
        {
            int bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            publicBindingIndex = bindingIndex;

            if(bindingIndex != -1)
            {
                if(action.bindings[bindingIndex].isComposite)
                {
                    List<string> parts = new List<string>();

                    for(int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
                    {
                        partEffectivePath = action.bindings[i].effectivePath;

                        if(string.IsNullOrEmpty(partEffectivePath))
                            continue;

                        angleOpen = partEffectivePath.IndexOf('<');
                        angleClose = partEffectivePath.IndexOf('>');

                        deviceLayoutName = (angleOpen > 0 && angleClose > angleOpen) ? partEffectivePath.Substring(angleOpen + 1, angleClose - angleOpen - 1) : "";
                        controlPath = partEffectivePath.Substring(partEffectivePath.LastIndexOf('/') + 1);

                        Dictionary<string, string> overrideDictionary = deviceLayoutName switch
                        {
                            "DualShock" => GameInputManager.PSDisplayNameOverrides, 
                            "DualSense" => GameInputManager.PSDisplayNameOverrides, 
                            "XInput" => GameInputManager.XboxDisplayNameOverrides, 
                            "Xbox" => GameInputManager.XboxDisplayNameOverrides,
                            "Gamepad" => GameInputManager.XboxDisplayNameOverrides,
                            "Keyboard" => GameInputManager.DisplayNameOverrides,
                            "Mouse" => GameInputManager.DisplayNameOverrides, 
                            _ => GameInputManager.DisplayNameOverrides
                        };

                        if(overrideDictionary.TryGetValue(controlPath, out string overrideName))
                        {
                            parts.Add(overrideName);
                        }
                        else
                        {
                            string partDisplay = action.GetBindingDisplayString(i);
                            parts.Add(partDisplay);
                        }
                    }
                    
                    if(parts.Count == 4)
                        Swap(parts, 1, 2);

                    displayString = string.Join("/", parts);
                    controlPath = action.bindings[bindingIndex].path;
                }
                else
                {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
                    displayString = GetControlDisplayName(controlPath, deviceLayoutName);

                    if(controlPath != null && GameInputManager.DisplayNameOverrides.TryGetValue(controlPath, out string overrideNameTwo))
                        displayString = overrideNameTwo;
                }
            }
        }

        if(m_BindingText != null)
            m_BindingText.text = displayString;
        
        m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }
    
    string GetControlDisplayName(string controlPath, string deviceLayoutName = null)
    {
        if(string.IsNullOrEmpty(controlPath))
            return "Unknown";
        
        string controlName = controlPath.Contains("/") ? controlPath.Substring(controlPath.LastIndexOf('/') + 1) : controlPath;

        if(deviceLayoutName != null)
        {
            if(deviceLayoutName.Contains("DualShock") || deviceLayoutName.Contains("DualSense"))
            {
                if(GameInputManager.PSDisplayNameOverrides.TryGetValue(controlName, out string psName))
                    return psName;
            }
            else if(deviceLayoutName.Contains("XInput") || deviceLayoutName.Contains("Xbox") || deviceLayoutName.Contains("Gamepad"))
            {
                if(GameInputManager.XboxDisplayNameOverrides.TryGetValue(controlName, out string xboxName))
                    return xboxName;
            }
        }

        if(GameInputManager.DisplayNameOverrides.TryGetValue(controlName, out string customName))
            return customName;
        
        controlName = char.ToUpper(controlName[0]) + controlName.Substring(1);
        controlName = Regex.Replace(controlName, "(\\B[A-Z])", " $1");

        return controlName;
    }

    public void ResetToDefault()
    {
        if(!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
            return;

        if(action.bindings[bindingIndex].isComposite)
        {
            // It's a composite. Remove overrides from part bindings.
            for(int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                action.RemoveBindingOverride(i);
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        
        UpdateBindingDisplay();
    }
    
    #region TODO
    //LIGHTNING GUN
    //MAKE SPAWRK PARTICLE SHAPE HEMISPHEERE
    
    ////THIS SCRIPT
    ///FIX NULL deviceLayoutName. store it probably
    
    ///ADD AN EVENT WHEN THE PLAYER'S DEAD BOOL IS ACTUALLY TURNED OFF, VS JUST TRACKING IT EVERY FRAME
    /// 
    //SPLASH SCREEN
    //TEST IF VSYNC IS WORKING IN SLPASH SCREEN
    ///DELAY ANIMATION BY LIKE A QUARTER SECOND

    //GET RID OF UNSUED PACKAGES like post processing, cinemachine, ect.
    //FIX WHY THE MOUSE IS RIGHT ABOVE THE PLAYER WHEN RESTARTING AND NOT MOVING
    //probably because the mouse inputs are disabled when the level first starts

    //CAMERA add a look bellow when falling and maybe when going up as well
    //fine tune camera follow better

    //PLayer
    // stop gun from flashing when dieing and fix gun not falling far enough,
    //it stops falling way too soon
    //MAYBE MAKE AN IDLE ANIMATION
    //FIX SLIGHT ARM ROTATION LAG WHEN FLIPPING WHEN LOCK AIMING IS ON
    
    //Moving Platform
    ///MAKE THE MOVING PLATFORM ADD VELOCITY TO PASSANGERS AND OBJECTS VS PARENTING THEM, 
    /// ITS CAUSING A TON OF ISSUES, like with shells or when the game exits and player is
    /// still on it

    ///Make the checkpoints and finish level back to box colliders maybe
    
    ///####AUTO SCREEN RESOLUTION CHANGING####
    //Make screen auto update resolutions about every second for different moniters
    ///Make settings sliders reset when entering settings if theyre modified
    /// Make sure level loading manager black background resizes when screen resolution changed

    ////####SPRINTING####
    /// Maybe make player walk particles emmision higher when sprinting
    
    ////####INPUT AND VIRTUAL CURSOR####
    /// CHEK IF PREVIOUS OVERRIDE NAME IS WORKING RIGHT
    /// //OPTIMIZE INPUT SETTINGS BY DISABLING BUTTONS EHRN OUT OF VIEW
    /// MAKE KEYBOARD MODE SAVE PERMINATLY
    ////ADD CURSOR WARPING TO KEYBOARD INPUT CHANGE ASWELL LIKE IN ONCONTROLLERCHANGE
    ///PUT TARGET VECTOR2 IN TOP VARIABLES AREA VS IN UPDATE
    //ADD SWAP JOYSTICK CURSOR
    ////ADD CURSOR ACCELERATION AS SLIDER
    /// ADD ADJUSTABLE CURSOR BINDING Buttons like click and movement
    
    ///####EXIT UI AREA####
    ////FIX CLICKING BUTTON AND EXITING UI AREA AT SAME TIME, GET PERMISSION FROM UISCREENMANAGER
    /// FIX EXIT UI EXITING WHILE POINTER IS DOWN ON BUTTON
    
    ///####DATA SAVING 
    ///FIGURE OUT WHY SAVING IS SO PAINFULLY SLOW NOW
///SAVE LESS OFTEN NOW, Probably save when loading a level

    #endregion

    public void StartInteractiveRebind()
    {
        if(!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
            return;

        // If the binding is a composite, we need to rebind each part in turn.
        if(action.bindings[bindingIndex].isComposite)
        {
            int firstPartIndex = bindingIndex + 1;
            
            if(firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        }
        else
        {
            PerformInteractiveRebind(action, bindingIndex);
        }
    }

    void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        m_RebindOperation?.Cancel();
        
        string previousPath = !string.IsNullOrEmpty(action.bindings[bindingIndex].overridePath)
        ? action.bindings[bindingIndex].overridePath
        : action.bindings[bindingIndex].path;

        void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
        }

        foreach(InputActionMap map in action.actionMap.asset.actionMaps)
            map.Disable();

        // Determine if this binding targets a gamepad/controller scheme
        InputBinding binding = action.bindings[bindingIndex];
        bool isGamepadBinding = binding.groups.Contains("Controller", StringComparison.OrdinalIgnoreCase);
        bool isKeyOrMouseButtonAllowed = false;
        bool isControllerButtonNotAllowed = false;
        bool isUIBinding = binding.groups.Contains("UI", StringComparison.OrdinalIgnoreCase);

        if(isGamepadBinding)
        {
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnPotentialMatch(operation =>
            {
                ButtonControl controller = operation.selectedControl as ButtonControl;
                
                if(controller == null)
                {
                    operation.Reset();
                    return;
                }

                if(controller != null)
                {
                    if(!isUIBinding)
                    {
                        isControllerButtonNotAllowed = operation.selectedControl == 
                        Gamepad.current.startButton || operation.selectedControl == 
                        Gamepad.current.dpad || operation.selectedControl == 
                        Gamepad.current.dpad.up || operation.selectedControl == 
                        Gamepad.current.dpad.down || operation.selectedControl == 
                        Gamepad.current.dpad.left || operation.selectedControl == 
                        Gamepad.current.dpad.right;
                    }
                    else
                    {
                        isControllerButtonNotAllowed = operation.selectedControl == 
                        Gamepad.current.startButton || operation.selectedControl == 
                        Gamepad.current.selectButton || operation.selectedControl ==
                        Gamepad.current.dpad || operation.selectedControl == 
                        Gamepad.current.dpad.up || operation.selectedControl == 
                        Gamepad.current.dpad.down || operation.selectedControl == 
                        Gamepad.current.dpad.left || operation.selectedControl == 
                        Gamepad.current.dpad.right || operation.selectedControl == 
                        Gamepad.current.buttonEast || operation.selectedControl == 
                        Gamepad.current.leftShoulder || operation.selectedControl == 
                        Gamepad.current.rightShoulder;
                    }
                }
            })
            .WithControlsExcluding("<Keyboard>")
            .WithCancelingThrough("<Gamepad>/select");
        }
        else
        {
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnPotentialMatch(operation =>
            {
                KeyControl key = operation.selectedControl as KeyControl;
                ButtonControl mouse = operation.selectedControl as ButtonControl;
                
                if(key == null && mouse == null)
                {
                    operation.Reset();
                    return;
                }

                if(key != null)
                {
                    Key k = key.keyCode;

                    if(!isUIBinding)
                    {
                        isKeyOrMouseButtonAllowed = (k >= Key.A && k <= Key.Z) ||
                        (k >= Key.Digit1 && k <= Key.Digit0) ||
                        (k >= Key.Numpad0 && k <= Key.Numpad9) ||
                        k == Key.LeftShift || k == Key.RightShift ||
                        k == Key.LeftCtrl || k == Key.RightCtrl ||
                        k == Key.LeftCommand || k == Key.RightCommand ||
                        k == Key.LeftAlt || k == Key.RightAlt ||
                        k == Key.UpArrow || k == Key.DownArrow || k == Key.LeftArrow || k == Key.RightArrow ||
                        k == Key.Enter || k == Key.NumpadEnter ||
                        k == Key.Backspace || k == Key.Space || 
                        k == Key.Tab || k == Key.Slash || k == Key.Period || k == Key.Comma ||
                        k == Key.Quote || k == Key.Semicolon || k == Key.LeftBracket || k == Key.RightBracket ||
                        k == Key.Backslash;
                    }
                    else
                    {
                        isKeyOrMouseButtonAllowed = (k >= Key.A && k <= Key.Z) ||
                        (k >= Key.Digit1 && k <= Key.Digit0) ||
                        (k >= Key.Numpad0 && k <= Key.Numpad9) ||
                        (k >= Key.F1 && k <= Key.F12) ||
                        k == Key.LeftShift || k == Key.RightShift ||
                        k == Key.LeftCtrl || k == Key.RightCtrl ||
                        k == Key.LeftCommand || k == Key.RightCommand ||
                        k == Key.LeftAlt || k == Key.RightAlt ||
                        k == Key.UpArrow || k == Key.DownArrow || k == Key.LeftArrow || k == Key.RightArrow ||
                        k == Key.Backspace || k == Key.Space || 
                        k == Key.Tab || k == Key.Slash || k == Key.Period || k == Key.Comma ||
                        k == Key.Quote || k == Key.Semicolon || k == Key.LeftBracket || k == Key.RightBracket ||
                        k == Key.Backslash;
                    }
                }
                else if(mouse != null)
                {
                    isKeyOrMouseButtonAllowed = operation.selectedControl == Mouse.current.leftButton ||
                    operation.selectedControl == Mouse.current.rightButton ||
                    operation.selectedControl == Mouse.current.middleButton;
                }
                
                //Add button not allowed for gamepad
            })
            .WithControlsExcluding("<Gamepad>")
            .WithCancelingThrough("<Keyboard>/escape");
        }

        m_RebindOperation.OnCancel(operation =>
        {
            AudioManager.Instance.PlaySound2D(buttonClickSound);
            
            action.ApplyBindingOverride(bindingIndex, previousPath);

            StartCoroutine(DelayFinishInputRebinding(1.5f));
            rebindCancelTimerText.text = "";
            m_RebindText.text = "Input remapping cancelled";

            foreach(InputActionMap map in action.actionMap.asset.actionMaps)
                map.Enable();
            
            m_RebindStopEvent?.Invoke(this, operation);
            UpdateBindingDisplay();
            CleanUp();
        })
        .OnComplete(operation =>
        {
            AudioManager.Instance.PlaySound2D(buttonClickSound);
            
            if(isGamepadBinding)
            {
                if(isControllerButtonNotAllowed)
                {
                    m_RebindText.text = "Button not allowed";
                    action.ApplyBindingOverride(bindingIndex, previousPath);

                    StartCoroutine(RestartBindingAfterDelay(action, bindingIndex, allCompositeParts));
                    return;
                }
            }
            else
            {
                if(!isKeyOrMouseButtonAllowed)
                {
                    m_RebindText.text = "Key not allowed";
                    action.ApplyBindingOverride(bindingIndex, previousPath);

                    StartCoroutine(RestartBindingAfterDelay(action, bindingIndex, allCompositeParts));
                    return;
                }
            }

            InputControl newPath = operation.selectedControl;
            bool isDuplicate = false;
            string duplicateActionName = "";
            
            int ownerCompositeIndex = -1;

            if(action.bindings[bindingIndex].isPartOfComposite)
            {
                for(int i = bindingIndex - 1; 1 > 0; i--)
                {
                    if(action.bindings[i].isComposite)
                    {
                        ownerCompositeIndex = i;
                        break;
                    }
                }
            }

            foreach(InputAction mapAction in action.actionMap.actions)
            {
                foreach(InputBinding binding in mapAction.bindings)
                {
                    if(binding.id.ToString() == m_BindingId)
                        continue;

                    if(binding.groups != action.bindings[bindingIndex].groups)
                        continue;

                    int otherIndex = mapAction.bindings.IndexOf(b => b.id == binding.id);

                    if(mapAction == action && otherIndex == bindingIndex)
                        continue;

                    if(InputControlPath.Matches(binding.effectivePath, newPath))
                    {
                        duplicateActionName = mapAction.name;
                        isDuplicate = true;
                        break;
                    }
                }

                if(isDuplicate)
                    break;
            }
                        
            if(isDuplicate)
            {
                string displayName = "";

                if(GetControlDisplayName(newPath.path).Contains("\n"))
                    displayName = GetControlDisplayName(newPath.path, deviceLayoutName).Replace("\n", " ");
                else
                    displayName = GetControlDisplayName(newPath.path, deviceLayoutName);

                m_RebindText.text = displayName + " is already used by " + duplicateActionName;
                
                action.ApplyBindingOverride(bindingIndex, previousPath);

                StartCoroutine(RestartBindingAfterDelay(action, bindingIndex, allCompositeParts));
                return;
            }
            
            foreach(InputActionMap map in action.actionMap.asset.actionMaps)
                map.Enable();

            m_RebindStopEvent?.Invoke(this, operation);
            UpdateBindingDisplay();
            CleanUp();

            if(allCompositeParts)
            {
                int nextBindingIndex = bindingIndex + 1;
                string compositeDisplay = "";

                if(nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                {
                    PerformInteractiveRebind(action, nextBindingIndex, true);
                }
                else
                {
                    int compositeIndex = -1;
                    
                    for(int i = bindingIndex; i > 0; i--)
                    {
                        if(action.bindings[i].isComposite)
                        {
                            compositeIndex = i;
                            break;
                        }
                    }

                    if(compositeIndex != -1)
                    {
                        List<string> partPaths = GetCompositePartNames(action, compositeIndex);

                        if(partPaths.Count == 4)
                            Swap(partPaths, 1, 2);

                        compositeDisplay = string.Join("/", partPaths) + " assigned to " + action.name;
                    }
                    else
                    {
                        compositeDisplay = "Input Remapping Complete";
                    }

                    m_RebindText.text = compositeDisplay;
                    StartCoroutine(DelayFinishInputRebinding(1.5f));
                }
            }
            else
            {
                m_RebindText.text = GetControlDisplayName(newPath.path, deviceLayoutName) + " assigned to " + action.name;
                StartCoroutine(DelayFinishInputRebinding(1.5f));
            }
        });

        if(isGamepadBinding)
        {
            if(isJoystickRebind)
            {
                // Joystick rebind: only accept stick/axis input, exclude buttons and triggers
                m_RebindOperation
                    .WithExpectedControlType("Stick")
                    .WithControlsExcluding("<Gamepad>/leftTrigger")
                    .WithControlsExcluding("<Gamepad>/rightTrigger")
                    .WithControlsExcluding("<Gamepad>/buttonNorth")
                    .WithControlsExcluding("<Gamepad>/buttonSouth")
                    .WithControlsExcluding("<Gamepad>/buttonEast")
                    .WithControlsExcluding("<Gamepad>/buttonWest")
                    .WithControlsExcluding("<Gamepad>/leftShoulder")
                    .WithControlsExcluding("<Gamepad>/rightShoulder")
                    .WithControlsExcluding("<Gamepad>/start")
                    .WithControlsExcluding("<Gamepad>/select")
                    .WithControlsExcluding("<Gamepad>/leftStickButton")
                    .WithControlsExcluding("<Gamepad>/rightStickButton")
                    .WithControlsExcluding("<DualShockGamepad>/touchpadButton")
                    .WithControlsExcluding("<Keyboard>")
                    .WithControlsExcluding("<Mouse>");
            }
            else
            {
                // Button/trigger rebind: exclude joystick axes, keyboard, and mouse
                m_RebindOperation
                    .WithControlsExcluding("<Gamepad>/leftStick")
                    .WithControlsExcluding("<Gamepad>/rightStick")
                    .WithControlsExcluding("<DualShockGamepad>/touchpadButton")
                    .WithControlsExcluding("<Keyboard>")
                    .WithControlsExcluding("<Mouse>");
            }
        }
        else
        {
            // Keyboard rebind: exclude all gamepad input
            m_RebindOperation
                .WithControlsExcluding("<Gamepad>")
                .WithControlsExcluding("<Keyboard>/leftWindows")
                .WithControlsExcluding("<Keyboard>/rightWindows")
                .WithControlsExcluding("<Keyboard>/leftMeta")
                .WithControlsExcluding("<Keyboard>/rightMeta")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithControlsExcluding("<Keyboard>/delete")
                .WithControlsExcluding("<Mouse>/scroll");
        }        

        string partName = default(string);

        if(action.bindings[bindingIndex].isPartOfComposite)
            partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

        if(m_RebindText != null)
        {
            string waitingMessage;

            if(action.bindings[bindingIndex].isPartOfComposite)
            {
                string partsDirection = action.bindings[bindingIndex].name;
                waitingMessage = $"Press {char.ToUpper(partsDirection[0]) + partsDirection.Substring(1)}";
            }
            else if(isGamepadBinding)
                waitingMessage = isJoystickRebind ? "Move any joystick for " + action.name : "Press any button for " + action.name;
            else
                waitingMessage = "Press any key or mouse button for " + action.name;
            
            m_RebindText.text = $"{waitingMessage}";
        }
        
        if(m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
            m_BindingText.text = "<Waiting...>";

        m_RebindStartEvent?.Invoke(this, m_RebindOperation);
        
        if(!rebinding)
        {
            rebindCancelTimer = 15;
            StartCoroutine(DelayStartInputRebinding(m_RebindOperation));
        }
        else
        {
            m_RebindOperation.Start();
            rebindCancelTimer = 15;
        }
    }

    void Swap<T>(List<T> list, int i, int j)
    {
        T temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    IEnumerator RestartBindingAfterDelay(InputAction action, int bindingIndex, bool allCompositeParts)
    {
        displayingRebindText = false;
        rebindCancelTimerText.text = "";

        yield return new WaitForSecondsRealtime(1.5f);
        PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
        displayingRebindText = true;
    }
    IEnumerator DelayStartInputRebinding(InputActionRebindingExtensions.RebindingOperation operation)
    {
        displayingRebindText = true;
        gameInputManager.Rebind(true);
        yield return new WaitForSecondsRealtime(0.275f);
        operation.Start();
        rebinding = true;
    }

    IEnumerator DelayFinishInputRebinding(float delay)
    {
        displayingRebindText = false;
        rebindCancelTimerText.text = "";
        rebinding = false;
        yield return new WaitForSeconds(delay);
        gameInputManager.Rebind(false);
    }

    static void OnActionChange(object obj, InputActionChange change)
    {
        if(change != InputActionChange.BoundControlsChanged)
            return;

        InputAction action = obj as InputAction;
        InputActionMap actionMap = action?.actionMap ?? obj as InputActionMap;
        InputActionAsset actionAsset = actionMap?.asset ?? obj as InputActionAsset;

        for(int i = 0; i < s_RebindActionUIs.Count; ++i)
        {
            InputRebindingButton component = s_RebindActionUIs[i];
            InputAction referencedAction = component.actionReference?.action;

            if(referencedAction == null)
                continue;

            if(referencedAction == action ||
                referencedAction.actionMap == actionMap ||
                referencedAction.actionMap?.asset == actionAsset)
                component.UpdateBindingDisplay();
        }
    }

    void UpdateActionLabel()
    {
        if(m_ActionLabel != null)
        {
            InputAction action = m_Action?.action;
            m_ActionLabel.text = action != null ? action.name : string.Empty;
        }
    }

    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<InputRebindingButton, string, string, string>
    {
    }

    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<InputRebindingButton, InputActionRebindingExtensions.RebindingOperation>
    {
    }
}