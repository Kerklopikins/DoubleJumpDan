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

    void Start()
    {
        mainMenuManager = GetComponent<MainMenuManager>();    
        gameInputManager = GameInputManager.Instance;
    }

    void Update()
    {
        if(levelSelect.activeSelf == false)
            return;
            
        if(gameInputManager.ControllerConnected())
        {
            if(Mathf.Abs(gameInputManager.AimDirection().y) > 0.1f)
            {
                if(gameInputManager.LeftTrigger())
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                levelsContent.anchoredPosition += new Vector2(0, -gameInputManager.AimDirection().y * mainMenuManager._scrollSpeed * Time.deltaTime);
                levelsContent.anchoredPosition = new Vector2(levelsContent.anchoredPosition.x, Mathf.Clamp(levelsContent.anchoredPosition.y, 0, 918));
            }
        }
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