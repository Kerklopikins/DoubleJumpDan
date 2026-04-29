using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SettingsManager settingsManager;
    [SerializeField] SettingType settingType;
    
    public enum SettingType { PostProcessing, DistortionEffects, WeatherEffects, Fullscreen, VSync, ShowFPS }
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }
    
	public void OnPointerClick(PointerEventData eventData)
	{
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
}