using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject levelSelect;
    [SerializeField] ScrollRect levelsScrollRect;
    [SerializeField] RectTransform levelsContent;
    [SerializeField] LevelButton[] levelButtons;
    [SerializeField] AudioClip levelEnterSound;
    [SerializeField] Button backButton;

    public event Action OnLevelButtonsRefresh;
    MainMenuManager mainMenuManager;
    GameInputManager gameInputManager;
    LevelLoadingManager levelLoadingManager;

    void Start()
    {
        mainMenuManager = GetComponent<MainMenuManager>();    
        gameInputManager = GameInputManager.Instance;
        levelLoadingManager = LevelLoadingManager.Instance;

        gameInputManager.OnControllerChanged += OnControllerChanged;
        gameInputManager.OnKeyboardOnlyInputChanged += OnKeyboardOnlyInputChanged;

        if(gameInputManager.ControllerConnected())
            levelsScrollRect.inertia = false;
        
        for(int i = 0; i < levelButtons.Length; i++)
            levelButtons[i].levelSelectMenu = this;
    }

    void Update()
    {
        if(levelSelect.activeInHierarchy == false || levelLoadingManager.Busy)
            return;
        
        if(gameInputManager.ControllerConnected())
            ScrollContent(gameInputManager.ControllerScrolling(), gameInputManager.ControllerFastCursor());  
        else if(gameInputManager.KeyboardOnly())
            ScrollContent(gameInputManager.KeyboardScrolling(), gameInputManager.KeyboardFastCursor());  
    }

    void ScrollContent(Vector2 input, bool fastCursor)
    {
        if(Mathf.Abs(input.y) > 0.1f)
        {
            if(fastCursor)
                mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
            else
                mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

            levelsContent.anchoredPosition += new Vector2(0, -input.y * mainMenuManager._scrollSpeed * Time.deltaTime);
            levelsContent.anchoredPosition = new Vector2(levelsContent.anchoredPosition.x, Mathf.Clamp(levelsContent.anchoredPosition.y, 0, levelsContent.sizeDelta.y - 524));
        }
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
            levelsScrollRect.inertia = false;
        else
            levelsScrollRect.inertia = true;
    }

    public void OnKeyboardOnlyInputChanged(bool keyboardOnly)
    {
        if(!gameInputManager.ControllerConnected())
        {
            if(keyboardOnly)
                levelsScrollRect.inertia = false;
            else
                levelsScrollRect.inertia = true;
        }
    }
    
    public void RefreshLevels()
    {
        OnLevelButtonsRefresh?.Invoke();
        StartCoroutine(DelayLevelsContentCentering());
    }

    public void EnterLevel(string level)
    {
        OnLevelButtonsRefresh?.Invoke();
		GameInputManager.Instance.RumbleController(0.5f, 0.5f, 0.75f);
        AudioManager.Instance.PlaySound2D(levelEnterSound);
        LevelLoadingManager.Instance.LoadScene(level);

        backButton.interactable = false;
        mainMenuManager.ToggleMainMenuButtons(false);
    }

    IEnumerator DelayLevelsContentCentering()
    {
        yield return null;
        yield return null;

        levelsScrollRect.verticalNormalizedPosition = 1;
    }
}