using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoadingManager : MonoBehaviour
{
    public static LevelLoadingManager Instance;
    [SerializeField] Sprite[] loadingDanSprites;
    [SerializeField] Sprite[] gameOverLoadingDanSprites;
    [SerializeField] bool startClear;

    public bool animationFinished { get; set; }
    public bool loading { get; set; }
    public bool Busy { get; set; }
	public SpriteRenderer fadeSprite { get; set; }
    SpriteRenderer loadingDan;
    Transform loadingSpritesTransform;
    GameObject loadingCircle;
    SpriteRenderer loadingCircleSprite;
    bool loaded;
    float levelDelay = 0.125f;
    GameObject eventSystem;
    GameHUD gameHUD;
    LocalWorldManager localWorldManager;
    Player player;
    GameObject levelLoadingGameObject;
    Camera _camera;
    float fadeDuration = 0.6f;
    bool fadedOut;

    void Awake()
    {
        Instance = this;

        levelLoadingGameObject = GameObject.Find("Level Loading");
        
        fadeSprite = levelLoadingGameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        loadingSpritesTransform = levelLoadingGameObject.transform.GetChild(1).transform;

        loadingDan = loadingSpritesTransform.GetChild(0).GetComponent<SpriteRenderer>();
        loadingCircle = loadingDan.transform.GetChild(0).gameObject;
        loadingCircleSprite = loadingCircle.GetComponent<SpriteRenderer>();
        _camera = Camera.main;

        if(!startClear)
            fadeSprite.color = Color.black;
    }

    bool IsNormalLevel()
    {
        if(localWorldManager.world == LocalWorldManager.World.MainMenu || localWorldManager.world == LocalWorldManager.World.SplashScreen)
            return false;
        else
            return true;
    }

	void Start()
	{
		ResizeFadeBackground();

        localWorldManager = GameObject.FindWithTag("Level Managers").GetComponent<LocalWorldManager>();

        if(IsNormalLevel())
        {
            player = LevelManager.Instance.player;
            gameHUD = GameHUD.Instance;
            
            if(GameManager.died)
            {
                fadeSprite.color = new Color(1, 0, 0, fadeSprite.color.a);
                loadingCircleSprite.color = Color.black;
            }
        }

        if(localWorldManager.world != LocalWorldManager.World.SplashScreen)
            eventSystem = GameObject.Find("Event System");    

        if(localWorldManager.world == LocalWorldManager.World.MainMenu)
            levelDelay = 0.5f;
        else if(IsNormalLevel())
            levelDelay = 0.125f;
    }

    void Update()
    {
        if(loading)
            loadingCircle.transform.Rotate(Vector3.forward, -250 * Time.unscaledDeltaTime);

        if(startClear)
            return;
            
        if(levelDelay > 0)
        {
            levelDelay -= Time.deltaTime;
            return;
        }

        if(!fadedOut)
            StartCoroutine(FadeOutCo());
    }
    
    IEnumerator FadeInCo()
    {
        Busy = true;

        if(eventSystem != null && eventSystem.activeInHierarchy == true)
            eventSystem.SetActive(false);

        if(player != null)
        {
            if(player.lives > 0)
            {
                loadingCircleSprite.color = Color.white;
                fadeSprite.color = new Color(0, 0, 0, fadeSprite.color.a);
            }
            else
            {
                loadingCircleSprite.color = Color.black;
                fadeSprite.color = new Color(1, 0, 0, fadeSprite.color.a);
                GameManager.died = true;
            }
        }
        else
        {
            loadingCircleSprite.color = Color.white;
            fadeSprite.color = new Color(0, 0, 0, fadeSprite.color.a);
        }

        float inTime = 0;

        while(inTime < fadeDuration)
        {
            inTime += Time.unscaledDeltaTime;
            
            if(localWorldManager.world != LocalWorldManager.World.SplashScreen)
                loadingSpritesTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, inTime / fadeDuration);

            fadeSprite.color = new Color(fadeSprite.color.r, fadeSprite.color.g, fadeSprite.color.b, Mathf.Lerp(0, 1, inTime / fadeDuration));
            yield return null;
        }

        fadeSprite.color = new Color(fadeSprite.color.r, fadeSprite.color.g, fadeSprite.color.b, 1);
        loadingSpritesTransform.localScale = Vector3.one;

        animationFinished = true;
    }

    IEnumerator FadeOutCo()
    {
        fadedOut = true;
        Busy = true;

        float inTime = 0;

        while(inTime < fadeDuration)
        {
            inTime += Time.unscaledDeltaTime;
            fadeSprite.color = new Color(fadeSprite.color.r, fadeSprite.color.g, fadeSprite.color.b, Mathf.Lerp(1, 0, inTime / fadeDuration));
            yield return null;
        }

        fadeSprite.color = new Color(fadeSprite.color.r, fadeSprite.color.g, fadeSprite.color.b, 0);
        Busy = false;

        if(IsNormalLevel())
            GameManager.died = false;
    }

    IEnumerator ScaleLoadingSprites()
    {
        loaded = true;

        float inTime = 0;

        while(inTime < fadeDuration)
        {
            inTime += Time.unscaledDeltaTime;

            if(localWorldManager.world != LocalWorldManager.World.SplashScreen)
                loadingSpritesTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, inTime / fadeDuration);
            
            yield return null;
        }

        loadingSpritesTransform.localScale = Vector3.zero;
    }

    public void ResizeFadeBackground()
    {
        float cameraWidth = _camera.orthographicSize * ((float)Screen.width / Screen.height);
        fadeSprite.transform.localScale = new Vector2(cameraWidth + 0.2f, _camera.orthographicSize + 0.2f);
    }

    public void ForceFadeResizeBackground()
    {
        StartCoroutine(ForceFadeResizeBackgroundCo());
    }

    IEnumerator ForceFadeResizeBackgroundCo()
    {
        while(fadeSprite.color.a > 0.1f)
        {
            ResizeFadeBackground();
            yield return null;
        }   
    }

    public void LoadScene(int sceneToLoad)
    {
        loading = true;

        if(localWorldManager.world != LocalWorldManager.World.SplashScreen)
            GameManager.Instance.SaveUserData();

        ResizeFadeBackground();		
        StartCoroutine(LoadSceneSlowly(sceneToLoad));
        StartCoroutine(FadeInCo());
    }

    public void LoadScene(string sceneToLoad)
    {
        loading = true;

        if(localWorldManager.world != LocalWorldManager.World.SplashScreen)
            GameManager.Instance.SaveUserData();
            
        ResizeFadeBackground();		
        StartCoroutine(LoadSceneSlowly(sceneToLoad));
        StartCoroutine(FadeInCo());
    }

    IEnumerator LoadSceneSlowly(int sceneToLoad)
    {
        AnimateDanAndMusicFade();

        while(!animationFinished)
            yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        while(!asyncLoad.isDone)
        {
            asyncLoad.allowSceneActivation = false;

            if(asyncLoad.progress == 0.9f && !loaded)
                StartCoroutine(ScaleLoadingSprites());

            if(loadingSpritesTransform.localScale.x <= 0)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }

    IEnumerator LoadSceneSlowly(string sceneToLoad)
    {
        AnimateDanAndMusicFade();

        while(!animationFinished)
            yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        while(!asyncLoad.isDone)
        {
            asyncLoad.allowSceneActivation = false;

            if(asyncLoad.progress == 0.9f && !loaded)
                StartCoroutine(ScaleLoadingSprites());

            if(loadingSpritesTransform.transform.localScale.x <= 0)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }

    void AnimateDanAndMusicFade()
    {
        if(localWorldManager.world == LocalWorldManager.World.SplashScreen)
            return;
            
        if(IsNormalLevel())
        {
            if(!gameHUD.paused)
                AudioManager.Instance.FadeMusicOut(1);
        }
        else
        {
            if(AudioManager.Instance != null)
                AudioManager.Instance.FadeMusicOut(1);
        }

        if(IsNormalLevel())
        {
            if(GameManager.died)
                StartCoroutine(LoadingDanAnimation());
            else
                StartCoroutine(LoadingDanAnimation());
        }
        else
        {
            StartCoroutine(LoadingDanAnimation());
        }
    }

    IEnumerator LoadingDanAnimation()
    {
        while(true)
        {
            if(IsNormalLevel() && player.lives <= 0)
            {
                loadingDan.sprite = gameOverLoadingDanSprites[0];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = gameOverLoadingDanSprites[1];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = gameOverLoadingDanSprites[2];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = gameOverLoadingDanSprites[1];
                yield return new WaitForSecondsRealtime(0.075f);
            }
            else
            {
                loadingDan.sprite = loadingDanSprites[0];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = loadingDanSprites[1];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = loadingDanSprites[2];
                yield return new WaitForSecondsRealtime(0.075f);
                loadingDan.sprite = loadingDanSprites[1];
                yield return new WaitForSecondsRealtime(0.075f);
            }
        }
    }
}