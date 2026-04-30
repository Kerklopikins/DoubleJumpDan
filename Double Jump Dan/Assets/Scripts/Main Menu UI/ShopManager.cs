using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [SerializeField] GameObject shop;
    public ConfirmPurchase confirmPurchase;
	[SerializeField] Text gemsText;
    [SerializeField] AudioClip tooExpensiveSound;
    public ScrollRect gunScrollRect;
    public ScrollRect hatScrollRect;
    public ScrollRect skinScrollRect;
    public Vector2 minMaxVisibilityDistance;
    public ItemManager itemManager;
    [SerializeField] float itemWidth;
    [SerializeField] RectTransform gunsContent;
    [SerializeField] RectTransform hatsContent;
    [SerializeField] RectTransform skinsContent;
    [SerializeField] Button skinsButton;
    [SerializeField] Button hatsButton;
    [SerializeField] Button gunsButton;
    [SerializeField] AudioClip shopTabSwitchSound;

    [Header("Custom Skin")]
    [SerializeField] Image bodyImage;
    [SerializeField] Image armsLegsImage;
    [SerializeField] Slider[] sliders;
    [SerializeField] Image[] sliderHandles;
    [SerializeField] Image[] sliderFills;
    [SerializeField] Sprite[] sliderFillSprites;
    [SerializeField] Text[] rgbValuesText;

    public event Action OnShopItemsChanged;
    public event Action OnShopTabsChanged;
    public RectTransform currentGunRect { get; set; }
    public RectTransform currentHatRect { get; set; }
    public RectTransform currentSkinRect { get; set; }
    CurrentShopTab currentShopTab = CurrentShopTab.Guns;
    public enum CurrentShopTab { Guns, Hats, Skins }
    GameManager gameManager;
    GameInputManager gameInputManager;
    MainMenuManager mainMenuManager;
    int shopTabIndex = 2;
    float shopTabTransitionTimer;
    UIScreenManager uIScreenManager;
    Animator shopAnimator;
    UIArea gunsArea;
    UIArea hatsArea;
    UIArea skinsArea;
    //bool scrollViewMoved;
    //int gunsCount;
    //int hatsCount;
    //int skinsCount;
    //public int currentItemIndex { get; set; }

    void Awake()
    {
        Instance = this;       
    }
    void Start()
    {
        gameManager = GameManager.Instance;
        gameInputManager = GameInputManager.Instance;
        mainMenuManager = GetComponent<MainMenuManager>();
        uIScreenManager = GetComponent<UIScreenManager>();
        shopAnimator = shop.GetComponent<Animator>();

        gameInputManager.OnControllerChanged += OnControllerChanged;

        if(gameInputManager.ControllerConnected())
        {
            gunScrollRect.inertia = false;
            hatScrollRect.inertia = false;
            skinScrollRect.inertia = false;
        }

        skinsButton.onClick.AddListener(SwitchToSkinTab);
        hatsButton.onClick.AddListener(SwitchToHatTab);
        gunsButton.onClick.AddListener(SwitchToGunTab);
        
        skinsArea = skinsButton.GetComponent<UIArea>();
        hatsArea = hatsButton.GetComponent<UIArea>();
        gunsArea = gunsButton.GetComponent<UIArea>();
    
        gunsButton.interactable = false;

        UpdateCustomSkinSliders();

        //UpdateCustomSkinBody();
        //UpdateCustomSkinArmsLegs();
        //gunsCount = itemManager.guns.Count - 1;
        //hatsCount = itemManager.hats.Count - 1;
        //skinsCount = itemManager.skins.Count - 1;
    }

    void Update()
    {        
        if(shop.activeSelf == false)
            return;

        if(shopTabTransitionTimer > 0)
            shopTabTransitionTimer -= Time.deltaTime;

        if(gameInputManager.ControllerConnected() && uIScreenManager.currentOpenPanel == shopAnimator && uIScreenManager.transitionTimer <= 0)
        {
            if(Mathf.Abs(gameInputManager.AimDirection().x) > 0.1f)
            {
                //scrollViewMoved = true;

                if(gameInputManager.LeftTrigger())
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                switch(currentShopTab)
                {   
                    case CurrentShopTab.Guns:
                        gunsContent.anchoredPosition += new Vector2(-gameInputManager.AimDirection().x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);
                        gunsContent.anchoredPosition = new Vector2(Mathf.Clamp(gunsContent.anchoredPosition.x, -gunsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), gunsContent.anchoredPosition.y);
                        break;
                    case CurrentShopTab.Hats:
                        hatsContent.anchoredPosition += new Vector2(-gameInputManager.AimDirection().x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);      
                        hatsContent.anchoredPosition = new Vector2(Mathf.Clamp(hatsContent.anchoredPosition.x, -hatsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), hatsContent.anchoredPosition.y);
                        break;
                    case CurrentShopTab.Skins:
                        skinsContent.anchoredPosition += new Vector2(-gameInputManager.AimDirection().x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);      
                        skinsContent.anchoredPosition = new Vector2(Mathf.Clamp(skinsContent.anchoredPosition.x, -skinsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), skinsContent.anchoredPosition.y);
                        break;
                }
            }
            
            if(shopTabTransitionTimer <= 0)
            {
                if(gameInputManager.LeftBumperDown())
                {
                    shopTabIndex--;
                    
                    if(shopTabIndex == -1)
                        shopTabIndex = 2;

                    SwitchShopTabs();
                }

                if(gameInputManager.RightBumperDown())
                {                    
                    shopTabIndex++;
                    
                    if(shopTabIndex == 3)
                        shopTabIndex = 0;

                    SwitchShopTabs();
                }
            }

            //if(gameInputManager.LeftBumperDown())
            //{
                //if(!scrollViewMoved)
               // {               
                    //currentItemIndex--;
               // }
                //else 
                //{
                    //CenterScrollRectsAndUpdateScrollIndex();
                    //scrollViewMoved = false;
                //}

                //switch(currentShopTab)
                //{   
                    //case CurrentShopTab.Guns:
                        //ScrollItems(gunsCount, gunsContent);          
                        //break;
                    //case CurrentShopTab.Hats:
                        //ScrollItems(hatsCount, hatsContent);                    
                        //break;
                    //case CurrentShopTab.Skins:
                        //ScrollItems(skinsCount, skinsContent);                   
                        //break;
                //}
            //}
            //else if(gameInputManager.RightBumperDown())
            //{
                //if(!scrollViewMoved)
                //{               
                    //currentItemIndex++;
                //}
                //else
                //{
                    //CenterScrollRectsAndUpdateScrollIndex();
                    //scrollViewMoved = false;
                //}

                //switch(currentShopTab)
                //{   
                    //case CurrentShopTab.Guns:
                        //ScrollItems(gunsCount, gunsContent);          
                        //break;
                    //case CurrentShopTab.Hats:
                        //ScrollItems(hatsCount, hatsContent);                    
                        //break;
                    //case CurrentShopTab.Skins:
                        //ScrollItems(skinsCount, skinsContent);                   
                        //break;
                //}
            //}
        }
    
    }   

    //void ScrollItems(int itemCount, RectTransform content)
    //{            
        //currentItemIndex = Mathf.Clamp(currentItemIndex, 0, itemCount - 2);
        //content.anchoredPosition = new Vector2(-(currentItemIndex * itemWidth), content.anchoredPosition.y);      
    //}
    
    //public void UpdateScrollIndex(int index, int itemCount)
    //{
        //currentItemIndex = index - 2;
        //currentItemIndex = Mathf.Clamp(currentItemIndex, 0, itemCount - 2);
    //}
    
    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
        {
            gunScrollRect.inertia = false;
            hatScrollRect.inertia = false;
            skinScrollRect.inertia = false;
        }
        else
        {
            gunScrollRect.inertia = true;
            hatScrollRect.inertia = true;
            skinScrollRect.inertia = true;
        }
    }

    void ResetShopTabTransitionTimer()
    {
        shopTabTransitionTimer = 0.3f;
    }

    void SwitchShopTabs()
    {
        switch(shopTabIndex)
        {
            case 0:
                skinsButton.interactable = false;
                hatsButton.interactable = true;
                gunsButton.interactable = true;

                if(currentShopTab != CurrentShopTab.Skins)
                {
                    currentShopTab = CurrentShopTab.Skins;
                    
                    skinsArea.OpenArea(true);
                    hatsArea.OpenArea(false);
                    gunsArea.OpenArea(false);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
                break;
            case 1:
                skinsButton.interactable = true;
                hatsButton.interactable = false;
                gunsButton.interactable = true;

                if(currentShopTab != CurrentShopTab.Hats)
                {
                    currentShopTab = CurrentShopTab.Hats;

                    skinsArea.OpenArea(false);
                    hatsArea.OpenArea(true);
                    gunsArea.OpenArea(false);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
                break;
            case 2:
                skinsButton.interactable = true;
                hatsButton.interactable = true;
                gunsButton.interactable = false;

                if(currentShopTab != CurrentShopTab.Guns)
                {
                    currentShopTab = CurrentShopTab.Guns;

                    skinsArea.OpenArea(false);
                    hatsArea.OpenArea(false);
                    gunsArea.OpenArea(true);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
                break;
        }
    }

    public void SwitchToGunTab()
    {
        if(gunsButton.interactable && shopTabTransitionTimer <= 0)
        {
            shopTabIndex = 2;
            SwitchShopTabs();
        }
    }

    public void SwitchToSkinTab()
    {
        if(skinsButton.interactable && shopTabTransitionTimer <= 0)
        {
            shopTabIndex = 0;
            SwitchShopTabs();
        }
    }

    public void SwitchToHatTab()
    {
        if(hatsButton.interactable && shopTabTransitionTimer <= 0)
        {
            shopTabIndex = 1;
            SwitchShopTabs();
        }
    }

    public void EquipItem(ShopItem shopItem)
	{
        itemManager.EquipItem(shopItem);
        UpdateCustomSkinSliders();

        OnShopItemsChanged?.Invoke();
    }

    public void RefreshShop()
    {
        RefreshGemsText();
        UpdateCustomSkinSliders();
        OnShopItemsChanged?.Invoke();  
    }

    public void RefreshShopScrollRects()
    {
        StartCoroutine(DelayShopContentCentering());
    }

    IEnumerator DelayShopContentCentering()
    {
        yield return null;
        yield return null;
        
        CenterScrollRects();
    }
    
    void CenterScrollRects()
    {
        switch(currentShopTab)
        {   
            case CurrentShopTab.Guns:
                CenterShopScrollRect(gunScrollRect, currentGunRect);
                //UpdateScrollIndex(currentGunRect.transform.GetSiblingIndex() + 1, gunsCount);
                break;
            case CurrentShopTab.Hats:
                CenterShopScrollRect(hatScrollRect, currentHatRect);
                //UpdateScrollIndex(currentHatRect.transform.GetSiblingIndex() + 1, hatsCount);
                break;
            case CurrentShopTab.Skins:
                CenterShopScrollRect(skinScrollRect, currentSkinRect);
                //UpdateScrollIndex(currentSkinRect.transform.GetSiblingIndex() + 1, skinsCount);
                break;
        }
    }

    public void CenterShopScrollRect(ScrollRect scrollRect, RectTransform currentRect)
    {
        float itemPosition = Mathf.Abs(currentRect.anchoredPosition.x);
        float targetPosition = itemPosition - (scrollRect.viewport.rect.width / 2);

        float normalized = Mathf.Clamp01(targetPosition / (scrollRect.content.rect.width - scrollRect.viewport.rect.width));
        scrollRect.horizontalNormalizedPosition = normalized;

        OnShopTabsChanged?.Invoke();
    }
    
    public void RefreshGemsText()
    {
        if(gameManager.currentUser.gems != 1)
            gemsText.text = "<color=yellow>" + gameManager.currentUser.gems.ToString() + "</color>" + "\nGems";
		else
            gemsText.text = "<color=yellow>" + gameManager.currentUser.gems.ToString() + "</color>" + "\nGem";
    }
    public void RefreshGemsText(string text)
    {
        gemsText.text = text;
    }

    void UpdateCustomSkinSliders()
    {
        //Custom skin
        if(gameManager.currentUser.skinID == 9999)
        {
            for(int i = 0; i < sliderFills.Length; i++)
            {
                sliders[i].interactable = true;
                sliderFills[i].sprite = sliderFillSprites[0];
            }
        }
        else
        {
            for(int i = 0; i < sliderFills.Length; i++)
            {
                sliders[i].interactable = false;
                sliderFills[i].sprite = sliderFillSprites[1];
            }
        }

        sliders[0].value = gameManager.currentUser.customSkinColor[0];
        sliders[1].value = gameManager.currentUser.customSkinColor[1];
        sliders[2].value = gameManager.currentUser.customSkinColor[2];

        sliders[3].value = gameManager.currentUser.customSkinColor[3];
        sliders[4].value = gameManager.currentUser.customSkinColor[4];
        sliders[5].value = gameManager.currentUser.customSkinColor[5];

        UpdateCustomSkinBody();
        UpdateCustomSkinArmsLegs();
    }
    
    public void UpdateCustomSkinBody()
    {
        bodyImage.color = new Color(sliders[0].value, sliders[1].value, sliders[2].value, 1);

        sliderHandles[0].color = new Color(sliders[0].value, 0, 0, 1);
        sliderHandles[1].color = new Color(0, sliders[1].value, 0, 1);
        sliderHandles[2].color = new Color(0, 0, sliders[2].value, 1);
        sliderFills[0].color = new Color(sliders[0].value, 0, 0, 1);
        sliderFills[1].color = new Color(0, sliders[1].value, 0, 1);
        sliderFills[2].color = new Color(0, 0, sliders[2].value, 1);

        rgbValuesText[0].text = Mathf.RoundToInt(255 * sliders[0].value).ToString() + "   " + Mathf.RoundToInt(255 * sliders[1].value).ToString() + "   " + Mathf.RoundToInt(255 * sliders[2].value).ToString();
    }

    public void UpdateCustomSkinArmsLegs()
    {
        armsLegsImage.color = new Color(sliders[3].value, sliders[4].value, sliders[5].value, 1);
        sliderHandles[3].color = new Color(sliders[3].value, 0, 0, 1);
        sliderHandles[4].color = new Color(0, sliders[4].value, 0, 1);
        sliderHandles[5].color = new Color(0, 0, sliders[5].value, 1);
        sliderFills[3].color = new Color(sliders[3].value, 0, 0, 1);
        sliderFills[4].color = new Color(0, sliders[4].value, 0, 1);
        sliderFills[5].color = new Color(0, 0, sliders[5].value, 1);

        rgbValuesText[1].text = Mathf.RoundToInt(255 * sliders[3].value).ToString() + "   " + Mathf.RoundToInt(255 * sliders[4].value).ToString() + "   " + Mathf.RoundToInt(255 * sliders[5].value).ToString();
    }

    public void SaveCustomSkin()
    {
        gameManager.currentUser.customSkinColor[0] = sliders[0].value;
        gameManager.currentUser.customSkinColor[1] = sliders[1].value;
        gameManager.currentUser.customSkinColor[2] = sliders[2].value;
        gameManager.currentUser.customSkinColor[3] = sliders[3].value;
        gameManager.currentUser.customSkinColor[4] = sliders[4].value;
        gameManager.currentUser.customSkinColor[5] = sliders[5].value;

        gameManager.SaveUserData();
    }

    public void GetOneThousandGems()
    {
        gameManager.currentUser.gems += 1000;
        gameManager.currentUser.totalGemsCollected += 1000;
        RefreshGemsText();
        gameManager.SaveUserData();

		RefreshShop();
    }

    public void GetEverything()
    {
        foreach(var hat in itemManager.hats)
        {
            if(!gameManager.currentUser.ownedHats.Contains(hat.itemID))            
                gameManager.currentUser.ownedHats.Add(hat.itemID);
        }

        foreach(var gun in itemManager.guns)
        {
            if(!gameManager.currentUser.ownedGuns.Contains(gun.itemID))            
                gameManager.currentUser.ownedGuns.Add(gun.itemID);
        }

		foreach(var skin in itemManager.skins)
        {
            if(!gameManager.currentUser.ownedSkins.Contains(skin.itemID))            
                gameManager.currentUser.ownedSkins.Add(skin.itemID);
        }
		
        RefreshShop();
		gameManager.SaveUserData();
    }

	public void FlashGemsText()
	{
		AudioManager.Instance.PlaySound2D(tooExpensiveSound);
		StartCoroutine(FlashGemsTextCo());
	}

	IEnumerator FlashGemsTextCo()
	{
		for(int i = 0; i < 3; i++)
		{
			if(gameManager.currentUser.gems != 1)
				RefreshGemsText("<color=red>" + gameManager.currentUser.gems.ToString() + "</color>" + "<color=red>\nGems</color>");
			else
				RefreshGemsText("<color=red>" + gameManager.currentUser.gems.ToString() + "</color>" + "<color=red>\nGem</color>");

			yield return new WaitForSeconds(0.07f);

			if(gameManager.currentUser.gems != 1)
				RefreshGemsText("<color=yellow>" + gameManager.currentUser.gems.ToString() + "</color>" + "<color=white>\nGems</color>");
			else
				RefreshGemsText("<color=yellow>" + gameManager.currentUser.gems.ToString() + "</color>" + "<color=white>\nGem</color>");

			yield return new WaitForSeconds(0.07f);
		}
    }
}