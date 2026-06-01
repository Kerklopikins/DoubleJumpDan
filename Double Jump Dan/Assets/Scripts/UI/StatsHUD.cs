using UnityEngine;
using System.Collections;

public class StatsHUD : MonoBehaviour
{
    public static StatsHUD Instance;

    [Header("Gems")]
    public Transform gemIcon;
    [SerializeField] Sprite[] gemIconSprites;
    [SerializeField] SpriteRenderer[] gemsCounter;
    [SerializeField] Sprite[] gemsCounterImages;
    [SerializeField] SpriteRenderer smallGemsCounter;
    [SerializeField] Sprite[] smallGemsImages;

    [Header("Double Gems")]
    [SerializeField] Sprite[] doubleGemsCounterImages;
    [SerializeField] Sprite[] doubleSmallGemsImages;

    [Header("Lives")]
    [SerializeField] SpriteRenderer livesCounter;
    [SerializeField] Sprite[] liveSprites;
    [SerializeField] Sprite[] extraLifeLiveSprites;

    [Header("Health Bar")]
    [SerializeField] Transform healthBarFillPivot;
    [SerializeField] GameObject extraHealthBar;
    [SerializeField] Transform extraHealthBarFillPivot;
    [SerializeField] float barScaleDuration;

    [Header("Ammo Bar")]
    [SerializeField] Transform ammoBarPivot;

    [Header("UI Offsets")]
    [SerializeField] Transform gemsTransform;
    [SerializeField] Vector2 gemsUIOffset;
    public Transform healthTransform;
    [SerializeField] Vector2 healthUIOffset;
    [SerializeField] Transform ammoTransform;
    [SerializeField] Vector2 ammoUIOffset;

    [Header("Other")]
    [SerializeField] Material uiFlashMaterial;
    [SerializeField] Camera _camera;

    int gemsCounterIndex; 
    int gemsImageIndex;
    int smallGemsImageIndex;
    Player player;
    Vector3 smallGemCountStartPosition; 
    WaitForSeconds flashSpeed = new WaitForSeconds(0.07f);
    bool canAnimateHurtUI = true;
    bool canAnimatedGemUI = true;
    bool canAnimatedAmmoUI = true;
    GunInfo gunInfo;
    bool usingExtraHealthBar;
    bool hasExtraLives;
    GameManager gameManager;
    SpriteRenderer gemIconSprite;
    float gemInTime;
    float gemOutTime;
    float hurtInTime;
    float hurtOutTime;
    float ammoBarInTime;
    float healthBarScaleInTime;
    Vector3 ammoBarMaxScale = new Vector3(1.3f, 1.3f, 1);
    Vector3 hurtUIMaxScale = new Vector3(1.5f, 1.5f, 1);
    Vector3 gemUIMaxScale = new Vector3(1.2f, 1.2f, 1);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        player = LevelManager.Instance.player;
        gemIconSprite = gemIcon.GetComponent<SpriteRenderer>();

        player.OnPlayerHurt += PlayerHurt;
        player.OnPlayerKilled += PlayerKilled;
        player.OnPlayerRespawn += PlayerRespawn;
        player.OnPlayerHealthChange += UpdateHealthBar;
            
        smallGemCountStartPosition = smallGemsCounter.transform.localPosition;
        gemsCounter[0].sprite = gemsCounterImages[0];
        smallGemsCounter.sprite = smallGemsImages[0];

