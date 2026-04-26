using UnityEngine;
using System.Collections;

public class StatsHUD : MonoBehaviour
{
    public static StatsHUD Instance;

    [Header("Gems")]
    public Transform gemIcon;
    [SerializeField] SpriteRenderer[] gemsCounter;
    [SerializeField] Sprite[] gemsCounterImages;
    [SerializeField] SpriteRenderer smallGemsCounter;
    [SerializeField] Sprite[] smallGemsImages;

    [Header("Lives")]
    [SerializeField] SpriteRenderer livesCounter;
    [SerializeField] Sprite[] liveSprites;

    [Header("Health Bar")]
    [SerializeField] Transform healthBarFillPivot;

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
    bool canAnimateHurtUI = true;
    bool canAnimatedGemUI = true;
    bool canAnimatedAmmoUI = true;
    GunInfo gunInfo;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = LevelManager.Instance.player;
        
        player.OnPlayerHurt += PlayerHurt;
        player.OnPlayerKilled += PlayerKilled;
        player.OnPlayerRespawn += PlayerRespawn;
        player.OnPlayerHealthChange += UpdateHealthBar;

        smallGemCountStartPosition = smallGemsCounter.transform.localPosition;
        gemsCounter[0].sprite = gemsCounterImages[0];
        smallGemsCounter.sprite = smallGemsImages[0];

        uiFlashMaterial.SetFloat("_FlashAmount", 0);
        PositionUI();
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
        float _startingHealth = player.health;
        var healthPercent = player._health / _startingHealth;
        healthPercent = Mathf.Clamp01(healthPercent);
        healthBarFillPivot.transform.localScale = new UnityEngine.Vector3(healthPercent, 1, 1);
    }

    public void PlayerKilled()
    {
        livesCounter.sprite = liveSprites[player.lives];
    }

    public void PlayerRespawn()
    {
        healthBarFillPivot.transform.localScale = Vector3.one;
    }

    void OnApplicationQuit()
    {
        uiFlashMaterial.SetFloat("_FlashAmount", 0);
    }
    IEnumerator AmmoBarUIAnimation()
	{
        Vector3 maxScale = new Vector3(1.3f, 1.3f, 1);

		canAnimatedAmmoUI = false;

        ammoTransform.transform.localScale = maxScale;

        float inTime = 0;
        
        while(inTime < 0.075f)
        {
            ammoTransform.transform.localScale = Vector3.Lerp(maxScale, Vector3.one, inTime / 0.075f);
            inTime += Time.deltaTime;
            yield return null;
        }

        ammoTransform.transform.localScale = Vector3.one;
		canAnimatedAmmoUI = true;
    }

    IEnumerator HurtUIAnimation()
	{
        Vector3 maxScale = new Vector3(1.5f, 1.5f, 1);

		canAnimateHurtUI = false;

        float inTime = 0;

        while(inTime < 0.1f)
		{
            healthTransform.transform.localScale = Vector3.Lerp(Vector3.one, maxScale, inTime / 0.1f);
            inTime += Time.deltaTime;
			yield return null;
		}

        float outTime = 0;
        
        while(outTime < 0.1f)
        {
            healthTransform.transform.localScale = Vector3.Lerp(maxScale, Vector3.one, outTime / 0.1f);
            outTime += Time.deltaTime;
            yield return null;
        }

        healthTransform.transform.localScale = Vector3.one;
		canAnimateHurtUI = true;
    }

    IEnumerator GemCollectAnimation()
	{
        Vector3 maxScale = new Vector3(1.2f, 1.2f, 1);

		canAnimatedGemUI = false;

        float inTime = 0;

        while(inTime < 0.05f)
		{
            gemsTransform.transform.localScale = Vector3.Lerp(Vector3.one, maxScale, inTime / 0.05f);
            inTime += Time.deltaTime;
			yield return null;
		}

        float outTime = 0;
        
        while(outTime < 0.05f)
        {
            gemsTransform.transform.localScale = Vector3.Lerp(maxScale, Vector3.one, outTime / 0.05f);
            outTime += Time.deltaTime;
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
            yield return new WaitForSeconds(0.07f);
            uiFlashMaterial.SetFloat("_FlashAmount", 0);
            yield return new WaitForSeconds(0.07f);
        }
    }
}