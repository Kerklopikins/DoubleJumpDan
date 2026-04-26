using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Collections;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    [SerializeField] InputActionAsset inputActions;

    public event Action<bool> OnControllerChanged;
    InputAction move;
    InputAction shoot;
    InputAction reload;
    InputAction jump;
    InputAction strafe;
    InputAction pause;
    InputAction escape;
    InputAction enter;
    InputAction screenshot;
    InputAction devMode;
    InputAction aim;
    InputAction click;
    InputAction point;
    InputAction joystickMove;
    InputAction leftTrigger;
    InputAction leftBumper;
    InputAction rightBumper;
    Camera _camera;
    Coroutine rumbleCoroutine;
    bool controllerConnectionState;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        move = inputActions.FindAction("Move");
        jump = inputActions.FindAction("Jump");
        strafe = inputActions.FindAction("Strafe");
        shoot = inputActions.FindAction("Shoot");
        reload = inputActions.FindAction("Reload");
        pause = inputActions.FindAction("Pause");
        escape = inputActions.FindAction("Escape");
        enter = inputActions.FindAction("Return");
        aim = inputActions.FindAction("Aim");
        screenshot = inputActions.FindAction("Screenshot");
        devMode = inputActions.FindAction("Dev Mode");
        click = inputActions.FindAction("Click");
        joystickMove = inputActions.FindAction("Joystick Move");
        point = inputActions.FindAction("Point");
        leftTrigger = inputActions.FindAction("Left Trigger");
        leftBumper = inputActions.FindAction("Left Bumper");
        rightBumper = inputActions.FindAction("Right Bumper");
        move.Enable();
        jump.Enable();
        strafe.Enable();
        shoot.Enable();
        reload.Enable();
        pause.Enable();
        escape.Enable();
        enter.Enable();
        screenshot.Enable();
        devMode.Enable();
        aim.Enable();
        click.Enable();
        joystickMove.Enable();
        point.Enable();
        leftTrigger.Enable();
        leftBumper.Enable();
        rightBumper.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        jump.Enable();
        strafe.Disable();
        shoot.Disable();
        reload.Disable();
        pause.Disable();
        escape.Disable();
        enter.Disable();
        screenshot.Disable();
        devMode.Disable();
        aim.Disable();
        click.Disable();
        joystickMove.Disable();
        point.Disable();
        leftTrigger.Disable();
        leftBumper.Disable();
        rightBumper.Disable();
    }

    void Start()
    {
        if(!GameManager.Instance.InMainMenu())
            _camera = LevelManager.Instance.mainCamera;
        else
            _camera = Camera.main;

        controllerConnectionState = ControllerConnected();
        OnControllerChanged?.Invoke(ControllerConnected());
    }

    void Update()
    {
        if(controllerConnectionState != ControllerConnected())
        {
            OnControllerChanged?.Invoke(ControllerConnected());
            controllerConnectionState = ControllerConnected();
        }
    }

    public bool ControllerConnected()
    {
        if(Gamepad.all.Count > 0)
            return true;
        else
            return false;
    }

    public float GetHorizontalInput()
    {
        return move.ReadValue<Vector2>().x;
    }

    public float GetVerticalInput()
    {
        return move.ReadValue<Vector2>().y;
    }

    public Vector2 GetJoystickMovement()
    {
        return joystickMove.ReadValue<Vector2>();
    }

    public Vector2 AimDirection()
    {
        return aim.ReadValue<Vector2>();
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

    public bool StrafeButton()
    {
        if(strafe.IsPressed())
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

    public Vector3 GetRealMousePosition()
    {
        return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public bool DevModeButtonDown()
    {
        if(devMode.WasPressedThisFrame())
            return true;
        else
            return false;
    }

    public bool LeftTrigger()
    {
        if(leftTrigger.IsPressed())
            return true;
        else
            return false;
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
        if(rumbleCoroutine != null)
            StopCoroutine(rumbleCoroutine);
        
        rumbleCoroutine = StartCoroutine(RumbleControllerCo(low, high, duration));
    }

    IEnumerator RumbleControllerCo(float low, float high, float duration)
    {
        if(Gamepad.current == null)
            yield break;
            
        Gamepad.current.SetMotorSpeeds(low, high);
        yield return new WaitForSecondsRealtime(duration);
        Gamepad.current.SetMotorSpeeds(0, 0);
    }
}
