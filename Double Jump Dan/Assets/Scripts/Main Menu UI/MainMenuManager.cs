using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public Button playButton;
    public Button shopButton;
    public Button settingsButton;
    public Button userButton;
    public Button tutorialButton;
    public Button statsButton;

    [Header("Initialization")]
    [SerializeField] GameObject shopGameObject;
    [SerializeField] GameObject userMenuGameObject;
    [SerializeField] GameObject screenshotsMenuGameObject;


    [Header("Tampered User File")]
    [SerializeField] Image tamperedUserFileImage;
    [SerializeField] Text tamperedUserFileText;

    [Header("Joystick")]
    public RectTransform cursor;
    [SerializeField] Camera _camera;
    public float scrollSpeed;
    [SerializeField] Sprite[] cursorSprites;

    ShopManager shopManager;
    UserMenu userMenu;
    UserStatsMenu userStatsMenu;
    LevelSelectMenu levelSelectMenu;
    SettingsManager settingsManager;
    ScreenshotsMenu screenshotsMenu;
    Canvas canvas;
    Vector2 position;
    Vector2 screenPosition;
    Vector2 input;
    float startDelay = 1.35f;
    GameInputManager gameInputManager;
    float _cursorSpeed;
    public float _scrollSpeed { get; set; }
    public float cursorSpeed { get; set; }
    Image cursorImage;
    [HideInInspector]
    public List<GameObject> initialUserButtons = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> shopItems = new List<GameObject>();
    [HideInInspector]
    public int screenshotsCount { get; set; }
    int initialUserCount;
    int shopItemCount;
    bool initialized;

    void Start()
    {
        Time.timeScale = 1;
        shopManager = GetComponent<ShopManager>(); 
        userMenu = GetComponent<UserMenu>();
        userStatsMenu = GetComponent<UserStatsMenu>();
        levelSelectMenu = GetComponent<LevelSelectMenu>();
        settingsManager = GetComponent<SettingsManager>();
        screenshotsMenu = GetComponent<ScreenshotsMenu>();

        shopGameObject.SetActive(true);
        userMenuGameObject.SetActive(true);
        screenshotsMenuGameObject.SetActive(true);

        gameInputManager = GameInputManager.Instance;

        initialUserCount = GameManager.Instance.users.Count;

        shopItemCount += shopManager.itemManager.guns.Count;
        shopItemCount += shopManager.itemManager.hats.Count;
        shopItemCount += shopManager.itemManager.skins.Count;

        if(playButton == null || shopButton == null || userButton == null || statsButton == null || settingsButton == null)
            Debug.LogError("Main Menu button is null");
            
        playButton.onClick.AddListener(levelSelectMenu.RefreshLevels);
        shopButton.onClick.AddListener(shopManager.RefreshShop);
        shopButton.onClick.AddListener(shopManager.RefreshShopScrollRects);
        userButton.onClick.AddListener(userMenu.RefreshUserByteSizes);
        statsButton.onClick.AddListener(userStatsMenu.RefreshUserStats);
        settingsButton.onClick.AddListener(settingsManager.RefreshFullscreenToggle);
        gameInputManager.OnControllerChanged += OnControllerChanged;

        canvas = GetComponent<Canvas>();
        cursorImage = cursor.gameObject.GetComponent<Image>();

        position = new Vector2(Screen.width / 2, Screen.height / 2);
        
        InputState.Change(Mouse.current.position, position);
        InputState.Change(Mouse.current.delta, Vector2.zero);

        if(gameInputManager.ControllerConnected())
        {    
            Cursor.visible = false;
            cursor.gameObject.SetActive(true);
        }
    }
    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
        {
            Cursor.visible = false;
            cursor.gameObject.SetActive(true);
        }
        else
        {
            Cursor.visible = true;
            cursor.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if(!initialized)
        {
            if(initialUserButtons.Count == initialUserCount && shopItems.Count == shopItemCount && screenshotsMenu.screenshotsCount == screenshotsCount)
            {
                initialized = true;

                shopGameObject.SetActive(false);
                userMenuGameObject.SetActive(false);

                Canvas.ForceUpdateCanvases();
                screenshotsMenu.Initialize();
                screenshotsMenuGameObject.SetActive(false);
            }

            return;
        }
        
        if(gameInputManager.ControllerConnected())
        {
            input = gameInputManager.GetJoystickMovement();

            if(gameInputManager.LeftTrigger())
                _cursorSpeed = cursorSpeed * 2f;
            else
                _cursorSpeed = cursorSpeed;

            position += input * _cursorSpeed * Time.unscaledDeltaTime;
            position.x = Mathf.Clamp(position.x, 0, Screen.width);
            position.y = Mathf.Clamp(position.y, 0, Screen.height);

            InputState.Change(Mouse.current.position, position);
            InputState.Change(Mouse.current.delta, Vector2.zero);

            screenPosition = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector2 localPosition);
            cursor.anchoredPosition = localPosition;

            if(gameInputManager.CursorPress())
                cursorImage.sprite = cursorSprites[1];
            else
                cursorImage.sprite = cursorSprites[0];
        }

        if(startDelay > 0)
        {
            tutorialButton.interactable = false;
            startDelay -= Time.deltaTime;
        }
        else
        {
            tutorialButton.interactable = true;
        }
    }

    public void LoadTutorial()
    {
        if(startDelay > 0)
            return;
            
        LevelLoadingManager.Instance.LoadScene("Tutorial");
    }

    #region TamperedUserFile
    public void TamperedUserFile(string text)
    {
        StartCoroutine(TamperedUserFileCo(text));
        StartCoroutine(ForceTamperedUserImageForward());
    }

    IEnumerator ForceTamperedUserImageForward()
    {
        float duration = 0.25f;

        while(duration > 0)
        {
            duration -= Time.deltaTime;
            tamperedUserFileImage.transform.SetAsLastSibling();
            yield return null;
        }
    }

    IEnumerator TamperedUserFileCo(string text)
    {
        tamperedUserFileImage.gameObject.SetActive(true);
        tamperedUserFileText.text = text;

        tamperedUserFileImage.color = new Color(tamperedUserFileImage.color.r, tamperedUserFileImage.color.g, tamperedUserFileImage.color.b, 1);
        tamperedUserFileText.color = new Color(tamperedUserFileText.color.r, tamperedUserFileText.color.g, tamperedUserFileText.color.b, 1);

        yield return new WaitForSeconds(3);

        float inTime = 0;
        float duration = 2;

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            tamperedUserFileImage.color = new Color(tamperedUserFileImage.color.r, tamperedUserFileImage.color.g, tamperedUserFileImage.color.b, Mathf.Lerp(1, 0, inTime / duration));
            tamperedUserFileText.color = new Color(tamperedUserFileText.color.r, tamperedUserFileText.color.g, tamperedUserFileText.color.b, Mathf.Lerp(1, 0, inTime / duration));
            yield return null;
        } 

        tamperedUserFileImage.gameObject.SetActive(false);
        tamperedUserFileImage.color = new Color(tamperedUserFileImage.color.r, tamperedUserFileImage.color.g, tamperedUserFileImage.color.b, 0);
        tamperedUserFileText.color = new Color(tamperedUserFileText.color.r, tamperedUserFileText.color.g, tamperedUserFileText.color.b, 0);
    }

    #endregion
    
    public void ExitGame()
    {
        Application.Quit();
    }
}