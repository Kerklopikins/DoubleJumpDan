using UnityEngine;
using UnityEngine.UI;

public class ExitUIArea : MonoBehaviour
{   
    [SerializeField] PanelType panelType;
    public Animator previousPanel;
    [SerializeField] float animationLength;
    public UIScreenManager uIScreenManager;
    [SerializeField] AudioClip exitSound;
    [SerializeField] bool saveGameDataOnExit;
    [SerializeField] SettingsManager settingsManager;
    [SerializeField] ShopManager shopManager;

    public bool CanExit { get; set; }
    GameInputManager gameInputManager;
    LevelLoadingManager levelLoadingManager;
    MiniPanel miniPanel;
    Animator animator;
    public enum PanelType { Normal, Mini }
    float exitTimer;
    float exitTimerLength = 0.125f;
    bool attempingExit;
    bool playSoundOnExit;

    void Start()
    {
        gameInputManager = GameInputManager.Instance;    
        levelLoadingManager = LevelLoadingManager.Instance;
        CanExit = true;

        if(panelType == PanelType.Mini)
            miniPanel = GetComponent<MiniPanel>();

        animator = GetComponent<Animator>();
    }

    public void SetPreviousPanel()
    {
        uIScreenManager.currentOpenPanel = previousPanel;
        SetTransitionTimer();
    }

    public void SetTransitionTimer()
    {
        uIScreenManager.transitionTimer = animationLength;
    }

    void OnEnable()
    {
        SetTransitionTimer();
        attempingExit = false;
        exitTimer = exitTimerLength;
    }

    void Update()
    { 
        if(levelLoadingManager.Busy || gameInputManager.Rebinding || !CanExit || uIScreenManager.currentOpenPanel != animator)
        {
            attempingExit = false;
            exitTimer = exitTimerLength;
            return;
        }
        
        if(attempingExit)
        {
            exitTimer -= Time.deltaTime;

            if(exitTimer <= 0 && uIScreenManager.transitionTimer <= 0)
            {
                //if(!uIScreenManager.MouseInputDisabled())
                //{
                    //uIScreenManager.ToggleMouseInput(false);
                //}
                if(!uIScreenManager.MouseInputDisabled())
                {
                    ExitArea();
                    attempingExit = false;
                }
            }
        }
        else
        {
            exitTimer = exitTimerLength;
        }
        
        if(uIScreenManager.transitionTimer <= 0 && uIScreenManager.currentOpenPanel == animator)
        {
            if(gameInputManager.EscapeButtonDown())
            {
                playSoundOnExit = true;
                attempingExit = true;
            }
        }
    }

    public void Exit()
    {
        playSoundOnExit = false;
        attempingExit = true;
    }

    void ExitArea()
    {
        if(uIScreenManager.currentOpenPanel == animator)
        {
            switch(panelType)
            {
                case PanelType.Normal:
                    uIScreenManager.OpenPanel(previousPanel);
                    
                    if(saveGameDataOnExit)
                    {
                        if(settingsManager != null)
                            settingsManager.SaveSettings();

                        if(shopManager != null)
                            shopManager.SaveShopData();
                    }
                    break;
                case PanelType.Mini:
                    miniPanel.Close(animator);

                    if(saveGameDataOnExit)
                    {
                        if(settingsManager != null)
                            settingsManager.SaveSettings();

                        if(shopManager != null)
                            shopManager.SaveShopData();
                    }
                    break;
            }
            
            if(playSoundOnExit)
                AudioManager.Instance.PlaySound2D(exitSound);

            SetTransitionTimer();
        }
    }
}