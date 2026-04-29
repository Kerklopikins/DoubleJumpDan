using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;

public class GameHUD: MonoBehaviour
{
    public static GameHUD Instance;
    
    [Header("Pause Menu")]
    [SerializeField] Button pauseButton;
    [SerializeField] Animator pauseAnimator;
    public AudioClip pauseSound;
    [SerializeField] Text pausedText;
    [SerializeField] Button[] pauseMenuButtons;

    [Header("Game Over")]
    [SerializeField] Animator gameOverAnimator;
    [SerializeField] AudioClip gameOverSound;

    [Header("Finish Level")]
    [SerializeField] Text levelNameText;
    [SerializeField] GameObject levelCompleteUI;

    [Header("Screenshot")]
    [SerializeField] Screenshot screenshotScript;

    [Header("Settings")]
    [SerializeField] GameObject settings;
    
    [Header("Joystick")]
    [SerializeField] RectTransform cursor;
    [SerializeField] Sprite[] cursorSprites;

    [Header("Player Crosshairs")]
    public Transform crosshairsParent;
    public Transform crosshairs;
    [SerializeField] float maxCrosshairsRadius = 10;
    [SerializeField] Sprite[] crosshairSprites;
    [SerializeField] float crosshairsAnimationSpeed = 0.1f;

    public bool paused { get; protected set; }
    public bool canPause { get; set; }
    public float cursorSpeed { get; set; }
    public float crosshairsSmoothSpeed { get; set; }
    public static float referenceTime;
    float startDelay = 1;
    Player player;
    LocalWorldManager localWorldManager;
    RectTransform pauseButtonRect;
    bool inSettings;
    bool canToggleSettings;
    GunInfo gunInfo;
    GameInputManager gameInputManager;
    Camera _camera;
    Canvas canvas;
    Vector2 position;
    Vector2 screenPosition;
    Vector2 input;
    float currentAngle;
    int crosshairAnimationIndex;
    SpriteRenderer crosshairsSprite;
    float crosshairsAnimationTimer;
    float _cursorSpeed;
    float _crosshairsAnimationSpeed;
    bool finishedLevel;
    bool crosshairsRed;
    SettingsManager settingsManager;
    Image cursorImage;

    void Awake()
    {
        Instance = this;
        referenceTime = 1;
    }

    void Start()
    {
        canPause = true;
        player = LevelManager.Instance.player;
        levelNameText.text = SceneManager.GetActiveScene().name + " Completed";
        pauseButtonRect = pauseButton.GetComponent<RectTransform>();
        gameInputManager = GameInputManager.Instance;
        settingsManager = GetComponent<SettingsManager>();

        player.OnPlayerKilled += PlayerKilled;
        player.OnPlayerRespawn += PlayerRespawn;
        gameInputManager.OnControllerChanged += OnControllerChanged;

        Time.timeScale = 1;
        pausedText.text = "Paused\n<size=25>" + SceneManager.GetActiveScene().name + "</size>";
        localWorldManager = LevelManager.Instance.localWorldManager;
        _camera = LevelManager.Instance.mainCamera;

        cursor.transform.localScale = Vector3.zero;
        _cursorSpeed = cursorSpeed;
        canvas = GetComponent<Canvas>();
        cursorImage = cursor.gameObject.GetComponent<Image>();

        position = new Vector2(Screen.width / 2, Screen.height / 4);

        InputState.Change(Mouse.current.position, position);
        InputState.Change(Mouse.current.delta, Vector2.zero);

        if(gameInputManager.ControllerConnected())
        {    
            Cursor.visible = false;

            cursor.gameObject.SetActive(true);
            crosshairsParent.gameObject.SetActive(true);
        }

        crosshairsSprite = crosshairs.GetComponent<SpriteRenderer>();
    }

    public void SubscribeToGun(GunInfo _gunInfo)
    {
        gunInfo = _gunInfo;
    }
    
