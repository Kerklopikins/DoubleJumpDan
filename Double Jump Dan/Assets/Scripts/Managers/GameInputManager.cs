using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    public InputActionAsset inputActions;

    public float HorizontalInputSensitivity { get; private set; }
    public float VerticalInputSensitivity { get; private set; }
    public event Action<bool> OnRebind;
    public bool Rebinding { get; set; }
    public InputMode inputMode { get; private set; }
    public enum InputMode { KeyboardAndMouse, Controller }
    public event Action<bool> OnControllerChanged;
    public event Action<bool> OnKeyboardOnlyInputChanged;
    DPadUse dPadUse;
    public enum DPadUse { Move, Aim }
    InputAction move;
    InputAction shoot;
    InputAction reload;
    InputAction jump;
    InputAction sprint;
    InputAction pause;
    InputAction escape;
    InputAction enter;
    InputAction screenshot;
    InputAction aim;
    InputAction click;
    InputAction point;
    InputAction leftBumper;
    InputAction rightBumper;

    InputAction controllerCursorMove;
    InputAction controllerScrolling;
    InputAction controllerFastCursor;

    InputAction switchKeyboardMode;
    InputAction keyboardCursorMove;
    InputAction keyboardScrolling;
    InputAction keyboardFastCursor;

    InputAction dPad;
    Camera _camera;
    Coroutine rumbleCoroutine;
    public static bool IsControllerConnected;
    public static bool IsKeyboardOnly;
    Gamepad currentGamepad;
    GameManager gameManager;
    float currentRumbleAmount;
    
    public static readonly Dictionary<string, string> DisplayNameOverrides = new()
    {
        { "leftButton", "Left Click" },
        { "rightButton", "Right Click" },
        { "middleButton", "Middle Mouse Button" },
        { "leftTrigger", "Left Trigger" },
        { "rightTrigger", "Right Trigger" },
        { "leftShoulder", "Left Bumper" },
        { "rightShoulder", "Right Bumper" },
        { "leftStickPress", "Left Stick Button" },
        { "rightStickPress", "Right Stick Button" },
        { "leftShift", "Left Shift" },
        { "rightShift", "Right Shift" },
        { "leftCtrl", "Left Ctrl" },
        { "rightCtrl", "Right Ctrl" },
        { "leftAlt", "Left Alt" },
        { "rightAlt", "Right Alt" },
        { "leftBracket", "Left Bracket" },
        { "rightBracket", "Right Bracket" },
        { "period", "Period" },
        { "semicolon", "Semicolon" },
        { "slash", "Slash" },
        { "backslash", "Backslash" },
        { "comma", "Comma" },
        { "quote", "Quote" },
        { "leftArrow", "Left" },
        { "rightArrow", "Right" },
        { "upArrow", "Up" },
        { "downArrow", "Down" },
    };

    public static readonly Dictionary<string, string> XboxDisplayNameOverrides = new()
    {
        { "buttonSouth", "A" },
        { "buttonNorth", "Y" },
        { "buttonWest", "X" },
        { "buttonEast", "B" },
        { "leftTrigger", "Left Trigger" },
        { "rightTrigger", "Right Trigger" },
        { "leftShoulder", "Left Bumper" },
        { "rightShoulder", "Right Bumper" },
        { "leftStickPress", "Left Stick Button" },
        { "rightStickPress", "Right Stick Button" },
        { "start", "Menu" },
        { "select", "View" },
    };

    public static readonly Dictionary<string, string> PSDisplayNameOverrides = new()
    {
        { "buttonSouth", "Cross" },
        { "buttonNorth", "Triangle" },
        { "buttonWest", "Circle" },
        { "buttonEast", "Square" },
        { "leftTrigger", "Left Trigger" },
        { "rightTrigger", "Right Trigger" },
        { "leftShoulder", "Left Bumper" },
        { "rightShoulder", "Right Bumper" },
        { "leftStickPress", "Left Stick Button" },
        { "rightStickPress", "Right Stick Button" },
        { "start", "Options" },
        { "select", "Share" },
    };
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        
        string rebinds = gameManager.inputBindings;

        if(!string.IsNullOrEmpty(rebinds))
            inputActions.LoadBindingOverridesFromJson(rebinds);

        move = inputActions.FindAction("Move");
        jump = inputActions.FindAction("Jump");
        sprint = inputActions.FindAction("Sprint");
        shoot = inputActions.FindAction("Shoot");
        reload = inputActions.FindAction("Reload");
        pause = inputActions.FindAction("Pause");
        escape = inputActions.FindAction("Escape");
        enter = inputActions.FindAction("Return");
        aim = inputActions.FindAction("Aim");
        screenshot = inputActions.FindAction("Screenshot");
        click = inputActions.FindAction("Click");
        point = inputActions.FindAction("Point");
        leftBumper = inputActions.FindAction("Left Bumper");
        rightBumper = inputActions.FindAction("Right Bumper");
        dPad = inputActions.FindAction("D Pad");

        controllerCursorMove = inputActions.FindAction("Cursor Move Controller");
        controllerScrolling = inputActions.FindAction("Scrolling Controller");
        controllerFastCursor = inputActions.FindAction("Fast Cursor Controller");

        switchKeyboardMode = inputActions.FindAction("Switch Keyboard Mode");
        keyboardCursorMove = inputActions.FindAction("Cursor Move Keyboard");
        keyboardScrolling = inputActions.FindAction("Scrolling Keyboard");
        keyboardFastCursor = inputActions.FindAction("Fast Cursor Keyboard");

        //defaultMovePath = move.bindings[0].effectivePath;
        //defaultAimPath = aim.bindings[0].effectivePath;

        move.Enable();
        jump.Enable();
        sprint.Enable();
        shoot.Enable();
        reload.Enable();
        pause.Enable();
        escape.Enable();
        enter.Enable();
        screenshot.Enable();
        aim.Enable();
        click.Enable();
        point.Enable();
        leftBumper.Enable();
        rightBumper.Enable();
        dPad.Enable();

        controllerCursorMove.Enable();
        controllerScrolling.Enable();
        controllerFastCursor.Enable();

        switchKeyboardMode.Enable();
        keyboardCursorMove.Enable();
        keyboardScrolling.Enable();
        keyboardFastCursor.Enable();

        HorizontalInputSensitivity = 0.2f;
        VerticalInputSensitivity = 0.5f;

        if(!gameManager.InMainMenu())
            _camera = LevelManager.Instance.mainCamera;
        else
            _camera = Camera.main;

        if(ControllerConnected())
            SetInput(InputMode.Controller);
        else
            SetInput(InputMode.KeyboardAndMouse);

        if(gameManager.swapJoysticks)
            dPadUse = DPadUse.Aim;
        else
            dPadUse = DPadUse.Move;

        RumbleController(0, 0, 0);
    }

    public bool ControllerConnected()
    {
        return IsControllerConnected;
    }

    public bool KeyboardOnly()
    {
        return IsKeyboardOnly;
    }

    public void Rebind(bool enabled)
    {
        Rebinding = enabled;
        OnRebind?.Invoke(enabled);
    }

    void Update()
    {
        if(Rebinding)
            return;

        if(switchKeyboardMode.WasPressedThisFrame())
        {   
            IsKeyboardOnly = !IsKeyboardOnly;
            OnKeyboardOnlyInputChanged?.Invoke(IsKeyboardOnly);
        }
        
        if(Gamepad.current != null)
        {
            currentGamepad = Gamepad.current;
            
            if(currentGamepad.leftStick.ReadValue().magnitude > 0.1f || currentGamepad.rightStick.ReadValue().magnitude > 0.1f)
            {
                SetInput(InputMode.Controller);
                return;
            }

            foreach(var control in currentGamepad.allControls)
            {
                if(control is ButtonControl button && button.wasPressedThisFrame)
                {
                    SetInput(InputMode.Controller);
                    return;
                }
            }
        }    

        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            SetInput(InputMode.KeyboardAndMouse);
            return;
        }

        if(Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SetInput(InputMode.KeyboardAndMouse);
            return;
        }
    }

    void SetInput(InputMode newMode)
    {
        if(inputMode == newMode)
            return;

        inputMode = newMode;

        if(inputMode == InputMode.KeyboardAndMouse)
        {
            OnControllerChanged?.Invoke(false);
            IsControllerConnected = false;           
        }

        if(inputMode == InputMode.Controller)
        {
            OnControllerChanged?.Invoke(true);
            IsControllerConnected = true;
        }
    }

    public float HorizontalMoveInput()
    {
        if(ControllerConnected())
        {
            if(gameManager.swapJoysticks)
            {
                return aim.ReadValue<Vector2>().x;
            }
            else
            {
                if(dPadUse == DPadUse.Move && gameManager.useDPad)
                    return dPad.ReadValue<Vector2>().x;
                else
                    return move.ReadValue<Vector2>().x;
            }
        }
        else
        {
            return move.ReadValue<Vector2>().x;
        }
    }

    public float VerticalMoveInput()
    {
        if(ControllerConnected())
        {
            if(gameManager.swapJoysticks)
            {
                return aim.ReadValue<Vector2>().y;
            }
            else
            {
                if(dPadUse == DPadUse.Move && gameManager.useDPad)
                    return dPad.ReadValue<Vector2>().y;
                else
                    return move.ReadValue<Vector2>().y;
            }
        }
        else
        {
            return move.ReadValue<Vector2>().y;
        }
    }

    public Vector2 AimDirection()
    {
        if(ControllerConnected())
        {
            if(gameManager.swapJoysticks)
            {
                if(dPadUse == DPadUse.Aim && gameManager.useDPad)
                    return dPad.ReadValue<Vector2>();
                else
                    return move.ReadValue<Vector2>();
            }
            else
            {
                return aim.ReadValue<Vector2>();
            }
        }
        else
        {
            return Vector2.zero;
        }
    }

    public bool Click()
    {
        if(click.IsPressed())
            return true;
        else
            return false;
    }

    public Vector2 ControllerCursorMove()
    {
        if(gameManager != null && gameManager.useDPad)
            return dPad.ReadValue<Vector2>();
        else
            return controllerCursorMove.ReadValue<Vector2>();
    }

    public Vector2 ControllerScrolling()
    {
        return controllerScrolling.ReadValue<Vector2>();
    }

    public bool ControllerFastCursor()
    {
        if(controllerFastCursor.IsPressed())
            return true;
        else
            return false;
    }

    public Vector2 KeyboardCursorMove()
    {
        return keyboardCursorMove.ReadValue<Vector2>();
    }

    public Vector2 KeyboardScrolling()
    {
        return keyboardScrolling.ReadValue<Vector2>();
    }

    public bool KeyboardFastCursor()
    {
        if(keyboardFastCursor.IsPressed())
            return true;
        else
            return false;
    }

    public bool SwitchKeyboardMode()
    {
        if(switchKeyboardMode.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool JumpButtonDown()
    {
        if(jump.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool JumpButtonUp()
    {
        if(jump.WasReleasedThisFrame())
            return true;
        else
            return false;
    }

    public bool SprintButton()
    {
        if(sprint.IsPressed())
            return true;
        else
            return false;
    }

    public bool ShootButton()
    {
        if(shoot.IsPressed())
            return true;
        else
            return false;
    }

    public bool ShootButtonDown()
    {
        if(shoot.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool ShootButtonUp()
    {
        if(shoot.WasReleasedThisFrame())
            return true;
        else
            return false;
    }

    public bool ReloadButtonDown()
    {
        if(reload.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool EscapeButtonDown()
    {
        if(escape.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool ReturnButtonDown()
    {
        if(enter.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool PauseButtonDown()
    {
        if(pause.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool ScreenshotButtonDown()
    {
        if(screenshot.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public Vector3 RealMousePosition()
    {
        return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public bool LeftBumperDown()
    {
        if(leftBumper.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool RightBumperDown()
    {
        if(rightBumper.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public void RumbleController(float low, float high, float duration)
    {
        if(gameManager.controllerVibration && inputMode == InputMode.Controller)
        {
            if(low + high < currentRumbleAmount)
                return;

            if(rumbleCoroutine != null)
                StopCoroutine(rumbleCoroutine);
        
            rumbleCoroutine = StartCoroutine(RumbleControllerCo(low, high, duration));
        }
    }

    IEnumerator RumbleControllerCo(float low, float high, float duration)
    {
        if(Gamepad.current == null)
            yield break;
        
        currentRumbleAmount = low + high;
        Gamepad.current.SetMotorSpeeds(low, high);
        yield return new WaitForSecondsRealtime(duration);

        if(Gamepad.current == null)
            yield break;

        Gamepad.current.SetMotorSpeeds(0, 0);
        currentRumbleAmount = 0;
    }
}
