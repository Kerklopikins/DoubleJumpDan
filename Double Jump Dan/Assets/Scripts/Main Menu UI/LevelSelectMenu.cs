using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject levelSelect;
    [SerializeField] ScrollRect levelsScrollRect;
    [SerializeField] RectTransform levelsContent;
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

        if(gameInputManager.ControllerConnected())
            levelsScrollRect.inertia = false;
    }

    void Update()
    {
        if(levelSelect.activeSelf == false || levelLoadingManager.Busy)
            return;
            
        if(gameInputManager.ControllerConnected())
        {
            if(Mathf.Abs(gameInputManager.ScrollDirection().y) > 0.1f)
            {
                if(gameInputManager.FastCursorButton())
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                levelsContent.anchoredPosition += new Vector2(0, -gameInputManager.ScrollDirection().y * mainMenuManager._scrollSpeed * Time.deltaTime);
                levelsContent.anchoredPosition = new Vector2(levelsContent.anchoredPosition.x, Mathf.Clamp(levelsContent.anchoredPosition.y, 0, levelsContent.sizeDelta.y - 524));
            }
        }
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
            levelsScrollRect.inertia = false;
        else
            levelsScrollRect.inertia = true;
    }

    public void RefreshLevels()
    {
        OnLevelButtonsRefresh?.Invoke();
        StartCoroutine(DelayLevelsContentCentering());
    }

    IEnumerator DelayLevelsContentCentering()
    {
        yield return null;
        yield return null;

        levelsScrollRect.verticalNormalizedPosition = 1;
    }
}