    bool CanPause()
    {
        if(player.dead || LevelLoadingManager.Instance.busy || LevelManager.Instance.FinishedLevel() || screenshotScript.frozen || inSettings)
            return false;
        else
            return true;
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
        {
            Cursor.visible = false;
            cursor.gameObject.SetActive(true);
            crosshairsParent.gameObject.SetActive(true);
        }
        else
        {
            Cursor.visible = true;
            cursor.gameObject.SetActive(false);
            crosshairsParent.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if(gameInputManager.ControllerConnected())
            crosshairsParent.position = player.transform.position;
    }

    void Update()
    {
        if(gameInputManager.ControllerConnected())
        {
            ///Main Cursor
            if(paused || finishedLevel || screenshotScript.frozen)
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

            ///Player Crosshairs
            Vector2 aimInput = gameInputManager.AimDirection();
            
            if(aimInput.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
                float rotationSpeed = crosshairsSmoothSpeed * aimInput.magnitude;
                currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
            }
            
            float angleRad = currentAngle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            crosshairs.localPosition = direction * maxCrosshairsRadius;

            crosshairsAnimationTimer -= Time.deltaTime;

            if(gameInputManager.ShootButton() && CanPause() && !paused)
            {
                _crosshairsAnimationSpeed = crosshairsAnimationSpeed * 0.5f;
                crosshairsSprite.color = crosshairsRed ? Color.red : Color.yellow;
            }
            else
            {
                _crosshairsAnimationSpeed = crosshairsAnimationSpeed;
                crosshairsSprite.color = Color.white;
            }
                
            if(crosshairsAnimationTimer <= 0)
            {
                crosshairAnimationIndex++;

                if(crosshairAnimationIndex > crosshairSprites.Length - 1)
                    crosshairAnimationIndex = 0;

                crosshairsSprite.sprite = crosshairSprites[crosshairAnimationIndex];
                crosshairsAnimationTimer = _crosshairsAnimationSpeed;
                
                crosshairsRed = !crosshairsRed;
            }
        }

        if(!LevelManager.Instance.FinishedLevel())
            gunInfo.CanShoot(!IsCursorOverPauseButton());

        player.gameHUDPaused = paused;
        player.gameHUDFrozen = screenshotScript.frozen;

        if(startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            pauseButton.interactable = false;
            return;
        }

        if(!CanPause() || paused)
            pauseButton.interactable = false;
        else
            pauseButton.interactable = true;

        if(CanPause())
        {
            if(!paused)
            {
                if(gameInputManager.PauseButtonDown())
                    TogglePause();
            }
            else if(paused)
            {
                if(gameInputManager.EscapeButtonDown() || gameInputManager.PauseButtonDown())
                    TogglePause();
            }
        }
            
        if(gameInputManager.EscapeButtonDown())
            ToggleSettings(false);
    }

    public bool IsCursorOverPauseButton()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(pauseButtonRect, Mouse.current.position.ReadValue(), _camera);
    }

    IEnumerator DelayPauseCo()
    {
        canPause = false;
        paused = true;
        referenceTime = 0;
        Time.timeScale = 0;

        ResetCursorPosition();
        ScaleCursor(Vector3.zero, Vector3.one);

        for(int i = 0; i < pauseMenuButtons.Length; i++)
            pauseMenuButtons[i].interactable = false;

        AudioManager.Instance.PlaySound2D(pauseSound);
        ScreenEffectsManager.Instance.FadeGrayScale(0, 1, 0.35f);
        AudioManager.Instance.FadeMusicOut(0.35f);
        pauseAnimator.SetBool("Paused", true);
        yield return new WaitForSecondsRealtime(0.45f);
        
        for(int i = 0; i < pauseMenuButtons.Length; i++)
            pauseMenuButtons[i].interactable = true;

        canPause = true;
        canToggleSettings = true;
    }
    IEnumerator DelayResumeCo()
    {
        canPause = false;
        canToggleSettings = false;

        ScaleCursor(Vector3.one, Vector3.zero);

        for(int i = 0; i < pauseMenuButtons.Length; i++)
            pauseMenuButtons[i].interactable = false;

        AudioManager.Instance.PlaySound2D(pauseSound);
        ScreenEffectsManager.Instance.FadeGrayScale(1, 0, 0.35f);
        AudioManager.Instance.FadeMusicIn(0.35f);
        pauseAnimator.SetBool("Paused", false);
        yield return new WaitForSecondsRealtime(0.45f);
        referenceTime = 1;
        Time.timeScale = 1;
        paused = false;
        canPause = true;
    }

    public void ScaleCursor(Vector3 from, Vector3 to)
    {
        StartCoroutine(ScaleCursorCo(from, to));
    }

    public void ResetCursorPosition()
    {
        if(gameInputManager.ControllerConnected())
        {
            position = new Vector2(Screen.width / 2, Screen.height / 4);
            InputState.Change(Mouse.current.position, position);
            InputState.Change(Mouse.current.delta, Vector2.zero);
        }
    }
    
    IEnumerator ScaleCursorCo(Vector3 from, Vector3 to)
    {
        float inTime = 0;
        float duration = 0.35f;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            
            float t = inTime / duration;
            cursor.localScale = Vector3.Lerp(from, to, t);   

            yield return null;
        }

