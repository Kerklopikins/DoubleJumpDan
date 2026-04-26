using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DevControls : MonoBehaviour
{
    [SerializeField] bool enableTestingMode;
    
    [SerializeField] GameObject devModeUI;
    [SerializeField] Text messageText;
    [SerializeField] InputField textField;

    WorldManager worldManager;
    List<Parallax> parallaxes = new List<Parallax>();
    bool enteredDevMode;
    ItemManager itemManager;
    Player player;
    bool searched;
    GameHUD gameHUD;
    Screenshot screenshot;
    bool checkedFocus;
    float timeScale = 1;
    List<TimeOfDayMaterialChanger> timeMatChangers;
    List<Health> healths;
    Vector2 lastScreenSize;
    float screenSizeCheckTimer;

    void Start()
    {
        if(enableTestingMode)
        {
            worldManager = WorldManager.Instance;
            Transform backgroundParent = GameObject.Find("Level Objects").transform.Find("Background");

            for(int i = 0; i < backgroundParent.childCount; i++)
                parallaxes.Add(backgroundParent.GetChild(i).GetComponent<Parallax>());
        }
    }

    void Update()
    {   
        if(!enableTestingMode)
            return;

#if UNITY_EDITOR

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
#endif
        if(GameInputManager.Instance.DevModeButtonDown())
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
            
            enteredDevMode = !enteredDevMode;
            devModeUI.SetActive(enteredDevMode);
            messageText.text = "Entered Dev Mode";
            StartCoroutine(ClearMessageText());
        }
        
        if(enteredDevMode)
        {
            if(textField.isFocused)
            {
                gameHUD.enabled = false;
                screenshot.enabled = false;
                player.enabled = false;
                Time.timeScale = 0;
                checkedFocus = false;
            }
            else
            {
                if(!checkedFocus)
                {
                    gameHUD.enabled = true;
                    screenshot.enabled = true;
                    player.enabled = true;
                    Time.timeScale = timeScale;
                    checkedFocus = true;
                }
            }

            if(textField.text.Length > 0 && GameInputManager.Instance.ReturnButtonDown())
            {
                string commandString = textField.text;
                float value;

                if(commandString.Any(char.IsDigit))
                    value = float.Parse(System.Text.RegularExpressions.Regex.Match(commandString, @"[\d]*\.?[\d]+").Value);
                else
                    value = 0;

                if(commandString.Contains("/timescale"))
                {
                    value = Mathf.Clamp(value, 0, 3);
                    timeScale = value;
                    Time.timeScale = timeScale;
                    messageText.text = "Time Scale set to " + timeScale;
                }
                else if(commandString.Contains("/speed"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.speed = value;
                    messageText.text = "Player speed set to: " + value;
                }
                else if(commandString.Contains("/accel air"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.accelerationTimeInAir = value;
                    messageText.text = "Player acceleration in air set to: " + value;
                }
                else if(commandString.Contains("/accel grnd"))
                {
                    value = Mathf.Clamp(value, 0, 60);
                    player.accelerationTimeGrounded = value;
                    messageText.text = "Player acceleration on ground set to: " + value;
                }
                else if(commandString.Contains("/jmp hght"))
                {
                    value = Mathf.Clamp(value, 0, 100);
                    player.jumpHeight = value;
                    messageText.text = "Player jump height set to: " + value;
                }
                else if(commandString.Contains("/gravity"))
                {
                    value = Mathf.Clamp(value, 0, 10);
                    player.GetComponent<Rigidbody2D>().gravityScale = value;
                    messageText.text = "Player gravity set to: " + value;
                }
                else if(commandString.Contains("/invincible"))
                {
                    bool invincible;

                    if(commandString.Contains("true"))
                        invincible = true;
                    else if(commandString.Contains("false"))
                        invincible = false;
                    else
                        invincible = false;

                    player.invincible = invincible;

                    if(invincible)
                        messageText.text = "Player invincible is set to true";
                    else
                        messageText.text = "Player invincible is set to false";
                }
                else if(commandString.Contains("/load scene"))
                {
                    value = Mathf.Clamp(value, 0, SceneManager.sceneCountInBuildSettings - 1);
                    int roundedValue = Mathf.RoundToInt(value);
                    LevelLoadingManager.Instance.LoadScene(roundedValue);
                    messageText.text = "Loading scene";
                }
                else if(commandString.Contains("/max lives"))
                {
                    player.lives = 3;
                    player.GiveHealth(player.health);
                    StatsHUD.Instance.PlayerKilled();
                    messageText.text = "Max lives and health";
                }
                else if(commandString.Contains("/gameover"))
                {
                    player.lives = 1;
                    player.Kill();
                    messageText.text = "Now that wasn't very nice";
                }
                else if(commandString.Contains("/kill all"))
                {
                    healths = new List<Health>(FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

                    if(healths.Count > 0)
                        foreach(var health in healths)
                            if(health.gameObject.activeInHierarchy)
                                health.Kill();
                    
                    healths.Clear();
                    messageText.text = "All enemies killed";
                }
                else if(commandString.Contains("/day"))
                {
                    ToggleDay();
                    messageText.text = "Time of day set to day";
                }
                else if(commandString.Contains("/sunset"))
                {
                    ToggleSunset();
                    messageText.text = "Time of day set to sunset";
                }
                else if(commandString.Contains("/night"))
                {
                    ToggleNight();
                    messageText.text = "Time of day set to night";
                }
                else if(commandString.Contains("/sunrise"))
                {
                    ToggleSunrise();
                    messageText.text = "Time of day set to sunrise";
                }
                else if(commandString.Contains("/overcast"))
                {
                    ToggleOvercast();
                    messageText.text = "Sky set to overcast";
                }
                else if(commandString.Contains("/dust enabled"))
                {
                    bool on;

                    if(commandString.Contains("true"))
                        on = true;
                    else if(commandString.Contains("false"))
                        on = false;
                    else
                        on = false;

                    ToggleDust(on);

                    if(on)
                        messageText.text = "Dust enabled";
                    else
                        messageText.text = "Dust disabled";
                }
                else if(commandString.Contains("/snow enabled"))
                {
                    bool on;

                    if(commandString.Contains("true"))
                        on = true;
                    else if(commandString.Contains("false"))
                        on = false;
                    else
                        on = false;

                    ToggleSnow(on);

                    if(on)
                        messageText.text = "Snow enabled";
                    else
                        messageText.text = "Snow disabled";
                }
                else if(commandString.Contains("/clouds enabled"))
                {
                    bool on;

                    if(commandString.Contains("true"))
                        on = true;
                    else if(commandString.Contains("false"))
                        on = false;
                    else
                        on = false;

                    ToggleClouds(on);

                    if(on)
                        messageText.text = "Clouds enabled";
                    else
                        messageText.text = "Clouds disabled";
                }
                else if(commandString.Contains("/resize"))
                {
                    ResizeBGAndPosUI();
                    messageText.text = "Background, Screen Effects, and UI resized and refreshed";
                }

                StartCoroutine(ClearMessageText());
                textField.text = "";
            }
        }
    }

    IEnumerator ClearMessageText()
    {
        yield return new WaitForSecondsRealtime(3);
        messageText.text = "";
    }

    public void ResizeBGAndPosUI()
    {
        worldManager.UpdatePositionAndScale();
        LevelLoadingManager.Instance.ResizeFadeBackground();
        StatsHUD.Instance.PositionUI();
        ScreenEffectsManager.Instance.ResizeScreenEffects();

        foreach(var parallax in parallaxes)
            parallax.OffsetBackground();    
    }

    public void SpawnGun()
    {
        GameObject oldGun = GameObject.Find("Arm 2").GetComponentInChildren<GunInfo>().gameObject;
        GameObject.Find("Arm 2").transform.localEulerAngles = new Vector3(0, 0, 90);
        oldGun.SetActive(false);
        
        Item randomItem = itemManager.guns[Random.Range(0, itemManager.guns.Count)];
        itemManager.InstantiateGun(randomItem);
        messageText.text = "Spawned " + randomItem.gameObject.name;
        StartCoroutine(ClearMessageText());
    }

    public void SpawnHat()
    {
        Item[] items = player.GetComponentsInChildren<Item>();

        print(items.Length);

        foreach(Item item in items)
            if(item.itemType == Item.ItemType.Hat)
                item.gameObject.SetActive(false);
            
        Item randomItem = itemManager.hats[Random.Range(0, itemManager.hats.Count)];
        itemManager.InstantiateHat(randomItem);
        messageText.text = "Spawned " + randomItem.gameObject.name;
        StartCoroutine(ClearMessageText());
    }

    public void ChangeSkin()
    {
        Item randomItem = itemManager.skins[Random.Range(0, itemManager.skins.Count)];
        
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
        worldManager.SetDay();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
            if(changer.gameObject.activeSelf)
                changer.UpdateMaterialColor();
    }

    public void ToggleSunset()
    {
        worldManager.SetSunset();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeSelf)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }
    }

    public void ToggleNight()
    {
        worldManager.SetNight();
        worldManager.stars.gameObject.SetActive(true);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeSelf)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }
    }

    public void ToggleSunrise()
    {
        worldManager.SetSunrise();
        worldManager.stars.gameObject.SetActive(false);

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeSelf)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }
    }

    public void ToggleOvercast()
    {
        worldManager.SetOvercast();

        foreach(TimeOfDayMaterialChanger changer in timeMatChangers)
        {
            if(changer != null)
            {
                if(changer.gameObject.activeSelf)
                    changer.UpdateMaterialColor();
            }
            else
            {
                timeMatChangers.Remove(changer);
            }
        }

        messageText.text = "Overcast toggled";
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