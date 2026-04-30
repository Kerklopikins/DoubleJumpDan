using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class ScreenshotsMenu : MonoBehaviour
{
    [SerializeField] GameObject screenshots;
    [SerializeField] Button screenshotMenuButton;
    [SerializeField] Image screenshotButton;
    [SerializeField] GameObject noScreenshotsText;
    [SerializeField] ScrollRect screenshotsScrollRect;
    [SerializeField] RectTransform screenshotsContent;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] ContentSizeFitter contentSizeFitter;

    public Animator screenshotViewerAnimator;
    public Text screenshotViewerText;
    public Image screenshotViewerImage;

    UIScreenManager uiScreenManager;
    GameInputManager gameInputManager;
    MainMenuManager mainMenuManager;
    bool loadedScreenshots;
    DirectoryInfo directory;
    FileInfo[] files;
    public int screenshotsCount { get; private set; }

    void Awake()
    {
        if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Double Jump Dan"))
        {
            mainMenuManager.screenshotsCount = 0;
            noScreenshotsText.SetActive(true);
        }
        else
        {
            directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Double Jump Dan");
            files = directory.GetFiles("*.jpg");
            screenshotsCount = files.Length;
        }
    }
    void Start()
    {
        uiScreenManager = GetComponent<UIScreenManager>();
        gameInputManager = GameInputManager.Instance;
        mainMenuManager = GetComponent<MainMenuManager>();

        screenshotMenuButton.onClick.AddListener(RefreshScreenshots);
        gameInputManager.OnControllerChanged += OnControllerChanged;

        if(gameInputManager.ControllerConnected())
            screenshotsScrollRect.inertia = false;

        MaybeLoadScreenshots();
    }

    void Update()
    {
        if(screenshots.activeSelf == false)
            return;
            
        if(gameInputManager.ControllerConnected())
        {
            if(Mathf.Abs(gameInputManager.AimDirection().y) > 0.1f)
            {
                if(gameInputManager.LeftTrigger())
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                screenshotsContent.anchoredPosition += new Vector2(0, -gameInputManager.AimDirection().y * mainMenuManager._scrollSpeed * Time.deltaTime);
                screenshotsContent.anchoredPosition = new Vector2(screenshotsContent.anchoredPosition.x, Mathf.Clamp(screenshotsContent.anchoredPosition.y, 0, screenshotsContent.sizeDelta.y - 624));
            }
        }
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
            screenshotsScrollRect.inertia = false;
        else
            screenshotsScrollRect.inertia = true;
    }
    
    public void RefreshScreenshots()
    {
        StartCoroutine(DelayContentCentering());
    }

    IEnumerator DelayContentCentering()
    {
        yield return null;
        yield return null;

        screenshotsScrollRect.verticalNormalizedPosition = 1;
    }

    public void Initialize()
    {
        gridLayoutGroup.enabled = false;
        contentSizeFitter.enabled = false;
    }

    private Sprite LoadSprite(string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null");
            return null;
        }

        if(!File.Exists(path))
        {
            Debug.LogError("File is null");
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        ImageConversion.LoadImage(texture, bytes, false);
        texture.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        //texture.Apply(false, false);

        return sprite;
    }

    public void MaybeLoadScreenshots()
    {
		if(noScreenshotsText.activeInHierarchy)
			return;
		
        if(loadedScreenshots)
            return;

        if(files.Length == 0)
        {
            noScreenshotsText.SetActive(true);
            return;
        }

        for(int i = 0; i < files.Length; i++)
        {
            Image _image = Instantiate(screenshotButton, Vector3.zero, Quaternion.identity, screenshotsContent.transform);
            _image.transform.localPosition = new Vector3(_image.transform.localPosition.x, _image.transform.localPosition.y, 0);
            string imageName = files[i].CreationTime.Month + "-" + files[i].CreationTime.Day + "-" + files[i].CreationTime.Year;
            _image.GetComponentInChildren<Text>().text = imageName;
            _image.name = imageName;
            _image.sprite = LoadSprite(files[i].FullName);
            ScreenshotButton button = _image.GetComponent<ScreenshotButton>();
            button.screenshotsMenu = this;
            button.uiScreenManager = uiScreenManager;
            mainMenuManager.screenshotsCount += 1;
            //_image.GetComponentInChildren<Text>().text = files[i].CreationTime.Month + "-" + files[i].CreationTime.Day + "-" + files[i].CreationTime.Year;
            //_image.name = files[i].ToString();
        }

        loadedScreenshots = true;
    }
}