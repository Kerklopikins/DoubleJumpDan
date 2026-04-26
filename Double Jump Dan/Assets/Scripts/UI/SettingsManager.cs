using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] Slider[] volumeSliders;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Toggle distortionEffectsToggle;
    [SerializeField] Toggle weatherEffectsToggle;
    [SerializeField] GameObject fpsTracker;

    [Header("Main Menu Only Settings")]
    [SerializeField] GameObject resettedGameImage;
	[SerializeField] Slider screenResolutionSlider;
	[SerializeField] Text screenResolutionText;
	[SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider frameRateSlider;
	[SerializeField] Text frameRateText;
    [SerializeField] Text frameRateTitleText;
    [SerializeField] Image frameRateSliderFill;
    [SerializeField] Sprite[] sliderFillSprites;
    [SerializeField] Button changeFrameRateButton;
	[SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle showFPSToggle;
    
    public MainMenuManager mainMenuManager { get; private set; }
    List<Vector2> screenResolutions = new List<Vector2>();
    List<int> frameRates = new List<int>();
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        if(GetComponent<MainMenuManager>() != null)
            mainMenuManager = GetComponent<MainMenuManager>();

        volumeSliders[0].value = gameManager.sfxVolume;
        volumeSliders[1].value = gameManager.musicVolume;

        frameRates.Add(30);
        frameRates.Add(60);
        frameRates.Add(120);
        frameRates.Add(144);
        frameRates.Add(-1);

        if(mainMenuManager != null)
        {
            for(int i = 0; i < Screen.resolutions.Length; i++)
            {
                if(Screen.resolutions[i].width >= 512)
                    if(!screenResolutions.Contains(new Vector2(Screen.resolutions[i].width, Screen.resolutions[i].height)))
                        screenResolutions.Add(new Vector2(Screen.resolutions[i].width, Screen.resolutions[i].height));
            }
            
            if(gameManager.screenResolution == -1 || gameManager.frameRate == -1)
            {
                Screen.SetResolution((int)screenResolutions[screenResolutions.Count - 1].x, (int)screenResolutions[screenResolutions.Count - 1].y, true);
                Application.targetFrameRate = frameRates[frameRates.Count - 1];

                LevelLoadingManager.Instance.ForceFadeResizeBackground();
                gameManager.frameRate = frameRates.Count - 1;
                gameManager.screenResolution = screenResolutions.Count - 1;
                gameManager.SaveData();
            }
            else if(gameManager.screenResolution > screenResolutions.Count - 1)
            {
                gameManager.screenResolution = screenResolutions.Count - 1;
                gameManager.SaveData();
            }

            frameRateSlider.maxValue = frameRates.Count - 1;
            frameRateSlider.value = gameManager.frameRate;

            ToggleFrameRateSettings(gameManager.vSync);
            
            screenResolutionSlider.maxValue = screenResolutions.Count - 1;
            screenResolutionSlider.value = gameManager.screenResolution;
            
            UpdateScreenResolutionText();
            UpdateFrameRateText();

            RefreshFullscreenToggle();

            vSyncToggle.isOn = gameManager.vSync;
            showFPSToggle.isOn = gameManager.showPerformanceData;

            QualitySettings.vSyncCount = gameManager.vSync ? 1 : 0;
        
            if(!gameManager.vSync)
                Application.targetFrameRate = frameRates[gameManager.frameRate];
        }

        UpdateFPSText(gameManager.showPerformanceData);

        postProcessingToggle.isOn = gameManager.postProcessing;
        distortionEffectsToggle.isOn = gameManager.distortionEffects;
        weatherEffectsToggle.isOn = gameManager.weatherEffects;
    }

    public void RefreshFullscreenToggle()
    {
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void UpdateScreenResolution()
    {
        gameManager.screenResolution = (int)screenResolutionSlider.value;
        Screen.SetResolution((int)screenResolutions[gameManager.screenResolution].x, (int)screenResolutions[gameManager.screenResolution].y, Screen.fullScreen);
        gameManager.SaveData();
    }

    public void UpdateFrameRate()
    {
        gameManager.frameRate = (int)frameRateSlider.value;
        Application.targetFrameRate = frameRates[gameManager.frameRate];
        gameManager.SaveData();
    }

    public void ToggleFrameRateSettings(bool vSyncOn)
    {
        if(vSyncOn)
        {
            gameManager.frameRate = frameRates.Count - 1;
            Application.targetFrameRate = frameRates[gameManager.frameRate];
            frameRateTitleText.text = "Disable VSync to Adjust Frame Rate Limit";
            frameRateSliderFill.sprite = sliderFillSprites[1];
        }
        else
        {
            frameRateTitleText.text = "Frame Rate Limit";
            frameRateSliderFill.sprite = sliderFillSprites[0];
        }

        frameRateSlider.value = gameManager.frameRate;

        UpdateFrameRateText();

        frameRateSlider.interactable = !vSyncOn;
        changeFrameRateButton.interactable = !vSyncOn;
    }

    public void UpdateFPSText(bool isOn)
    {
        if(isOn)
            fpsTracker.gameObject.SetActive(true);
        else
            fpsTracker.gameObject.SetActive(false);
    }
    
    public void UpdateScreenResolutionText()
    {
        screenResolutionText.text = screenResolutions[(int)screenResolutionSlider.value].x.ToString() + "x" + screenResolutions[(int)screenResolutionSlider.value].y.ToString();
    }

    public void UpdateFrameRateText()
    {
        if(!gameManager.vSync)
        {
            if(frameRateSlider.value < frameRates.Count - 1)
                frameRateText.text = frameRates[(int)frameRateSlider.value].ToString() + " FPS";
            else
                frameRateText.text = "Unlimited";
        }
        else
        {
            frameRateText.text = "Unlimited";
        }
    }

    public void SaveSettings()
    {
        gameManager.musicVolume = volumeSliders[1].value;
        gameManager.sfxVolume = volumeSliders[0].value;
        gameManager.SaveData();
    }

    public void AdjustSfxVolume()
    {
        AudioManager.Instance.sfxVolumePercent = volumeSliders[0].value;
        AudioManager.Instance.SetVolume(volumeSliders[0].value, AudioManager.AudioChannel.Sfx);
    }

    public void AdjustMusicVolume()
    {
        AudioManager.Instance.musicVolumePercent = volumeSliders[1].value;
        AudioManager.Instance.SetVolume(volumeSliders[1].value, AudioManager.AudioChannel.Music);
    }

    public void ResetGame()
    {
        resettedGameImage.SetActive(true);
        gameManager.ResetGame();
        PlayerPrefs.DeleteAll();
        StartCoroutine(ResetGameCo());
    }

    IEnumerator ResetGameCo()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main Menu");
    }
}