        uiFlashMaterial.SetFloat("_FlashAmount", 0);
        CheckForUpgrades();
        PositionUI();
    }

    void CheckForUpgrades()
    {
        //The Red Cross
        if(gameManager.currentUser.equippedUpgrades.Contains(6487))
        {
            usingExtraHealthBar = true;
            extraHealthBar.SetActive(true);
        }
        
        //The Golden Heart
        if(gameManager.currentUser.equippedUpgrades.Contains(5480))
        {
            hasExtraLives = true;
            livesCounter.sprite = extraLifeLiveSprites[extraLifeLiveSprites.Length - 1];
        }

        //The Giving Gem
        if(gameManager.currentUser.equippedUpgrades.Contains(8902))
        {
            gemIconSprite.sprite = gemIconSprites[1];

            gemsCounterImages = doubleGemsCounterImages;
            smallGemsImages = doubleSmallGemsImages;
        }
    }

    public void SubscribeToGun(GunInfo _gunInfo)
    {
        gunInfo = _gunInfo;
        gunInfo.OnAmmoChanged += UpdateAmmoInfo;
    }

    public void PositionUI()
    {
        float cameraHalfWidth = _camera.orthographicSize * ((float)Screen.width / Screen.height);

        healthTransform.localPosition = new Vector3(healthTransform.transform.localPosition.x + healthUIOffset.x, _camera.orthographicSize + healthUIOffset.y, 0);
        ammoTransform.localPosition = new Vector3(cameraHalfWidth + ammoUIOffset.x, _camera.orthographicSize + ammoUIOffset.y, 0);
        gemsTransform.localPosition = new Vector3(-cameraHalfWidth + gemsUIOffset.x, _camera.orthographicSize + gemsUIOffset.y, 0);
    }

    public void UpdateAmmoInfo(float currentAmmo)
    {
        if(ammoBarPivot == null)
            return;

        if(currentAmmo < gunInfo.maxAmmo)
            if(canAnimatedAmmoUI)
                StartCoroutine(AmmoBarUIAnimation());

        float startingAmmo = gunInfo.maxAmmo;
        var ammoPercent = currentAmmo / startingAmmo;
        ammoBarPivot.transform.localScale = new Vector3(ammoPercent, 1, 1);
    }

    public void UpdateGemsCounter(int gems)
    {
        if(canAnimatedGemUI)
            StartCoroutine(GemCollectAnimation());

        gemsCounterIndex = gems / 50;
        
        int rowValue = gems % 50;
        gemsImageIndex = (rowValue / 5);
        
        if(gems >= 50)
            gemsCounter[0].sprite = gemsCounterImages[10];
        else if(gems < 50)
            gemsCounter[0].sprite = gemsCounterImages[gemsImageIndex];

        if(gems >= 100)
            gemsCounter[1].sprite = gemsCounterImages[10];
        else if(gems < 100 && gemsCounterIndex == 1)
            gemsCounter[1].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 1)
            gemsCounter[1].sprite = gemsCounterImages[0];

        if(gems >= 150)
            gemsCounter[2].sprite = gemsCounterImages[10];
        else if(gems < 150 && gemsCounterIndex == 2)
            gemsCounter[2].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 2)
            gemsCounter[2].sprite = gemsCounterImages[0];

        if(gems >= 200)
            gemsCounter[3].sprite = gemsCounterImages[10];
        else if(gems < 200 && gemsCounterIndex == 3)
            gemsCounter[3].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 3)
            gemsCounter[3].sprite = gemsCounterImages[0];

        if(gems >= 250)
            gemsCounter[4].sprite = gemsCounterImages[10];
        else if(gems < 250 && gemsCounterIndex == 4)
            gemsCounter[4].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 4)
            gemsCounter[4].sprite = gemsCounterImages[0];

        if(gems >= 300)
            gemsCounter[5].sprite = gemsCounterImages[10];
        else if(gems < 300 && gemsCounterIndex == 5)
            gemsCounter[5].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 5)
            gemsCounter[5].sprite = gemsCounterImages[0];

        if(gems >= 350)
            gemsCounter[6].sprite = gemsCounterImages[10];
        else if(gems < 350 && gemsCounterIndex == 6)
            gemsCounter[6].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 6)
            gemsCounter[6].sprite = gemsCounterImages[0];

        if(gems >= 400)
            gemsCounter[7].sprite = gemsCounterImages[10];
        else if(gems < 400 && gemsCounterIndex == 7)
            gemsCounter[7].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 7)
            gemsCounter[7].sprite = gemsCounterImages[0];

        if(gems >= 450)
            gemsCounter[8].sprite = gemsCounterImages[10];
        else if(gems < 450 && gemsCounterIndex == 8)
            gemsCounter[8].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 8)
            gemsCounter[8].sprite = gemsCounterImages[0];

        if(gems >= 500)
            gemsCounter[9].sprite = gemsCounterImages[10];
        else if(gems < 500 && gemsCounterIndex == 9)
            gemsCounter[9].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 9)
            gemsCounter[9].sprite = gemsCounterImages[0];

        if(gems >= 550)
            gemsCounter[10].sprite = gemsCounterImages[10];
        else if(gems < 550 && gemsCounterIndex == 10)
            gemsCounter[10].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 10)
            gemsCounter[10].sprite = gemsCounterImages[0];
        
        if(gems >= 600)
            gemsCounter[11].sprite = gemsCounterImages[10];
        else if(gems < 600 && gemsCounterIndex == 11)
            gemsCounter[11].sprite = gemsCounterImages[gemsImageIndex];
        else if(gemsCounterIndex != 11)
            gemsCounter[11].sprite = gemsCounterImages[0];
       
        smallGemsImageIndex = gems % 5;
        smallGemsCounter.transform.localPosition = new Vector3(smallGemCountStartPosition.x + gemsImageIndex - 0.375f * gemsImageIndex, smallGemCountStartPosition.y - gemsCounterIndex, 0);

        if(smallGemsImageIndex > -1)
            smallGemsCounter.sprite = smallGemsImages[smallGemsImageIndex];
        else
            smallGemsCounter.sprite = smallGemsImages[0];
    }

    public void PlayerHurt()
    {
        ScreenEffectsManager.Instance.TriggerHurtEffect();

        if(canAnimateHurtUI)
            StartCoroutine(HurtUIAnimation());

        UpdateHealthBar();
        StartCoroutine(FlashCo());
    }

    void UpdateHealthBar()
    {
        if(!usingExtraHealthBar)
        {
            healthBarFillPivot.transform.localScale = new Vector3(Mathf.Clamp01((float)player._health / player.health), 1, 1);
        }
        else
        {
            healthBarFillPivot.transform.localScale = new Vector3(Mathf.Clamp01((float)player._health / 100), 1, 1);
            extraHealthBarFillPivot.transform.localScale = new Vector3(Mathf.Clamp((float)player._health - 100, 0, 50) / 50, 1, 1); 
        }
    }

    public void PlayerKilled()
    {
        livesCounter.sprite = hasExtraLives ? extraLifeLiveSprites[player.lives] : liveSprites[player.lives];
    }

    public void PlayerRespawn()
    {
        StartCoroutine(AnimateHealthBarsToFull());
        //healthBarFillPivot.transform.localScale = Vector3.one;
        //extraHealthBarFillPivot.transform.localScale = Vector3.one;
    }

    void OnApplicationQuit()
    {
        uiFlashMaterial.SetFloat("_FlashAmount", 0);
    }

    IEnumerator AnimateHealthBarsToFull()
    {
        healthBarScaleInTime = 0;

        while(healthBarScaleInTime < barScaleDuration)
        {
            healthBarScaleInTime += Time.unscaledDeltaTime;
            healthBarFillPivot.transform.localScale = Vector3.Lerp(new Vector3(0, 1, 1), Vector3.one, healthBarScaleInTime / barScaleDuration);
            extraHealthBarFillPivot.transform.localScale = Vector3.Lerp(new Vector3(0, 1, 1), Vector3.one, healthBarScaleInTime / barScaleDuration);

            yield return null;
        }

        healthBarFillPivot.transform.localScale = Vector3.one;
        extraHealthBarFillPivot.transform.localScale = Vector3.one;
    }

    IEnumerator AmmoBarUIAnimation()
	{
		canAnimatedAmmoUI = false;

        ammoTransform.transform.localScale = ammoBarMaxScale;
        ammoBarInTime = 0;
        
        while(ammoBarInTime < 0.075f)
        {
            ammoBarInTime += Time.deltaTime;
            ammoTransform.transform.localScale = Vector3.Lerp(ammoBarMaxScale, Vector3.one, ammoBarInTime / 0.075f);
            yield return null;
        }

        ammoTransform.transform.localScale = Vector3.one;
		canAnimatedAmmoUI = true;
    }

    IEnumerator HurtUIAnimation()
	{
		canAnimateHurtUI = false;
        hurtInTime = 0;

        while(hurtInTime < 0.1f)
		{
            hurtInTime += Time.deltaTime;
            healthTransform.transform.localScale = Vector3.Lerp(Vector3.one, hurtUIMaxScale, hurtInTime / 0.1f);
			yield return null;
		}

        hurtOutTime = 0;
        
        while(hurtOutTime < 0.1f)
        {
            hurtOutTime += Time.deltaTime;
            healthTransform.transform.localScale = Vector3.Lerp(hurtUIMaxScale, Vector3.one, hurtOutTime / 0.1f);
            yield return null;
        }

        healthTransform.transform.localScale = Vector3.one;
		canAnimateHurtUI = true;
    }

    IEnumerator GemCollectAnimation()
	{
		canAnimatedGemUI = false;

        gemInTime = 0;

        while(gemInTime < 0.05f)
		{
            gemInTime += Time.deltaTime;
            gemsTransform.transform.localScale = Vector3.Lerp(Vector3.one, gemUIMaxScale, gemInTime / 0.05f);
			yield return null;
		}

        gemOutTime = 0;
        
        while(gemOutTime < 0.05f)
        {
            gemOutTime += Time.deltaTime;
            gemsTransform.transform.localScale = Vector3.Lerp(gemUIMaxScale, Vector3.one, gemOutTime / 0.05f);
            yield return null;
        }

        gemsTransform.transform.localScale = Vector3.one;
		canAnimatedGemUI = true;
    }

    IEnumerator FlashCo()
    {
        for(int i = 0; i < 4; i++)
        {
            uiFlashMaterial.SetFloat("_FlashAmount", 1);
            yield return flashSpeed;
            uiFlashMaterial.SetFloat("_FlashAmount", 0);
            yield return flashSpeed;
        }
    }
}