        cursor.localScale = to;
    }

    public void TogglePause()
    {
        if(startDelay > 0)
            return;

        if(canPause)
        {
            if(!paused)
                StartCoroutine(DelayPauseCo());
            else if(paused && !inSettings)
                StartCoroutine(DelayResumeCo());
        }
    }  

    public void FinishLevel()
    {
        if(localWorldManager.world != LocalWorldManager.World.Tutorial)
        {
            finishedLevel = true;

            ResetCursorPosition();            
            ScaleCursor(Vector3.zero, Vector3.one);

            levelCompleteUI.SetActive(true);
        }
        else
        {
            LoadMainMenu();
        }
    }

    public void LoadScene(string sceneToLoad)
    {
        if(canPause)
            LevelLoadingManager.Instance.LoadScene(sceneToLoad);
    }

    public void LoadNextScene()
    {        
        if(canPause)
        {
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

            if(nextScene + 1 > SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("Scene doesn't exist in build index, loading main menu instead...");
                LevelLoadingManager.Instance.LoadScene("Main Menu");
            }
            else
            {
                LevelLoadingManager.Instance.LoadScene(nextScene);
            }
        }
    }

    public void Restart()
    {
        if(canPause)
            LevelLoadingManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        if(canPause)
            LevelLoadingManager.Instance.LoadScene("Main Menu");
    }

    public void ToggleSettings(bool open)
    {
        if(open && !inSettings)
        {
            if(canToggleSettings)
                StartCoroutine(DelaySettingsOpen());
        }
        else if(!open && inSettings)
        {
            if(canToggleSettings)
                StartCoroutine(DelaySettingsClose());
        }
    }
    IEnumerator DelaySettingsOpen()
    {
        canToggleSettings = false;
        canPause = false;
        settings.SetActive(true);
        pauseAnimator.SetBool("Settings", true);
        inSettings = true;
        AudioManager.Instance.PlaySound2D(pauseSound);

        for(int i = 0; i < pauseMenuButtons.Length; i++)
            pauseMenuButtons[i].interactable = false;   

        yield return new WaitForSecondsRealtime(0.45f);
        canToggleSettings = true;
    }

    IEnumerator DelaySettingsClose()
    {
        canToggleSettings = false;
        pauseAnimator.SetBool("Settings", false);
        AudioManager.Instance.PlaySound2D(pauseSound);
        settingsManager.SaveSettings();
        
        yield return new WaitForSecondsRealtime(0.45f);

        settings.SetActive(false);
        inSettings = false;
        canPause = true;
        canToggleSettings = true;

        for(int i = 0; i < pauseMenuButtons.Length; i++)
            pauseMenuButtons[i].interactable = true;
    }

    public void PlayerKilled()
    {
        if(player.lives == 0)
            GameOver();
        else if(player.lives != 0)
            StartCoroutine(KillCo(0, 1, 1));
    }

    public void PlayerRespawn()
    {
        currentAngle = 0;
        StartCoroutine(RespawnCo(1, 0, 0.45f));
    }

    IEnumerator KillCo(float from, float to, float duration)
    {
        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;

            ScreenEffectsManager.Instance.AdjustGrayScale(Mathf.Lerp(from, to, inTime / duration));
            AudioManager.Instance.sfxSource.pitch = Mathf.Lerp(1, 0.125f, inTime / duration);
            AudioManager.Instance.musicSource.pitch = Mathf.Lerp(1, 0.125f, inTime / duration);

            Time.timeScale = Mathf.Lerp(1, 0.2f, inTime / duration);
            yield return null;
        }
    }

    IEnumerator RespawnCo(float from, float to, float duration)
    {
        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;

            ScreenEffectsManager.Instance.AdjustGrayScale(Mathf.Lerp(from, to, inTime / duration));
            AudioManager.Instance.sfxSource.pitch = Mathf.Lerp(0.125f, 1, inTime / duration);
            AudioManager.Instance.musicSource.pitch = Mathf.Lerp(0.125f, 1, inTime / duration);

            Time.timeScale = Mathf.Lerp(0.2f, 1, inTime / duration);
            yield return null;
        }

        Time.timeScale = 1;

        ScreenEffectsManager.Instance.AdjustGrayScale(0);
        AudioManager.Instance.sfxSource.pitch = 1;
        AudioManager.Instance.musicSource.pitch = 1;
    }

    public void GameOver()
    {
		AudioManager.Instance.PlaySound2D(gameOverSound);
		gameOverAnimator.enabled = true;
    }
}