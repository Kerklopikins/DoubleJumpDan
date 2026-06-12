using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] Animator splashScreenAnimator;
    [SerializeField] GameObject debugUI;
    [SerializeField] GameObject[] objectsToDisable;
    
    float levelDelay = 0.125f;
    bool disabled;

    void Start()
    {
        Time.timeScale = 1;
        QualitySettings.vSyncCount = GameManager.Instance.vSync ? 1 : 0;
    }
    
    void Update()
    {
        if(levelDelay > 0)
        {
            levelDelay -= Time.unscaledDeltaTime;
        }
        else
        {
            splashScreenAnimator.SetBool("Open", true);

            if(Keyboard.current.leftShiftKey.IsPressed() && Keyboard.current.qKey.IsPressed() && !disabled)
            {                
                disabled = true;
                debugUI.SetActive(true);

                for(int i = 0; i < objectsToDisable.Length; i++)
                    objectsToDisable[i].SetActive(false);
            }
        }
    }
    
    public void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}