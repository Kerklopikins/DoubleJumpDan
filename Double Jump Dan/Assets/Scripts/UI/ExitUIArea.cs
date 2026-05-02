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
    GameInputManager gameInputManager;
    LevelLoadingManager levelLoadingManager;
    MiniPanel miniPanel;
    Animator animator;
    public enum PanelType { Normal, Mini }

    void Start()
    {
        gameInputManager = GameInputManager.Instance;    
        levelLoadingManager = LevelLoadingManager.Instance;

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
    }

    void Update()
    { 
        if(levelLoadingManager.Busy || gameInputManager.rebinding)
            return;
            
        if(uIScreenManager.transitionTimer <= 0)
        {
            if(gameInputManager.EscapeButtonDown())
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
                    
                    AudioManager.Instance.PlaySound2D(exitSound);
                    SetTransitionTimer();
                }
            }
        }
    }
}
