using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class ScreenshotsMenu : MonoBehaviour
{
    [SerializeField] GameObject screenshots;
    [SerializeField] Text titleText;
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
    DirectoryInfo directory;
    FileInfo[] files;
    public int screenshotsCount { get; private set; }
    float verticalNormalizePosition = 1;
    bool checkedNormalizedPosition;

    void Awake()
    {
        mainMenuManager = GetComponent<MainMenuManager>();

        if(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Double Jump Dan"))
        {
            directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Double Jump Dan");
            files = directory.GetFiles("*.jpg");
            screenshotsCount = files.Length;

            if(screenshotsCount == 0)
            {
                mainMenuManager.screenshotsCount = 0;
                noScreenshotsText.SetActive(true);
                titleText.text = "No Screenshots";
            }
            else
            {
                if(screenshotsCount == 1)
                    titleText.text = "1 Screenshot";
                else if(screenshotsCount > 1)
                    titleText.text = screenshotsCount + " Screenshots";
            }
        }
        else
        {
            mainMenuManager.screenshotsCount = 0;
            noScreenshotsText.SetActive(true);
            titleText.text = "No Screenshots";
        }
    }

    void Start()
    {
        uiScreenManager = GetComponent<UIScreenManager>();
        gameInputManager = GameInputManager.Instance;

        gameInputManager.OnControllerChanged += OnControllerChanged;
        gameInputManager.OnKeyboardOnlyInputChanged += OnKeyboardOnlyInputChanged;

        if(gameInputManager.ControllerConnected())
            screenshotsScrollRect.inertia = false;

        if(screenshotsCount > 0)
            MaybeLoadScreenshots();
    }

    void Update()
    {
        if(screenshots.activeInHierarchy == false)
        {
            checkedNormalizedPosition = false;
            return;
        }
        else if(screenshots.activeInHierarchy && !checkedNormalizedPosition)
        {
            StartCoroutine(DelayContentReposition(verticalNormalizePosition));
            checkedNormalizedPosition = true;
        }
        
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

            screenshotsContent.anchoredPosition += new Vector2(0, -input.y * mainMenuManager._scrollSpeed * Time.deltaTime);
            screenshotsContent.anchoredPosition = new Vector2(screenshotsContent.anchoredPosition.x, Mathf.Clamp(screenshotsContent.anchoredPosition.y, 0, screenshotsContent.sizeDelta.y - 624));
        }
    }

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
            screenshotsScrollRect.inertia = false;
        else
            screenshotsScrollRect.inertia = true;
    }
    
    public void OnKeyboardOnlyInputChanged(bool keyboardOnly)
    {
        if(!gameInputManager.ControllerConnected())
        {
            if(keyboardOnly)
                screenshotsScrollRect.inertia = false;
            else
                screenshotsScrollRect.inertia = true;
        }
    }

    public void OpenScreenshotsViewerPanel()
    {
        verticalNormalizePosition = screenshotsScrollRect.verticalNormalizedPosition;
        uiScreenManager.OpenPanel(screenshotViewerAnimator);
    }
    
    public void RefreshScreenshots()
    {
        verticalNormalizePosition = 1;
    }

    IEnumerator DelayContentReposition(float normalizedPosition)
    {
        yield return null;
        yield return null;

        screenshotsScrollRect.verticalNormalizedPosition = normalizedPosition;
    }

    public void Initialize()
    {
        gridLayoutGroup.enabled = false;
        contentSizeFitter.enabled = false;
    }

    Sprite LoadSprite(string path)
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

        return sprite;
    }

    void MaybeLoadScreenshots()
    {
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
            mainMenuManager.screenshotsCount += 1;
        }
    }
}