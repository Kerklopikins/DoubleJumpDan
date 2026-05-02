using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] Slider aimSensitivitySlider;
    [SerializeField] Slider cursorSensitivitySlider;
    [SerializeField] Toggle controllerVibrationToggle;
    [SerializeField] Toggle swapJoysticksToggle;
    [SerializeField] Toggle useDPadToggle;

    [Header("Input Settings")]  
    [SerializeField] Button inputSettingsButton;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] GameObject inputSettingsGameObject;
    [SerializeField] ScrollRect inputSettingsScrollRect;
    [SerializeField] RectTransform inputSettingsContent;
    [SerializeField] Button[] keyboardButtons;
    [SerializeField] Button[] controllerButtons;
    [SerializeField] InputRebindingButton[] inputRebindingButtons;

    public MainMenuManager mainMenuManager { get; private set; }
    List<Vector2> screenResolutions = new List<Vector2>();
    List<int> frameRates = new List<int>();
    GameManager gameManager;
    GameHUD gameHUD;
    GameInputManager gameInputManager;
    LevelLoadingManager levelLoadingManager;
    bool inMainMenu;

    void Start()
    {
        gameManager = GameManager.Instance;
        gameInputManager = GameInputManager.Instance;
        levelLoadingManager = LevelLoadingManager.Instance;
        gameInputManager.OnRebind += Rebind;

        if(GetComponent<MainMenuManager>() != null)
        {
            inMainMenu = true;            
            mainMenuManager = GetComponent<MainMenuManager>();
        }
        else if(GetComponent<GameHUD>() != null)
        {
            gameHUD = GetComponent<GameHUD>();
        }

        volumeSliders[0].value = gameManager.sfxVolume;
        volumeSliders[1].value = gameManager.musicVolume;

        frameRates.Add(30);
        frameRates.Add(60);
        frameRates.Add(120);
        frameRates.Add(144);
        frameRates.Add(-1);

        if(mainMenuManager != null)
        {
            gameInputManager.OnControllerChanged += OnControllerChanged;

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

            mainMenuManager.cursorSpeed = 1600 * cursorSensitivitySlider.value;

            aimSensitivitySlider.value = gameManager.aimSensitivity;
            cursorSensitivitySlider.value = gameManager.cursorSensitivity;
            controllerVibrationToggle.isOn = gameManager.controllerVibration;
            swapJoysticksToggle.isOn = gameManager.swapJoysticks;
            useDPadToggle.isOn = gameManager.useDPad;

            postProcessingToggle.isOn = gameManager.postProcessing;
            distortionEffectsToggle.isOn = gameManager.distortionEffects;
            weatherEffectsToggle.isOn = gameManager.weatherEffects;

            if(gameInputManager.ControllerConnected())
            {
                inputSettingsScrollRect.inertia = false;

                for(int i = 0; i < keyboardButtons.Length; i++)
                    keyboardButtons[i].interactable = false;
            }
            else
            {
                for(int i = 0; i < controllerButtons.Length; i++)
                    controllerButtons[i].interactable = false;
            }

            inputSettingsButton.onClick.AddListener(RefreshInputSettings);
        }
        else if(gameHUD != null)
        {
            gameHUD.cursorSpeed = 1600 * gameManager.cursorSensitivity;
            gameHUD.crosshairsSmoothSpeed = 16 * gameManager.aimSensitivity;
        }

        UpdateFPSText(gameManager.showPerformanceData);
    }

    void Update()
    {
        if(!inMainMenu)
            return;

        if(inputSettingsGameObject.activeSelf == false || levelLoadingManager.Busy)
            return;
            
        if(gameInputManager.ControllerConnected())
        {
            if(Mathf.Abs(gameInputManager.ScrollDirection().y) > 0.1f)
            {
                if(gameInputManager.FastCursorButton())
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                inputSettingsContent.anchoredPosition += new Vector2(0, -gameInputManager.ScrollDirection().y * mainMenuManager._scrollSpeed * Time.deltaTime);
                inputSettingsContent.anchoredPosition = new Vector2(inputSettingsContent.anchoredPosition.x, Mathf.Clamp(inputSettingsContent.anchoredPosition.y, 0, inputSettingsContent.sizeDelta.y - 484));
            }
        }
    }

    void Rebind(bool enabled)
    {
        if(enabled)
        {
            if(keyboardButtons[0].interactable || controllerButtons[0].interactable)
                AudioManager.Instance.PlaySound2D(buttonClickSound);

            for(int i = 0; i < keyboardButtons.Length; i++)
                keyboardButtons[i].interactable = false;

            for(int i = 0; i < controllerButtons.Length; i++)
                controllerButtons[i].interactable = false;
        }
        else
        {
            if(gameInputManager.ControllerConnected())
                OnControllerChanged(true);
            else
                OnControllerChanged(false);
        }
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
        {
            inputSettingsScrollRect.inertia = false;

            for(int i = 0; i < keyboardButtons.Length; i++)
                keyboardButtons[i].interactable = false;

            for(int i = 0; i < controllerButtons.Length; i++)
                controllerButtons[i].interactable = true;
        }
        else
        {
            inputSettingsScrollRect.inertia = true;

            for(int i = 0; i < keyboardButtons.Length; i++)
                keyboardButtons[i].interactable = true;

            for(int i = 0; i < controllerButtons.Length; i++)
                controllerButtons[i].interactable = false;
        }
    }

    public void RefreshInputSettings()
    {
        StartCoroutine(DelayInputSettingsContentCentering());
    }

    IEnumerator DelayInputSettingsContentCentering()
    {
        yield return null;
        yield return null;

        inputSettingsScrollRect.verticalNormalizedPosition = 1;
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

    public void RestoreDefaultControllerSettings()
    {
        aimSensitivitySlider.value = 0.5f;
        cursorSensitivitySlider.value = 0.5f;
        controllerVibrationToggle.isOn = true;
        swapJoysticksToggle.isOn = false;
        useDPadToggle.isOn = false;

        gameManager.swapJoysticks = false;
        gameManager.useDPad = false;
        gameManager.controllerVibration = true;

        for(int i = 0; i < inputRebindingButtons.Length; i++)
            inputRebindingButtons[i].ResetToDefault();

        AdjustCursorSensitivity();
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
        gameManager.inputBindings = gameInputManager.inputActions.SaveBindingOverridesAsJson();

        if(mainMenuManager != null)
        {    
            gameManager.aimSensitivity = aimSensitivitySlider.value;
            gameManager.cursorSensitivity = cursorSensitivitySlider.value;
        }

        gameManager.SaveData();
    }

    public void AdjustCursorSensitivity()
    {
        mainMenuManager.cursorSpeed = 1600 * cursorSensitivitySlider.value;
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