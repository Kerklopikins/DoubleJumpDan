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
    [SerializeField] GameObject fpsTracker;

    [Header("Toggles")]
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Toggle distortionEffectsToggle;
    [SerializeField] Toggle weatherEffectsToggle;
	[SerializeField] Toggle fullscreenToggle;
	[SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle cameraShakeToggle;
    [SerializeField] Toggle showFPSToggle;
    [SerializeField] Toggle controllerVibrationToggle;
    [SerializeField] Toggle swapJoysticksToggle;
    [SerializeField] Toggle useDPadToggle;
    [SerializeField] Toggle lockAimingToggle;

    [Header("Frame Rate Settings")]
    [SerializeField] Slider frameRateSlider;
	[SerializeField] Text frameRateText;
    [SerializeField] Text frameRateTitleText;
    [SerializeField] Image frameRateSliderFill;
    [SerializeField] Button changeFrameRateButton;

    [Header("Screen Resolution")]
    [SerializeField] Text monitorChangedText;
    [SerializeField] GameObject moniterChangedGameObject;
	[SerializeField] Slider screenResolutionSlider;
	[SerializeField] Text screenResolutionText;

    [Header("Sprites")]
    [SerializeField] Sprite[] sliderFillSprites;
    [SerializeField] Sprite[] sliderBackgroundSprites;
    [SerializeField] Sprite[] toggleSelectedSprites;

    [Header("Misc")]
    [SerializeField] GameObject resettedGameImage;

    [Header("Input Settings")]  
    [SerializeField] Button inputSettingsButton;
    [SerializeField] Slider aimSensitivitySlider;
    [SerializeField] Slider cursorSensitivitySlider;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] GameObject inputSettingsGameObject;
    [SerializeField] ScrollRect inputSettingsScrollRect;
    [SerializeField] RectTransform inputSettingsContent;
    [SerializeField] InputRebindingButton[] inputRebindingButtons;
    [SerializeField] Button[] keyboardButtons;
    [SerializeField] Button[] controllerButtons;
    [SerializeField] Image[] toggleSelectedImages;
    [SerializeField] Image[] sliderBackgroundImages;
    [SerializeField] Image[] controllerSlidersFills;
    [SerializeField] Animator rebindOverlayAnimator;
    [SerializeField] Text dPadText;

    [Header("Controller Icons")]
    [SerializeField] KeyboardMouseIcons keyboardMouse;
    [SerializeField] ControllerIcons xbox;
    [SerializeField] ControllerIcons playStation;

    public MainMenuManager mainMenuManager { get; private set; }
    List<Vector2> screenResolutions = new List<Vector2>();
    List<Vector2> temporaryResolutions = new List<Vector2>();
    Vector2 currentResolution;
    List<int> frameRates = new List<int>();
    GameManager gameManager;
    GameHUD gameHUD;
    GameInputManager gameInputManager;
    LevelLoadingManager levelLoadingManager;
    UIScreenManager uIScreenManager;
    bool inMainMenu;
    float screenResolutionsCheckTimer = 1;
    int screenResolutionsLength;
    float monitorChangedTimer;
    bool displayedMoniterChangedUI = true;
    //bool inEditor;

    void Start()
    {
        //#if UNITY_EDITOR
            //inEditor = true;
        //#endif

        gameManager = GameManager.Instance;
        gameInputManager = GameInputManager.Instance;
        levelLoadingManager = LevelLoadingManager.Instance;
        gameInputManager.OnRebind += Rebind;

        if(GetComponent<MainMenuManager>() != null)
        {
            inMainMenu = true;            
            mainMenuManager = GetComponent<MainMenuManager>();
            uIScreenManager = GetComponent<UIScreenManager>();
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
            gameInputManager.OnKeyboardOnlyInputChanged += OnKeyboardOnlyInputChanged;
            gameInputManager.OnRebind += AnimateRebindOverlay;
            
            screenResolutionsLength = Screen.resolutions.Length;

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

            currentResolution = screenResolutions[gameManager.screenResolution];

            frameRateSlider.maxValue = frameRates.Count - 1;
            frameRateSlider.value = gameManager.frameRate;

            ToggleFrameRateSettings(gameManager.vSync);
            
            screenResolutionSlider.maxValue = screenResolutions.Count - 1;
            screenResolutionSlider.value = gameManager.screenResolution;
            
            UpdateScreenResolutionText();
            UpdateFrameRateText();

            RefreshFullscreenToggle();

            vSyncToggle.isOn = gameManager.vSync;
            cameraShakeToggle.isOn = gameManager.cameraShake;
            showFPSToggle.isOn = gameManager.showPerformanceData;

            QualitySettings.vSyncCount = gameManager.vSync ? 1 : 0;
        
            if(!gameManager.vSync)
                Application.targetFrameRate = frameRates[gameManager.frameRate];

            mainMenuManager.cursorSpeed = 1600 * cursorSensitivitySlider.value;
            
            postProcessingToggle.isOn = gameManager.postProcessing;
            distortionEffectsToggle.isOn = gameManager.distortionEffects;
            weatherEffectsToggle.isOn = gameManager.weatherEffects;

            aimSensitivitySlider.value = gameManager.aimSensitivity;
            cursorSensitivitySlider.value = gameManager.cursorSensitivity;
            controllerVibrationToggle.isOn = gameManager.controllerVibration;
            swapJoysticksToggle.isOn = gameManager.swapJoysticks;
            useDPadToggle.isOn = gameManager.useDPad;
            lockAimingToggle.isOn = gameManager.lockAiming;
            
            UpdateDPadText(gameManager.swapJoysticks);
            
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

                for(int i = 0; i < controllerSlidersFills.Length; i++)
                    controllerSlidersFills[i].sprite = sliderFillSprites[1];

                for(int i = 0; i < toggleSelectedImages.Length; i++)
                    toggleSelectedImages[i].sprite = toggleSelectedSprites[1];

                for(int i = 0; i < sliderBackgroundImages.Length; i++)
                    sliderBackgroundImages[i].sprite = sliderBackgroundSprites[1];
                
                aimSensitivitySlider.interactable = false;
                cursorSensitivitySlider.interactable = false;

                controllerVibrationToggle.interactable = false;
                swapJoysticksToggle.interactable = false;
                useDPadToggle.interactable = false;
            }

            inputSettingsButton.onClick.AddListener(RefreshInputSettings);

            foreach(InputRebindingButton component in inputRebindingButtons)
            {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
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
    
        if(monitorChangedTimer > 0)
        {
            monitorChangedTimer -= Time.deltaTime;
        }
        else if(monitorChangedTimer <= 0 && !displayedMoniterChangedUI)
        {
            displayedMoniterChangedUI = true;
            moniterChangedGameObject.SetActive(false);
        }

        if(screenResolutionsCheckTimer > 0)
        {
            screenResolutionsCheckTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            if(Screen.resolutions.Length != screenResolutionsLength)
            {
                screenResolutions.Clear();
                screenResolutionsLength = Screen.resolutions.Length;

                for(int i = 0; i < Screen.resolutions.Length; i++)
                {
                    if(Screen.resolutions[i].width >= 512)
                        if(!screenResolutions.Contains(new Vector2(Screen.resolutions[i].width, Screen.resolutions[i].height)))
                            screenResolutions.Add(new Vector2(Screen.resolutions[i].width, Screen.resolutions[i].height));
                }

                screenResolutionSlider.maxValue = screenResolutions.Count - 1;
    
                if(currentResolution.x > screenResolutions[screenResolutions.Count - 1].x || currentResolution.y > screenResolutions[screenResolutions.Count - 1].y)
                {
                    print("Current res is GREATER than screenResolutions max size");

                    DisplayMonitorChangeUI("Previous resolution of " + currentResolution.x + " x " + currentResolution.y + "\nis greater than the max resolution of " +
                    screenResolutions[screenResolutions.Count - 1].x + " x " + screenResolutions[screenResolutions.Count - 1].y + " on this monitor");

                    screenResolutionSlider.value = screenResolutions.Count - 1;
                    UpdateScreenResolution();
                    UpdateScreenResolutionText();
                }
                else
                {
                    print("Current res is LESS than screenResolutions max size");
                    
                    if(screenResolutions.Contains(currentResolution))
                    {
                        print("CONTAINS RESOLUTION");

                        DisplayMonitorChangeUI(currentResolution.x + " x " + currentResolution.y + " is supported on both monitors");

                        if(temporaryResolutions.Count > 0)
                            temporaryResolutions.Clear();

                        screenResolutionSlider.value = screenResolutions.IndexOf(currentResolution);
                        UpdateScreenResolution();
                        UpdateScreenResolutionText();
                    }
                    else if(!screenResolutions.Contains(currentResolution))
                    {
                        print("DOESNT CONTAIN RESOLUTION");
                        if(temporaryResolutions.Count > 0)
                            temporaryResolutions.Clear();

                        print(screenResolutions.Count + " screenres count");

                        foreach(Vector2 resolution in screenResolutions)
                        {
                            if(resolution.x <= currentResolution.x || resolution.y <= currentResolution.y)
                                temporaryResolutions.Add(resolution);
                            else
                                break;
                        }
                        
                        ///MAKE MONITOR CHANGE TEXT GLOBAL, EVEN when settings arn't open, and in actual levels maybe
                        /// Test on actual build
                        /// test if openeing game on big monitor, then quitting, then opening on small one, that it
                        /// resizes correctely
                        /// Maybe replace 
                        ///Only happens at the very start, bevcause temp resolutions arnt set
                        ////FIX WHEN MOVING GAME FROM DIFFERENT MONITOR WHEN SETTINGS ARNT open, and temporayry resolutions arnt set
                        DisplayMonitorChangeUI(currentResolution.x + " x " + currentResolution.y + " is not natively supported by this monitor\n" +
                        temporaryResolutions[temporaryResolutions.Count - 1].x + " x " + temporaryResolutions[temporaryResolutions.Count - 1].y + " is the closet supported native resolution");

                        screenResolutionSlider.value = temporaryResolutions.Count - 1;
                        UpdateScreenResolution();
                        UpdateScreenResolutionText();
                    }
                }
            }

            screenResolutionsCheckTimer = 1;
        }

        if(inputSettingsGameObject.activeInHierarchy == false || levelLoadingManager.Busy)
            return;
            
        if(gameInputManager.ControllerConnected())
            ScrollContent(gameInputManager.ControllerScrolling(), gameInputManager.ControllerFastCursor());  
        else if(gameInputManager.KeyboardOnly())
            ScrollContent(gameInputManager.KeyboardScrolling(), gameInputManager.KeyboardFastCursor());  
    }
    
    void DisplayMonitorChangeUI(string message)
    {
        uIScreenManager.transitionTimer = 3.25f;
        monitorChangedTimer = 3;
        displayedMoniterChangedUI = false;
        moniterChangedGameObject.SetActive(true);

        monitorChangedText.text = message;
    }

    void ScrollContent(Vector2 input, bool fastCursor)
    {
        if(Mathf.Abs(input.y) > 0.1f)
        {
            if(fastCursor)
                mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
            else
                mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

            inputSettingsContent.anchoredPosition += new Vector2(0, -input.y * mainMenuManager._scrollSpeed * Time.deltaTime);
            inputSettingsContent.anchoredPosition = new Vector2(inputSettingsContent.anchoredPosition.x, Mathf.Clamp(inputSettingsContent.anchoredPosition.y, 0, inputSettingsContent.sizeDelta.y - 484));
        }
    }

    void AnimateRebindOverlay(bool open)
    {
        rebindOverlayAnimator.SetBool("Open", open);

        if(!open)
            uIScreenManager.transitionTimer = 0.275f;
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

    void UpdateCompositeIcons(InputRebindingButton component, InputAction action, int compositeIndex)
    {
        int iconIndex = 0;

        for(int i = compositeIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
        {
            InputBinding binding = action.bindings[i];
            string rawPath = binding.effectivePath;

            if(string.IsNullOrEmpty(rawPath))
                continue;
            
            InputControl control = InputSystem.FindControl(rawPath);

            if(control == null)
                continue;

            string deviceLayout = control.device.layout;
            string controlPath = control.path.Substring(control.path.LastIndexOf('/') + 1);        

            Sprite icon = GetIcon(deviceLayout, controlPath);

            if (icon != null && iconIndex < component.m_CompositeControlImages.Count)
                component.m_CompositeControlImages[iconIndex].sprite = icon;

            iconIndex++;
        }
    }

    protected void OnUpdateBindingDisplay(InputRebindingButton component, string bindingDisplayString, string deviceLayoutName, string controlPath)
    {
        InputAction action = component.actionReference.action;
        int bindingIndex = component.publicBindingIndex;

        if(action.bindings[bindingIndex].isComposite)
        {
            UpdateCompositeIcons(component, action, bindingIndex);
            return;
        }

        if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
            return;
                
        Sprite icon = GetIcon(deviceLayoutName, controlPath);

        if (icon != null)
            component.m_ControlImage.sprite = icon;
    }

    Sprite GetIcon(string deviceLayoutName, string controlPath)
    {
        if(InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
            return playStation.GetSprite(controlPath);
        else if(InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
            return xbox.GetSprite(controlPath);
        else if(InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard") || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Mouse"))
            return keyboardMouse.GetSprite(controlPath);

        return null;
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

            for(int i = 0; i < controllerSlidersFills.Length; i++)
                controllerSlidersFills[i].sprite = sliderFillSprites[0];

            for(int i = 0; i < toggleSelectedImages.Length; i++)
                toggleSelectedImages[i].sprite = toggleSelectedSprites[0];

            for(int i = 0; i < sliderBackgroundImages.Length; i++)
                sliderBackgroundImages[i].sprite = sliderBackgroundSprites[0];

            aimSensitivitySlider.interactable = true;
            cursorSensitivitySlider.interactable = true;

            controllerVibrationToggle.interactable = true;
            swapJoysticksToggle.interactable = true;
            useDPadToggle.interactable = true;
        }   
        else
        {
            inputSettingsScrollRect.inertia = true;

            for(int i = 0; i < keyboardButtons.Length; i++)
                keyboardButtons[i].interactable = true;

            for(int i = 0; i < controllerButtons.Length; i++)
                controllerButtons[i].interactable = false;

            for(int i = 0; i < controllerSlidersFills.Length; i++)
                controllerSlidersFills[i].sprite = sliderFillSprites[1];

            for(int i = 0; i < toggleSelectedImages.Length; i++)
                toggleSelectedImages[i].sprite = toggleSelectedSprites[1];

            for(int i = 0; i < sliderBackgroundImages.Length; i++)
                sliderBackgroundImages[i].sprite = sliderBackgroundSprites[1];
            
            aimSensitivitySlider.interactable = false;
            cursorSensitivitySlider.interactable = false;

            controllerVibrationToggle.interactable = false;
            swapJoysticksToggle.interactable = false;
            useDPadToggle.interactable = false;
        }
    }

    public void OnKeyboardOnlyInputChanged(bool keyboardOnly)
    {
        if(!gameInputManager.ControllerConnected())
        {
            if(keyboardOnly)
                inputSettingsScrollRect.inertia = false;
            else
                inputSettingsScrollRect.inertia = true;
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
        currentResolution = screenResolutions[gameManager.screenResolution];
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
        screenResolutionText.text = screenResolutions[(int)screenResolutionSlider.value].x.ToString() + " x " + screenResolutions[(int)screenResolutionSlider.value].y.ToString();
    }

    public void UpdateDPadText(bool joysticksSwapped)
    {
        dPadText.text = joysticksSwapped ? "Use D-Pad for Aim / Cursor" : "Use D-Pad for Move / Cursor";
    }

    public void RestoreDefaultControllerSettings()
    {
        aimSensitivitySlider.value = 0.5f;
        cursorSensitivitySlider.value = 0.5f;
        controllerVibrationToggle.isOn = true;
        swapJoysticksToggle.isOn = false;
        useDPadToggle.isOn = false;
        lockAimingToggle.isOn = false;

        gameManager.controllerVibration = true;
        gameManager.swapJoysticks = false;
        gameManager.useDPad = false;
        gameManager.lockAiming = false;

        for(int i = 0; i < inputRebindingButtons.Length; i++)
            inputRebindingButtons[i].ResetToDefault();

        UpdateDPadText(gameManager.swapJoysticks);
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

        if(mainMenuManager != null)
        {    
            gameManager.inputBindings = gameInputManager.inputActions.SaveBindingOverridesAsJson();
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
        uIScreenManager.transitionTimer = 4;
        gameManager.ResetGame();
        StartCoroutine(ResetGameCo());
    }

    IEnumerator ResetGameCo()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main Menu");
    }

    [System.Serializable]
    public struct ControllerIcons
    {
        public Sprite buttonSouth;
        public Sprite buttonNorth;
        public Sprite buttonEast;
        public Sprite buttonWest;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite rightStick;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetSprite(string controlPath)
        {
            switch (controlPath)
            {
                case "buttonSouth": return buttonSouth;
                case "buttonNorth": return buttonNorth;
                case "buttonEast": return buttonEast;
                case "buttonWest": return buttonWest;
                case "start": return startButton;
                case "select": return selectButton;
                case "leftTrigger": return leftTrigger;
                case "rightTrigger": return rightTrigger;
                case "leftShoulder": return leftShoulder;
                case "rightShoulder": return rightShoulder;
                case "dpad": return dpad;
                case "dpad/up": return dpadUp;
                case "dpad/down": return dpadDown;
                case "dpad/left": return dpadLeft;
                case "dpad/right": return dpadRight;
                case "leftStick": return leftStick;
                case "rightStick": return rightStick;
                case "leftStickPress": return leftStickPress;
                case "rightStickPress": return rightStickPress;
            }
            return null;
        }
    }
}



[System.Serializable]
public struct KeyboardMouseIcons
{
    public Sprite mouseLeftButton;
    public Sprite mouseRightButton;
    public Sprite mouseMiddleButton;
    public Sprite a;
    public Sprite b;
    public Sprite c;
    public Sprite d;
    public Sprite e;
    public Sprite f;
    public Sprite g;
    public Sprite h;
    public Sprite i;
    public Sprite j;
    public Sprite k;
    public Sprite l;
    public Sprite m;
    public Sprite n;
    public Sprite o;
    public Sprite p;
    public Sprite q;
    public Sprite r;
    public Sprite s;
    public Sprite t;
    public Sprite u;
    public Sprite v;
    public Sprite w;
    public Sprite x;
    public Sprite y;
    public Sprite z;

    public Sprite one;
    public Sprite two;
    public Sprite three;
    public Sprite four;
    public Sprite five;
    public Sprite six;
    public Sprite seven;
    public Sprite eight;
    public Sprite nine;
    public Sprite zero;

    public Sprite shift;
    public Sprite ctrl;
    public Sprite command;
    public Sprite alt;

    public Sprite upArrow;
    public Sprite downArrow;
    public Sprite leftArrow;
    public Sprite rightArrow;
    public Sprite enter;
    public Sprite backspace;
    public Sprite space;
    public Sprite tab;
    public Sprite slash;
    public Sprite period;
    public Sprite comma;
    public Sprite quote;
    public Sprite semicolon;
    public Sprite leftBracket;
    public Sprite rightBracket;
    public Sprite backslash;

    public Sprite fOne;
    public Sprite fTwo;
    public Sprite fThree;
    public Sprite fFour;
    public Sprite fFive;
    public Sprite fSix;
    public Sprite fSeven;
    public Sprite fEight;
    public Sprite fNine;
    public Sprite fTen;
    public Sprite fEleven;
    public Sprite fTwelve;
    
    public Sprite GetSprite(string controlPath)
    {
        switch (controlPath)
        {
            case "leftButton": return mouseLeftButton;
            case "rightButton": return mouseRightButton;
            case "middleButton": return mouseMiddleButton;

            case "a": return a;
            case "b": return b;
            case "c": return c;
            case "d": return d;
            case "e": return e;
            case "f": return f;
            case "g": return g;
            case "h": return h;
            case "i": return i;
            case "j": return j;
            case "k": return k;
            case "l": return l;
            case "m": return m;
            case "n": return n;
            case "o": return o;
            case "p": return p;
            case "q": return q;
            case "r": return r;
            case "s": return s;
            case "t": return t;
            case "u": return u;
            case "v": return v;
            case "w": return w;
            case "x": return x;
            case "y": return y;
            case "z": return z;

            case "1": return one;
            case "2": return two;
            case "3": return three;
            case "4": return four;
            case "5": return five;
            case "6": return six;
            case "7": return seven;
            case "8": return eight;
            case "9": return nine;
            case "0": return zero;

            case "numpad1": return one;
            case "numpad2": return two;
            case "numpad3": return three;
            case "numpad4": return four;
            case "numpad5": return five;
            case "numpad6": return six;
            case "numpad7": return seven;
            case "numpad8": return eight;
            case "numpad9": return nine;
            case "numpad0": return zero;

            case "leftShift": return shift;
            case "rightShift": return shift;

            case "leftCtrl": return ctrl;
            case "rightCtrl": return ctrl;

            case "leftCommand": return command;
            case "rightCommand": return command;

            case "leftAlt": return alt;
            case "rightAlt": return alt;

            case "upArrow": return upArrow;
            case "downArrow": return downArrow;
            case "leftArrow": return leftArrow;
            case "rightArrow": return rightArrow;

            case "enter": return enter;
            case "numpadEnter": return enter;
            case "backspace": return backspace;
            case "space": return space;
            case "tab": return tab;
            case "slash": return slash;
            case "period": return period;
            case "comma": return comma;
            case "quote": return quote;
            case "semicolon": return semicolon;
            case "leftBracket": return leftBracket;
            case "rightBracket": return rightBracket;
            case "backslash": return backslash;

            case "f1": return fOne;
            case "f2": return fTwo;
            case "f3": return fThree;
            case "f4": return fFour;
            case "f5": return fFive;
            case "f6": return fSix;
            case "f7": return fSeven;
            case "f8": return fEight;
            case "f9": return fNine;
            case "f10": return fTen;
            case "f11": return fEleven;
            case "f12": return fTwelve;
        }
        return null;
    }
}