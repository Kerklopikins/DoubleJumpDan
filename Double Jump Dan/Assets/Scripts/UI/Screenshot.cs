using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class Screenshot : MonoBehaviour
{
    public Animator animator { get; set; }
    public bool frozen { get; protected set; }
    public bool canShoot { get; set; }
    GameHUD gameHUD;
    float startDelay = 1;
    Player player;
    GameInputManager gameInputManager;

    void Start()
    {
        canShoot = true;

        animator = GetComponent<Animator>();
        gameHUD = GameHUD.Instance;
        player = LevelManager.Instance.player;
        gameInputManager = GameInputManager.Instance;
    }

    void Update()
    {
        if(startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            return;
        }
        
        if(player.dead || LevelLoadingManager.Instance.busy || LevelManager.Instance.FinishedLevel() || gameHUD.paused)
            return;

        if(gameInputManager.ScreenshotButtonDown())
            ToggleScreenshot();

        if(canShoot)
            if(frozen)
                if(gameInputManager.EscapeButtonDown())
                    StartCoroutine(ExitScreenshotCo());
    }

    IEnumerator TakeScreenshotCo()
    {
        canShoot = false;
        frozen = true;
        Time.timeScale = 0;
        gameHUD.ScaleCursor(Vector3.zero, Vector3.one);
        gameHUD.ResetCursorPosition();
        
        if(!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Double Jump Dan")))
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Double Jump Dan"));

        ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Double Jump Dan") + "/DJD Screenshot " + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + " " + "(" + string.Format("{0:hh-mm-ss tt}", DateTime.Now) + ")" + ".jpg");
        
        yield return new WaitForEndOfFrame();
        
        animator.SetBool("Screenshot", true);
        
        yield return new WaitForSecondsRealtime(1.9f);
        
        canShoot = true;
    }
    IEnumerator ExitScreenshotCo()
    {
        canShoot = false;
        animator.SetBool("Screenshot", false);
        gameHUD.ScaleCursor(Vector3.one, Vector3.zero);
        AudioManager.Instance.PlaySound2D(gameHUD.pauseSound);
        
        yield return new WaitForSecondsRealtime(0.35f);
        Time.timeScale = 1;
        frozen = false;
        canShoot = true;
    }

    public void ExitScreenshot()
    {
        if(canShoot)
            if(frozen)
                StartCoroutine(ExitScreenshotCo());
    }

    public void ToggleScreenshot()
    {
        if(canShoot)
        {
            if(!frozen)
            {
                StartCoroutine(TakeScreenshotCo());

            }
            else
                StartCoroutine(ExitScreenshotCo());
        }
    }
}