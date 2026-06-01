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
    public Button statsButton;
    public Button screenshotsButton;

    [Header("Initialization")]
    [SerializeField] Button[] mainMenuButtons;
    [SerializeField] GameObject shopGameObject;
    [SerializeField] GameObject userMenuGameObject;
    [SerializeField] GameObject screenshotsMenuGameObject;
    
    [Header("Tampered User File")]
    [SerializeField] Image tamperedUserFileImage;
    [SerializeField] Text tamperedUserFileText;

    [Header("Joystick")]
    public RectTransform cursor;
    public float scrollSpeed;
    [SerializeField] Sprite[] cursorSprites;

    Vector2 cursorVelocity;
    ShopManager shopManager;
    UserMenu userMenu;
    UserStatsMenu userStatsMenu;
    LevelSelectMenu levelSelectMenu;
    SettingsManager settingsManager;
    ScreenshotsMenu screenshotsMenu;
    GameManager gameManager;
    Canvas canvas;
    Vector2 position;
    Vector2 screenPosition;
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
    bool startedDelay;
    bool endedDelay;

    void Start()
    {
        Time.timeScale = 1;
        shopManager = GetComponent<ShopManager>(); 
        userMenu = GetComponent<UserMenu>();
        userStatsMenu = GetComponent<UserStatsMenu>();
        levelSelectMenu = GetComponent<LevelSelectMenu>();
        settingsManager = GetComponent<SettingsManager>();
        screenshotsMenu = GetComponent<ScreenshotsMenu>();
        gameManager = GameManager.Instance;
        
        shopGameObject.SetActive(true);
        userMenuGameObject.SetActive(true);
        screenshotsMenuGameObject.SetActive(true);

        gameInputManager = GameInputManager.Instance;
        gameInputManager.OnRebind += Rebind;

        initialUserCount = GameManager.Instance.users.Count;

        shopItemCount += shopManager.itemManager.guns.Count;
        shopItemCount += shopManager.itemManager.hats.Count;
        shopItemCount += shopManager.itemManager.skins.Count;
        shopItemCount += shopManager.itemManager.upgrades.Count;
        
        if(playButton == null || shopButton == null || statsButton == null || settingsButton == null)
            Debug.LogError("Main Menu button is null");
            
        playButton.onClick.AddListener(levelSelectMenu.RefreshLevels);
        shopButton.onClick.AddListener(shopManager.RefreshShop);
        shopButton.onClick.AddListener(shopManager.RefreshShopScrollRects);
        statsButton.onClick.AddListener(userStatsMenu.RefreshUserStats);
        settingsButton.onClick.AddListener(settingsManager.RefreshFullscreenToggle);
        screenshotsButton.onClick.AddListener(screenshotsMenu.RefreshScreenshots);
        
        gameInputManager.OnControllerChanged += OnControllerChanged;
        gameInputManager.OnKeyboardOnlyInputChanged += OnKeyboardOnlyInputChanged;

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
            
            position = Mouse.current.position.ReadValue();

            InputState.Change(Mouse.current.position, position);
            InputState.Change(Mouse.current.delta, Vector2.zero);
            
            AnchorCursor(position);
        }
        else
        {
            if(gameInputManager.KeyboardOnly())
                return;
            
            Mouse.current.WarpCursorPosition(screenPosition);
            
            Cursor.visible = true;
            cursor.gameObject.SetActive(false);
        }
    }
    
    public void OnKeyboardOnlyInputChanged(bool keyboardOnly)
    {
        if(!gameInputManager.ControllerConnected())
        {
            if(keyboardOnly)
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
    }

    void Rebind(bool enabled)
    {        
        if(gameInputManager.ControllerConnected() || gameInputManager.KeyboardOnly())
        {
            if(enabled)
                StartCoroutine(ScaleCursorCo(Vector3.one, Vector3.zero));
            else
                StartCoroutine(ScaleCursorCo(Vector3.zero, Vector3.one));
        }
    }

    IEnumerator ScaleCursorCo(Vector3 from, Vector3 to)
    {
        float inTime = 0;
        float duration = 0.25f;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            
            float t = inTime / duration;
            cursor.localScale = Vector3.Lerp(from, to, t);   

            yield return null;
        }

        cursor.localScale = to;
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
        
        if(!gameInputManager.Rebinding)
        {
            if(gameInputManager.ControllerConnected())
                MoveCursor(gameInputManager.ControllerCursorMove(), gameInputManager.Click(), gameInputManager.ControllerFastCursor(), gameManager.useDPad);
            else if(gameInputManager.KeyboardOnly())
                MoveCursor(gameInputManager.KeyboardCursorMove(), gameInputManager.Click(), gameInputManager.KeyboardFastCursor(), true);
        }

        if(startDelay > 0)
        {
            if(!startedDelay)
            {
                ToggleMainMenuButtons(false);
                startedDelay = true;
            }
            
            startDelay -= Time.deltaTime;
        }
        else
        {
            if(!endedDelay)
            {
                ToggleMainMenuButtons(true);
                endedDelay = true;
            }
        }
    }

    void MoveCursor(Vector2 input, bool click, bool fastCursor, bool cursorSmoothing)
    {
        if(fastCursor)
            _cursorSpeed = cursorSpeed * 2f;
        else
            _cursorSpeed = cursorSpeed;

        Vector2 target = input * _cursorSpeed;
        cursorVelocity = Vector2.Lerp(cursorVelocity, target, 1 - Mathf.Exp(-gameManager.cursorAcceleration * Time.unscaledDeltaTime));

        if(cursorSmoothing)
        {
            if(input != Vector2.zero)
                cursorVelocity += input * gameManager.cursorAcceleration * Time.unscaledDeltaTime;
            else
                cursorVelocity = Vector2.Lerp(cursorVelocity, Vector2.zero, gameManager.cursorDeceleration * Time.unscaledDeltaTime);
            
            position += cursorVelocity * Time.unscaledDeltaTime;
        }
        else
        {
            position += input * _cursorSpeed * Time.unscaledDeltaTime;
        }

        position.x = Mathf.Clamp(position.x, 0, Screen.width);
        position.y = Mathf.Clamp(position.y, 0, Screen.height);

        InputState.Change(Mouse.current.position, position);
        InputState.Change(Mouse.current.delta, Vector2.zero);

        screenPosition = Mouse.current.position.ReadValue();
        AnchorCursor(screenPosition);

        if(click)
            cursorImage.sprite = cursorSprites[1];
        else
            cursorImage.sprite = cursorSprites[0];
    }

    void AnchorCursor(Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, position, canvas.worldCamera, out Vector2 localPosition);
        cursor.anchoredPosition = localPosition;
    }

    public void ToggleMainMenuButtons(bool interactable)
    {
        for(int i = 0; i < mainMenuButtons.Length; i++)
            mainMenuButtons[i].interactable = interactable;
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