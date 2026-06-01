using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SettingsManager settingsManager;
    [SerializeField] SettingType settingType;
    
    public enum SettingType { PostProcessing, DistortionEffects, WeatherEffects, Fullscreen, VSync, ShowFPS, ControllerVibration, JoystickSwap, UseDPad, LockAiming, CameraShake }
    GameManager gameManager;
    Toggle toggle;

    void Start()
    {
        gameManager = GameManager.Instance;
        toggle = GetComponent<Toggle>();
    }
    
	public void OnPointerClick(PointerEventData eventData)
	{
        if(!toggle.interactable)
            return;
            
		switch(settingType)
        {
            case SettingType.PostProcessing:
            ToggleColorPP();
            break;
            case SettingType.DistortionEffects:
            ToggleDistortionEffects();
            break;
            case SettingType.WeatherEffects:
            ToggleWeatherEffects();
            break;
            case SettingType.Fullscreen:
            ToggleFullscreen();
            break;
            case SettingType.VSync:
            ToggleVSync();
            break;
            case SettingType.ShowFPS:
            ToggleFPSText();
            break;
            case SettingType.ControllerVibration:
            ToggleControllerVibration();
            break;
            case SettingType.JoystickSwap:
            ToggleJoystickSwap();
            break;
            case SettingType.UseDPad:
            ToggleUseDPad();
            break;
            case SettingType.LockAiming:
            ToggleLockAiming();
            break;
            case SettingType.CameraShake:
            ToggleCameraShake();
            break;
        }
	}

    void ToggleColorPP()
    {
        gameManager.postProcessing = !gameManager.postProcessing;
        ScreenEffectsManager.Instance.UpdatePostProcessing();
    }

    void ToggleDistortionEffects()
    {
        gameManager.distortionEffects = !gameManager.distortionEffects;
        WorldManager.Instance.UpdateDistortionEffects(gameManager.distortionEffects);
    }
    
    void ToggleWeatherEffects()
    {
        gameManager.weatherEffects = !gameManager.weatherEffects;
        WorldManager.Instance.UpdateWeatherEffects(gameManager.weatherEffects);
    }

    void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    void ToggleVSync()
    {
        gameManager.vSync = !gameManager.vSync;
        settingsManager.ToggleFrameRateSettings(gameManager.vSync);
        QualitySettings.vSyncCount = gameManager.vSync ? 1 : 0;
    }

    void ToggleFPSText()
    {
        gameManager.showPerformanceData = !gameManager.showPerformanceData;
        settingsManager.UpdateFPSText(gameManager.showPerformanceData);
    }

    void ToggleControllerVibration()
    {
        gameManager.controllerVibration = !gameManager.controllerVibration;

        if(gameManager.controllerVibration)
            GameInputManager.Instance.RumbleController(0.9f, 0.9f, 0.5f);
    }

    void ToggleJoystickSwap()
    {
        gameManager.swapJoysticks = !gameManager.swapJoysticks;
        settingsManager.UpdateDPadText(gameManager.swapJoysticks);
    }

    void ToggleUseDPad()
    {
        gameManager.useDPad = !gameManager.useDPad;
    }

    void ToggleLockAiming()
    {
        gameManager.lockAiming = !gameManager.lockAiming;
    }

    void ToggleCameraShake()
    {
        gameManager.cameraShake = !gameManager.cameraShake;
    }
}