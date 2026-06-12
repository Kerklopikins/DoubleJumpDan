using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class DevControls : MonoBehaviour
{   
    [SerializeField] LevelType levelType;
    [SerializeField] bool enableTestingMode;
    
    [SerializeField] GameObject devModeUI;
    [SerializeField] Text messageText;
    [SerializeField] InputField textField;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    Dictionary<string, Action> keywordActions = new Dictionary<string, Action>();
    KeywordRecognizer keywordRecognizer;
#endif

    WorldManager worldManager;
    List<Parallax> parallaxes = new List<Parallax>();
    bool enteredDevMode;
    ItemManager itemManager;
    Player player;
    bool searched;
    GameHUD gameHUD;
    Screenshot screenshot;
    float timeScale = 1;
    List<TimeOfDayMaterialChanger> timeMatChangers;
    List<Health> healths;
    Vector2 lastScreenSize;
    float screenSizeCheckTimer;
    public enum LevelType { Normal, MainMenu }
    bool invincible;
    bool isWindows10OrNewer;
    int numberFromVoiceCommand;

    void Start()
    {
        Version version = Environment.OSVersion.Version;
        isWindows10OrNewer = (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && version.Major >= 10;

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        LevelLoadingManager.Instance.OnLevelLoading += DisposeOfKeywordRecognizer;

        if(isWindows10OrNewer)
        {
            keywordActions.Add("invincible true", InvincibleTrue);
            keywordActions.Add("invincible false", InvincibleFalse);
            keywordActions.Add("heal", Heal);
            keywordActions.Add("game over", GameOver);
            keywordActions.Add("kill all", KillAll);
            keywordActions.Add("daytime", ToggleDay);
            keywordActions.Add("sunset", ToggleSunset);
            keywordActions.Add("night", ToggleNight);
            keywordActions.Add("sunrise", ToggleSunrise);
            keywordActions.Add("overcast", ToggleOvercast);

            keywordActions.Add("dust on", DustOn);
            keywordActions.Add("dust off", DustOff);

            keywordActions.Add("snow on", SnowOn);
            keywordActions.Add("snow off", SnowOff);

            keywordActions.Add("clouds on", CloudsOn);
            keywordActions.Add("clouds off", CloudsOff);

            keywordActions.Add("change gun", ChangeGun);
            keywordActions.Add("change hat", ChangeHat);
            keywordActions.Add("change skin", ChangeSkin);

            keywordActions.Add("load scene zero", LoadScene);
            keywordActions.Add("load scene one", LoadScene);
            keywordActions.Add("load scene two", LoadScene);
            keywordActions.Add("load scene three", LoadScene);
            keywordActions.Add("load scene four", LoadScene);
            keywordActions.Add("load scene five", LoadScene);
            keywordActions.Add("load scene six", LoadScene);
            keywordActions.Add("load scene seven", LoadScene);
            keywordActions.Add("load scene eight", LoadScene);
            keywordActions.Add("load scene nine", LoadScene);
            keywordActions.Add("load scene ten", LoadScene);

            keywordRecognizer = new KeywordRecognizer(keywordActions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;   
        }
    #endif

        if(levelType == LevelType.Normal)
        {
            if(enableTestingMode)
            {
                worldManager = WorldManager.Instance;
                Transform backgroundParent = GameObject.Find("Level Objects").transform.Find("Background");

                for(int i = 0; i < backgroundParent.childCount; i++)
                    parallaxes.Add(backgroundParent.GetChild(i).GetComponent<Parallax>());
            }
        }
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        if(isWindows10OrNewer)
        {                
            if(args.text.Contains("zero"))
                numberFromVoiceCommand = 0;
            else if(args.text.Contains("one"))
                numberFromVoiceCommand = 1;
            else if(args.text.Contains("two"))
                numberFromVoiceCommand = 2;
            else if(args.text.Contains("three"))
                numberFromVoiceCommand = 3;
            else if(args.text.Contains("four"))
                numberFromVoiceCommand = 4;
            else if(args.text.Contains("five"))
                numberFromVoiceCommand = 5;
            else if(args.text.Contains("six"))
                numberFromVoiceCommand = 6;
            else if(args.text.Contains("seven"))
                numberFromVoiceCommand = 7;
            else if(args.text.Contains("eight"))
                numberFromVoiceCommand = 8;
            else if(args.text.Contains("nine"))
                numberFromVoiceCommand = 9;
            else if(args.text.Contains("ten"))
                numberFromVoiceCommand = 10;
            else
                numberFromVoiceCommand = -1;

            keywordActions[args.text].Invoke();

            textField.text = "";
            StartCoroutine(ClearMessageText());
        }
    }
#endif

    void Update()
    {   
        if(!enableTestingMode)
            return;

#if UNITY_EDITOR
        if(levelType == LevelType.Normal)
        {
            if(screenSizeCheckTimer > 0)
                screenSizeCheckTimer -= Time.deltaTime;
            else
            {
                Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

                if(currentScreenSize != lastScreenSize)
                {
                    lastScreenSize = currentScreenSize;
                    ResizeBGAndPosUI();
                }

                screenSizeCheckTimer = 0.5f;
            }  
        } 
#endif
        if(Keyboard.current.leftShiftKey.IsPressed() && Keyboard.current.qKey.wasPressedThisFrame)
        {   
            if(levelType == LevelType.Normal)
            {
                if(!searched)
                {
                    itemManager = GameObject.FindWithTag("Managers").GetComponent<ItemManager>();
                    player = GameObject.FindWithTag("Player").GetComponent<Player>();
                    gameHUD = GameObject.Find("Game HUD").GetComponent<GameHUD>();
                    screenshot = GetComponentInChildren<Screenshot>();

                    timeMatChangers = new List<TimeOfDayMaterialChanger>(FindObjectsByType<TimeOfDayMaterialChanger>(FindObjectsInactive.Include, FindObjectsSortMode.None));

                    searched = true;
                }
            }
            
            enteredDevMode = !enteredDevMode;
            devModeUI.SetActive(enteredDevMode);

            if(levelType == LevelType.Normal)
            {
                if(enteredDevMode)
                {
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    if(isWindows10OrNewer)
                        keywordRecognizer.Start();
                #endif
                    textField.Select();
                    gameHUD.enabled = false;
                    screenshot.enabled = false;
                    player.enabled = false;
                }
                else
                {
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    if(isWindows10OrNewer)
                        keywordRecognizer.Stop();
                #endif
                    gameHUD.enabled = true;
                    screenshot.enabled = true;
                    player.enabled = true;
                }
                
                messageText.text = "Entered Dev Mode";
                StartCoroutine(ClearMessageText());   
            }
        }
        
        if(levelType == LevelType.MainMenu)
            return;

        if(enteredDevMode)
        {
            if(textField.text.Length > 0 && GameInputManager.Instance.ReturnButtonDown())
            {
                string commandString = textField.text;
                float value;

                if(commandString.Any(char.IsDigit))
                    value = float.Parse(System.Text.RegularExpressions.Regex.Match(commandString, @"[\d]*\.?[\d]+").Value);
                else
                    value = 0;

                if(commandString.ToLower().Contains("timescale"))
                {
                    value = Mathf.Clamp(value, 0, 3);
                    timeScale = value;
                    Time.timeScale = timeScale;
                    messageText.text = "Time Scale set to " + timeScale;
                }
                else if(!commandString.ToLower().Contains("sprint") && commandString.ToLower().Contains("speed"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.walkSpeed = value;
                    messageText.text = "Player walk speed set to: " + value;
                }
                else if(commandString.ToLower().Contains("sprintspeed"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.sprintSpeed = value;
                    messageText.text = "Player sprint speed set to: " + value;
                }
                else if(commandString.ToLower().Contains("accelair"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.accelerationTimeInAir = value;
                    messageText.text = "Player acceleration in air set to: " + value;
                }
                else if(commandString.ToLower().Contains("accelgrnd"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.accelerationTimeGrounded = value;
                    messageText.text = "Player acceleration on ground set to: " + value;
                }
                else if(commandString.ToLower().Contains("jmphght"))
                {
                    value = Mathf.Clamp(value, 0, 100);
                    player.jumpHeight = value;
                    messageText.text = "Player jump height set to: " + value;
                }
                else if(commandString.ToLower().Contains("gravity"))
                {
                    value = Mathf.Clamp(value, 0, 10);
                    player.GetComponent<Rigidbody2D>().gravityScale = value;
                    messageText.text = "Player gravity set to: " + value;
                }
                else if(commandString.ToLower().Contains("invincible"))
                {
                    if(commandString.ToLower().Contains("true"))
                        invincible = true;
                    else if(commandString.ToLower().Contains("false"))
                        invincible = false;
                    else
                        invincible = false;

                    player.invincible = invincible;

                    if(invincible)
                        messageText.text = "Player invincible is set to true";
                    else
                        messageText.text = "Player invincible is set to false";
                }
                else if(commandString.ToLower().Contains("loadscene"))
                {
                    value = Mathf.Clamp(value, 0, SceneManager.sceneCountInBuildSettings - 1);
                    int roundedValue = Mathf.RoundToInt(value);
                    LevelLoadingManager.Instance.LoadScene(roundedValue);

                    messageText.text = "Loading scene";
                }
                else if(commandString.ToLower().Contains("heal"))
                {
                    Heal();
                }
                else if(commandString.ToLower().Contains("gameover"))
                {
                    GameOver();
                }
                else if(commandString.ToLower().Contains("killall"))
                {
                    KillAll();
                }
                else if(commandString.ToLower().Contains("daytime"))
                {
                    ToggleDay();
                }
                else if(commandString.ToLower().Contains("sunset"))
                {
                    ToggleSunset();
                }
                else if(commandString.ToLower().Contains("night"))
                {
                    ToggleNight();
                }
                else if(commandString.ToLower().Contains("sunrise"))
                {
                    ToggleSunrise();
                }
                else if(commandString.ToLower().Contains("overcast"))
                {
                    ToggleOvercast();
                }
                else if(commandString.ToLower().Contains("dust"))
                {
                    bool on;

                    if(commandString.ToLower().Contains("on"))
                        on = true;
                    else if(commandString.ToLower().Contains("off"))
                        on = false;
                    else
                        on = false;

                    ToggleDust(on);

                    if(on)
                        messageText.text = "Dust enabled";
                    else
                        messageText.text = "Dust disabled";
                }
                else if(commandString.ToLower().Contains("snow"))
                {
                    bool on;

                    if(commandString.ToLower().Contains("on"))
                        on = true;
                    else if(commandString.ToLower().Contains("off"))
                        on = false;
                    else
                        on = false;

                    ToggleSnow(on);

                    if(on)
                        messageText.text = "Snow enabled";
                    else
                        messageText.text = "Snow disabled";
                }
                else if(commandString.ToLower().Contains("clouds"))
                {
                    bool on;

                    if(commandString.ToLower().Contains("on"))
                        on = true;
                    else if(commandString.ToLower().Contains("off"))
                        on = false;
                    else
                        on = false;

                    ToggleClouds(on);

                    if(on)
                        messageText.text = "Clouds enabled";
                    else
                        messageText.text = "Clouds disabled";
                }
                else if(commandString.ToLower().Contains("resize"))
                {
                    ResizeBGAndPosUI();
                }
                else if(commandString.ToLower().Contains("changegun"))
                {
                    ChangeGun();
                }
                else if(commandString.ToLower().Contains("changehat"))
                {
                    ChangeHat();
                }
                else if(commandString.ToLower().Contains("changeskin"))
                {
                    ChangeSkin();
                }
                else
                {
                    messageText.text = "Invalid command";
                }

                StartCoroutine(ClearMessageText());
                textField.text = "";
            }
        }
    }

    #region Voice Commands

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void DisposeOfKeywordRecognizer()
    {
        if(isWindows10OrNewer)
        {
            if(keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();

            if(keywordRecognizer != null)
                keywordRecognizer.Dispose();
        }
    }
    #endif

    void LoadScene()
    {
        LevelLoadingManager.Instance.LoadScene(numberFromVoiceCommand);
    }

    void Heal()
    {
         //The Golden Heart
        player.lives = GameManager.Instance.currentUser.equippedUpgrades.Contains(5480) ? 4 : 3;
        player.GiveHealth(player.health);
        StatsHUD.Instance.PlayerKilled();
        messageText.text = "Max lives and health";
    }

    void GameOver()
    {
        player.lives = 1;
        player.Kill();
        messageText.text = "Now that wasn't very nice";
    }

    void KillAll()
    {
        healths = new List<Health>(FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

        if(healths.Count > 0)
            foreach(var health in healths)
                if(health.gameObject.activeInHierarchy)
                    health.Kill();
        
        healths.Clear();
        messageText.text = "All enemies killed";
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void InvincibleTrue()
    {
        invincible = true;
        messageText.text = "Player invincible is set to true";

        player.invincible = invincible;
    }

    void InvincibleFalse()
    {
        invincible = false;
        messageText.text = "Player invincible is set to false";

        player.invincible = invincible;
    }

    void DustOn()
    {
        ToggleDust(true);
        messageText.text = "Dust enabled";
    }

    void DustOff()
    {
        ToggleDust(false);
        messageText.text = "Dust disabled";
    }

    void SnowOn()
    {
        ToggleSnow(true);
        messageText.text = "Snow enabled";
    }

    void SnowOff()
    {
        ToggleSnow(false);
        messageText.text = "Snow disabled";
    }

    void CloudsOn()
    {
        ToggleClouds(true);
        messageText.text = "Clouds enabled";
    }

    void CloudsOff()
    {
        ToggleClouds(false);
        messageText.text = "Clouds disabled";
    }
#endif
    #endregion

    IEnumerator ClearMessageText()
    {
        yield return new WaitForSecondsRealtime(3);
        messageText.text = "";
    }

    public void ResizeBGAndPosUI()
    {
        messageText.text = "Background, Screen Effects, and UI resized and refreshed";
        worldManager.UpdatePositionAndScale();
        LevelLoadingManager.Instance.ResizeFadeBackground();
        StatsHUD.Instance.PositionUI();
        ScreenEffectsManager.Instance.ResizeScreenEffects();

        foreach(var parallax in parallaxes)
            parallax.OffsetBackground();    
    }

    public void ChangeGun()
    {
        GameObject oldGun = GameObject.Find("Arm 2").GetComponentInChildren<GunInfo>().gameObject;
        GameObject.Find("Arm 2").transform.localEulerAngles = new Vector3(0, 0, 90);
        oldGun.SetActive(false);
        
        Item randomItem = itemManager.guns[UnityEngine.Random.Range(0, itemManager.guns.Count)];
        itemManager.InstantiateGun(randomItem);
        messageText.text = "Changed gun to " + randomItem.gameObject.name;
        StartCoroutine(ClearMessageText());
    }

    public void ChangeHat()
    {
        Item[] items = player.GetComponentsInChildren<Item>();

        foreach(Item item in items)
            if(item.itemType == Item.ItemType.Hat)
                item.gameObject.SetActive(false);
            
        Item randomItem = itemManager.hats[UnityEngine.Random.Range(0, itemManager.hats.Count)];
        itemManager.InstantiateHat(randomItem);
        messageText.text = "Changed hat to " + randomItem.gameObject.name;
        StartCoroutine(ClearMessageText());
    }

    public void ChangeSkin()
    {
        Item randomItem = itemManager.skins[UnityEngine.Random.Range(0, itemManager.skins.Count)];
        
        ////Shadow dan
        if(randomItem.itemID != 1757)
        {
            player.GetComponent<ShadowDanTracer>().enabled = false;
        }

        itemManager.ChangeSkin(randomItem);
        messageText.text = "Changed skin to " + randomItem.gameObject.name;
        StartCoroutine(ClearMessageText());
    }

    public void ToggleDay()
    {
        messageText.text = "Time of day set to day";

        worldManager.SetDay();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeInHierarchy)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }

        ParticleSystemRenderer cloudParticleSystemRenderer = worldManager.clouds.GetComponent<ParticleSystemRenderer>();
        cloudParticleSystemRenderer.material.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, cloudParticleSystemRenderer.material.color.a);
    }

    public void ToggleSunset()
    {
        messageText.text = "Time of day set to sunset";

        worldManager.SetSunset();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeInHierarchy)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }

        ParticleSystemRenderer cloudParticleSystemRenderer = worldManager.clouds.GetComponent<ParticleSystemRenderer>();
        cloudParticleSystemRenderer.material.color = new Color(worldManager.sunsetCloudTint.r, worldManager.sunsetCloudTint.g, worldManager.sunsetCloudTint.b, cloudParticleSystemRenderer.material.color.a);
    }

    public void ToggleNight()
    {
        messageText.text = "Time of day set to night";

        worldManager.SetNight();
        worldManager.stars.gameObject.SetActive(true);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeInHierarchy)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }

        ParticleSystemRenderer cloudParticleSystemRenderer = worldManager.clouds.GetComponent<ParticleSystemRenderer>();
        cloudParticleSystemRenderer.material.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, cloudParticleSystemRenderer.material.color.a);
    }

    public void ToggleSunrise()
    {
        messageText.text = "Time of day set to sunrise";
        
        worldManager.SetSunrise();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeInHierarchy)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }
        
        ParticleSystemRenderer cloudParticleSystemRenderer = worldManager.clouds.GetComponent<ParticleSystemRenderer>();
        cloudParticleSystemRenderer.material.color = new Color(worldManager.sunriseCloudTint.r, worldManager.sunriseCloudTint.g, worldManager.sunriseCloudTint.b, cloudParticleSystemRenderer.material.color.a);
    }

    public void ToggleOvercast()
    {
        messageText.text = "Sky set to overcast";

        worldManager.SetOvercast();

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeInHierarchy)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }

        ParticleSystemRenderer cloudParticleSystemRenderer = worldManager.clouds.GetComponent<ParticleSystemRenderer>();
        cloudParticleSystemRenderer.material.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, cloudParticleSystemRenderer.material.color.a);

        StartCoroutine(ClearMessageText());
    }
    
    public void ToggleDust(bool on)
    {
        Camera _camera = Camera.main;
        float cameraSize = _camera.orthographicSize * ((float)Screen.width / Screen.height);
        worldManager.dust.transform.localScale = new Vector2(cameraSize * 3, _camera.orthographicSize * 3);
        worldManager.dust.gameObject.SetActive(on);
    }

    public void ToggleSnow(bool on)
    {            
        worldManager.snow.gameObject.SetActive(on);
        worldManager.clouds.gameObject.SetActive(!on);

        Camera _camera = Camera.main;
        float cameraSize = _camera.orthographicSize * ((float)Screen.width / Screen.height);
        worldManager.snow.GetChild(0).transform.localScale = new Vector2(cameraSize * 3, _camera.orthographicSize * 3);

        worldManager.snow.GetChild(1).GetComponent<ParticleSystem>().Stop();
        worldManager.snow.GetChild(1).transform.localPosition = new Vector2(-cameraSize, _camera.orthographicSize);
        worldManager.snow.GetChild(1).GetComponent<ParticleSystem>().Play();
    }

    public void ToggleClouds(bool on)
    {
        worldManager.clouds.gameObject.SetActive(on);
    }
}