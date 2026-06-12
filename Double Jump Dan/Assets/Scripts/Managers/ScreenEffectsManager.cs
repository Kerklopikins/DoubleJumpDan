using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

[ExecuteAlways]
[ImageEffectAllowedInSceneView]
public class ScreenEffectsManager : MonoBehaviour
{
    public static ScreenEffectsManager Instance;
    
    [SerializeField] SpriteRenderer hurtSprite;
    [SerializeField] SpriteRenderer whiteFadeSprite;
    [SerializeField] SpriteRenderer greenCircleSprite;
    [SerializeField] SpriteRenderer[] respawnBackgroundSprites;

    [Header("Color Temperature")]
    [SerializeField] Material colorTemperatureMaterial;
    [SerializeField, Range(0f, 3f)] float exposure = 1f;
    [SerializeField] Color tintColor = Color.white;
    
    [Header("Grayscale")]
    [SerializeField] Material grayscaleMaterial;
    [SerializeField, Range(0, 1)] float grayscaleAmount;

    [Header("Motion Blur")]
    [SerializeField] Material motionBlurMaterial;
    [SerializeField, Range(0, 1)] float intensity = 0.05f;
    [SerializeField, Range(2, 64)] int sampleCount = 8;

    public bool canAnimateWhiteFade { get; private set; }
    Camera _camera;
    bool canAnimateHurt = true;
    bool canAnimateHealth = true;
    bool canAnimateGrayScale = true;
    Player player;
    GameManager gameManager;
    bool usePostProcessing = true;
    bool useMotionBlur = true;
    Matrix4x4 previousVP;
    bool firstFrame = true;
    RenderTexture temp;
    RenderTexture temp2;
    RenderTexture currentSource;

    void Awake()
    {
        if(!Application.isPlaying)
            return;
        
        if(Instance == null)
            Instance = this;
    }

    void Start()
    {
        if(_camera == null)
            _camera = GetComponent<Camera>();
        
        if(!Application.isPlaying)
            return;
        
        gameManager = GameManager.Instance;
        UpdatePostProcessing();

        if(SceneManager.GetActiveScene().name == "Main Menu")
            return;

        player = LevelManager.Instance.player;
        canAnimateWhiteFade = true;
        player.OnPlayerHealthChange += AnimateHealthCollect;
        
        ResizeScreenEffects();
    }

    public void ResizeScreenEffects()
    {
        float cameraWidth = _camera.orthographicSize * ((float)Screen.width / Screen.height);

        whiteFadeSprite.transform.localScale = new Vector2(cameraWidth + 0.2f, _camera.orthographicSize + 0.2f);
        hurtSprite.transform.localScale = new Vector2(cameraWidth / 32 + 0.01f, _camera.orthographicSize / 32 + 0.01f);
    }

    // public void AnimateRespawnBackground(float from, float to, float duration, float inBetweenDelay)
    // {
    //     StartCoroutine(AnimateRespawnBackgroundCo(from, to, duration, inBetweenDelay));
    // }

    // IEnumerator AnimateRespawnBackgroundCo(float from, float to, float duration, float inBetweenDelay)
    // {
    //     float inTime = 0;

    //     while(inTime < duration)
    //     {
    //         inTime += Time.unscaledDeltaTime;
    //         respawnBackgroundSprites[0].size = new Vector2(respawnBackgroundSprites[0].size.x, Mathf.Lerp(from, to, inTime / duration));
    //         respawnBackgroundSprites[1].size = new Vector2(respawnBackgroundSprites[1].size.x, Mathf.Lerp(from, to, inTime / duration));
    //         yield return null;
    //     }  

    //     respawnBackgroundSprites[0].size = new Vector2(respawnBackgroundSprites[0].size.x, to);
    //     respawnBackgroundSprites[1].size = new Vector2(respawnBackgroundSprites[1].size.x, to);

    //     yield return new WaitForSecondsRealtime(inBetweenDelay);

    //     float outTime = 0;

    //     while(outTime < duration)
    //     {
    //         outTime += Time.unscaledDeltaTime;
    //         respawnBackgroundSprites[0].size = new Vector2(respawnBackgroundSprites[0].size.x, Mathf.Lerp(to, from, outTime / duration));
    //         respawnBackgroundSprites[1].size = new Vector2(respawnBackgroundSprites[1].size.x, Mathf.Lerp(to, from, outTime / duration));
    //         yield return null;
    //     }  

    //     respawnBackgroundSprites[0].size = new Vector2(respawnBackgroundSprites[0].size.x, from);
    //     respawnBackgroundSprites[1].size = new Vector2(respawnBackgroundSprites[1].size.x, from);
    // }

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
        useMotionBlur = gameManager.motionBlur;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        currentSource = src;
        
        if(usePostProcessing && colorTemperatureMaterial != null)
        {
            temp = RenderTexture.GetTemporary(src.width, src.height);
            
            colorTemperatureMaterial.SetFloat("_Exposure", exposure);
            colorTemperatureMaterial.SetColor("_TintColor", tintColor);

             Graphics.Blit(currentSource, temp, colorTemperatureMaterial);
             currentSource = temp;
        }

        if(useMotionBlur && motionBlurMaterial != null)
        {
            temp2 = RenderTexture.GetTemporary(src.width, src.height);
            
            Matrix4x4 view = _camera.worldToCameraMatrix;
            Matrix4x4 proj = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true);
            Matrix4x4 currentVP = proj * view;

            if(firstFrame)
            {
                previousVP = currentVP;
                firstFrame = false;
            }

            motionBlurMaterial.SetMatrix("_PreviousVP", previousVP);
            motionBlurMaterial.SetMatrix("_CurrentVPInverse", currentVP.inverse);
            motionBlurMaterial.SetFloat ("_Intensity", intensity);
            motionBlurMaterial.SetInt   ("_SampleCount", sampleCount);

            Graphics.Blit(currentSource, temp2, motionBlurMaterial);
            currentSource = temp2;

            previousVP = currentVP;   
        }

        if(usePostProcessing && grayscaleMaterial != null)
        {
            grayscaleMaterial.SetFloat("_Amount", grayscaleAmount);
            Graphics.Blit(currentSource, dest, grayscaleMaterial);
        }
        else
        {
            Graphics.Blit(currentSource, dest);   
        }

        if(usePostProcessing)
            RenderTexture.ReleaseTemporary(temp);

        if(useMotionBlur)
            RenderTexture.ReleaseTemporary(temp2);
    }
}