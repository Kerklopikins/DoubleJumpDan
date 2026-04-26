using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenEffectsManager : MonoBehaviour
{
    public static ScreenEffectsManager Instance;

    [SerializeField] SpriteRenderer hurtSprite;
    [SerializeField] SpriteRenderer whiteFadeSprite;
    [SerializeField] SpriteRenderer greenCircleSprite;

    [Header("Color Temperature")]
    [SerializeField] bool useColorTemperature;
    [SerializeField] Material colorTemperatureMaterial;
    [Range(0f, 3f)]
    [SerializeField] float exposure = 1f;
    [SerializeField] Color tintColor = Color.white;
    
    [Header("Grayscale")]
    [SerializeField] Material grayscaleMaterial;
    [Range(0, 1)]
    [SerializeField] float grayscaleAmount;

    public bool canAnimateWhiteFade { get; private set; }
    Camera _camera;
    bool canAnimateHurt = true;
    bool canAnimateHealth = true;
    bool canAnimateGrayScale = true;
    Player player;
    GameManager gameManager;
    bool usePostProcessing;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        gameManager = GameManager.Instance;

        if(SceneManager.GetActiveScene().name == "Main Menu")
            return;

        player = LevelManager.Instance.player;
        canAnimateWhiteFade = true;
        player.OnPlayerHealthChange += AnimateHealthCollect;
        ResizeScreenEffects();
    }

    void Update()
    {
        ////////////////////////FIX put in start/////////////////////////
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        UpdatePostProcessing();
    }

    public void ResizeScreenEffects()
    {
        float cameraWidth = _camera.orthographicSize * ((float)Screen.width / Screen.height);

        whiteFadeSprite.transform.localScale = new Vector2(cameraWidth + 0.2f, _camera.orthographicSize + 0.2f);
        hurtSprite.transform.localScale = new Vector2(cameraWidth / 32 + 0.01f, _camera.orthographicSize / 32 + 0.01f);
    }

    public void AnimateWhiteFade(float from, float to, float duration)
    {
        if(canAnimateWhiteFade)
            StartCoroutine(AnimateWhiteFadeCo(from, to, duration));
    }

    IEnumerator AnimateWhiteFadeCo(float from, float to, float duration)
    {
        canAnimateWhiteFade = false;

        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            whiteFadeSprite.color = new Color(whiteFadeSprite.color.r, whiteFadeSprite.color.g, whiteFadeSprite.color.b, Mathf.Lerp(from, to, inTime / duration));
            yield return null;
        }  

        whiteFadeSprite.color = new Color(whiteFadeSprite.color.r, whiteFadeSprite.color.g, whiteFadeSprite.color.b, to);
        canAnimateWhiteFade = true;
    }

    public bool CanAnimateWhiteFade()
    {
        if(canAnimateWhiteFade)  
            return true;
        else
            return false;      
    }

    public bool WhiteFadedIn()
    {
        if(whiteFadeSprite.color.a >= 1)
            return true;
        else
            return false;
    }

    public void AnimateHealthCollect()
    {
        if(canAnimateHealth)
            StartCoroutine(AnimateHealthCollectCo());
    }

    IEnumerator AnimateHealthCollectCo()
	{
        canAnimateHealth = false;

        float inTime = 0;        

        while(inTime < 1f)
		{
			greenCircleSprite.color = new Color(greenCircleSprite.color.r, greenCircleSprite.color.g, greenCircleSprite.color.b, Mathf.Lerp(1, 0, inTime / 1f));
            greenCircleSprite.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 25, inTime / 0.35f);
            inTime += Time.deltaTime;
			yield return null;
		}

        greenCircleSprite.transform.localScale = Vector3.zero;
        greenCircleSprite.color = new Color(greenCircleSprite.color.r, greenCircleSprite.color.g, greenCircleSprite.color.b, 0);

        canAnimateHealth = true;
    }
    public void TriggerHurtEffect()
    {
        if(canAnimateHurt)
            StartCoroutine(AnimateHurt());
    }

    IEnumerator AnimateHurt()
	{
		canAnimateHurt = false;

        float inTime = 0;
        hurtSprite.color = new Color(hurtSprite.color.r, hurtSprite.color.g, hurtSprite.color.b, 0);

        while(inTime < 0.2f)
		{
			hurtSprite.color = new Color(hurtSprite.color.r, hurtSprite.color.g, hurtSprite.color.b, Mathf.Lerp(0, 1, inTime / 0.2f));
            inTime += Time.deltaTime;
			yield return null;
		}

        float outTime = 0;
        hurtSprite.color = new Color(hurtSprite.color.r, hurtSprite.color.g, hurtSprite.color.b, 1);

        while(outTime < 0.2f)
        {
            hurtSprite.color = new Color(hurtSprite.color.r, hurtSprite.color.g, hurtSprite.color.b, Mathf.Lerp(1, 0, outTime / 0.2f));
            outTime += Time.deltaTime;
            yield return null;
        }

        hurtSprite.color = new Color(hurtSprite.color.r, hurtSprite.color.g, hurtSprite.color.b, 0);
		canAnimateHurt = true;
    }

    public void HitStop(float timeScale, float duration)
    {
        StartCoroutine(HitStopCo(timeScale, duration));
    }

    IEnumerator HitStopCo(float timeScale, float duration)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = GameHUD.referenceTime;
    }

    public void FadeGrayScale(float from, float to, float duration)
    {
        if(canAnimateGrayScale)
            StartCoroutine(FadeGrayScaleCo(from, to, duration));
    }

    IEnumerator FadeGrayScaleCo(float from, float to, float duration)
    {
        canAnimateGrayScale = false;

        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            AdjustGrayScale(Mathf.Lerp(from, to, inTime / duration));
            yield return null;
        }  

        canAnimateGrayScale = true;
    }

    public void SetTintColor(Color _tintColor)
    {
        tintColor = _tintColor;
    }

    public void AdjustGrayScale(float amount)
    {
        grayscaleAmount = amount;
    }

    public void UpdatePostProcessing()
    {
        if(gameManager == null)
            return;

        usePostProcessing = gameManager.postProcessing;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(!usePostProcessing)
        {
            Graphics.Blit(src, dest);
            return;
        }

        if(!useColorTemperature)
        {
            if(grayscaleMaterial != null)
            {
                grayscaleMaterial.SetFloat("_Amount", grayscaleAmount);
                Graphics.Blit(src, dest, grayscaleMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }

            return;
        }
    
        RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height);
        RenderTexture currentSource = src;

        if(colorTemperatureMaterial != null)
        {
            //colorTemperatureMaterial.SetFloat("_Temperature", colorTemperature);
            colorTemperatureMaterial.SetFloat("_Exposure", exposure);
            colorTemperatureMaterial.SetColor("_TintColor", tintColor);

            Graphics.Blit(currentSource, temp, colorTemperatureMaterial);
            currentSource = temp;
        }
        else
        {
            Graphics.Blit(src, temp);
            currentSource = temp;
        }

        if(grayscaleMaterial != null)
        {
            grayscaleMaterial.SetFloat("_Amount", grayscaleAmount);
            Graphics.Blit(currentSource, dest, grayscaleMaterial);
        }
        else
        {
            Graphics.Blit(currentSource, dest);
        }
        
        RenderTexture.ReleaseTemporary(temp);
    }
}