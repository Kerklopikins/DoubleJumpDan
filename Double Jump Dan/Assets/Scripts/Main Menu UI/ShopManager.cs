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
    public ScrollRect upgradesScrollRect;

    public Vector2 minMaxVisibilityDistance;
    public ItemManager itemManager;
    [SerializeField] float itemWidth;
    [SerializeField] RectTransform gunsContent;
    [SerializeField] RectTransform hatsContent;
    [SerializeField] RectTransform skinsContent;
    [SerializeField] RectTransform upgradesContent;

    [SerializeField] Button gunsButton;
    [SerializeField] Button hatsButton;
    [SerializeField] Button skinsButton;
    [SerializeField] Button upgradesButton;

    [SerializeField] AudioClip shopTabSwitchSound;

    [Header("Custom Skin")]
    [SerializeField] Button changeSkinColorButton;
    [SerializeField] ColorPicker bodyColorPicker;
    [SerializeField] ColorPicker armsAndLegsColorPicker;
    [SerializeField] Image[] eyes;
    [SerializeField] Sprite[] eyeSprites;

    public event Action<bool> OnShopItemsChanged;
    public event Action OnShopTabsChanged;
    public RectTransform currentGunRect { get; set; }
    public RectTransform currentHatRect { get; set; }
    public RectTransform currentSkinRect { get; set; }
    CurrentShopTab currentShopTab = CurrentShopTab.Guns;
    public enum CurrentShopTab { Guns, Hats, Skins, Upgrades }
    GameManager gameManager;
    GameInputManager gameInputManager;
    MainMenuManager mainMenuManager;
    int shopTabIndex = 0;
    float shopTabTransitionTimer;
    UIScreenManager uIScreenManager;
    Animator shopAnimator;
    UIArea gunsArea;
    UIArea hatsArea;
    UIArea skinsArea;
    UIArea upgradesArea;
    ExitUIArea exitUIArea;
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
        gameInputManager.OnKeyboardOnlyInputChanged += OnKeyboardOnlyInputChanged;

        if(gameInputManager.ControllerConnected())
        {
            gunScrollRect.inertia = false;
            hatScrollRect.inertia = false;
            skinScrollRect.inertia = false;
        }
        
        changeSkinColorButton.onClick.AddListener(RefreshHexData);

        skinsButton.onClick.AddListener(SwitchToSkinTab);
        hatsButton.onClick.AddListener(SwitchToHatTab);
        gunsButton.onClick.AddListener(SwitchToGunTab);
        upgradesButton.onClick.AddListener(SwitchToUpgradeTab);

        gunsArea = gunsButton.GetComponent<UIArea>();
        hatsArea = hatsButton.GetComponent<UIArea>();
        skinsArea = skinsButton.GetComponent<UIArea>();
        upgradesArea = upgradesButton.GetComponent<UIArea>();

        gunsButton.interactable = false;
        exitUIArea = shop.GetComponent<ExitUIArea>();

        bodyColorPicker.Initialize();
        armsAndLegsColorPicker.Initialize();

        UpdateCustomSkinData(true);
        //UpdateCustomSkinBody();
        //UpdateCustomSkinArmsLegs();
        //gunsCount = itemManager.guns.Count - 1;
        //hatsCount = itemManager.hats.Count - 1;
        //skinsCount = itemManager.skins.Count - 1;
    }

    void Update()
    {        
        if(shop.activeInHierarchy == false)
            return;

        if(shopTabTransitionTimer > 0)
        {
            exitUIArea.CanExit = false;
            shopTabTransitionTimer -= Time.deltaTime;
        }
        else
        {
            exitUIArea.CanExit = true;
        }

        if(gameInputManager.ControllerConnected())
            ScrollContent(gameInputManager.ControllerScrolling(), gameInputManager.ControllerFastCursor());
        else if(gameInputManager.KeyboardOnly())
            ScrollContent(gameInputManager.KeyboardScrolling(), gameInputManager.KeyboardFastCursor());  
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
    
    void ScrollContent(Vector2 input, bool fastCursor)
    {
        if(uIScreenManager.currentOpenPanel == shopAnimator && uIScreenManager.transitionTimer <= 0)
        {
            if(Mathf.Abs(input.x) > 0.1f)
            {
                //scrollViewMoved = true;

                if(fastCursor)
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed * 2;
                else
                    mainMenuManager._scrollSpeed = mainMenuManager.scrollSpeed;

                switch(currentShopTab)
                {   
                    case CurrentShopTab.Guns:
                        gunsContent.anchoredPosition += new Vector2(-input.x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);
                        gunsContent.anchoredPosition = new Vector2(Mathf.Clamp(gunsContent.anchoredPosition.x, -gunsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), gunsContent.anchoredPosition.y);
                        break;
                    case CurrentShopTab.Hats:
                        hatsContent.anchoredPosition += new Vector2(-input.x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);      
                        hatsContent.anchoredPosition = new Vector2(Mathf.Clamp(hatsContent.anchoredPosition.x, -hatsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), hatsContent.anchoredPosition.y);
                        break;
                    case CurrentShopTab.Skins:
                        skinsContent.anchoredPosition += new Vector2(-input.x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);      
                        skinsContent.anchoredPosition = new Vector2(Mathf.Clamp(skinsContent.anchoredPosition.x, -skinsContent.sizeDelta.x + (itemWidth * 3) - 4, 0), skinsContent.anchoredPosition.y);
                        break;
                    case CurrentShopTab.Upgrades:
                        upgradesContent.anchoredPosition += new Vector2(-input.x * mainMenuManager._scrollSpeed * Time.deltaTime, 0);      
                        upgradesContent.anchoredPosition = new Vector2(Mathf.Clamp(upgradesContent.anchoredPosition.x, -upgradesContent.sizeDelta.x + (itemWidth * 3) - 4, 0), upgradesContent.anchoredPosition.y);
                        break;
                }
            }
            
            if(shopTabTransitionTimer <= 0)
            {
                if(gameInputManager.LeftBumperDown())
                {
                    shopTabIndex--;
                    
                    if(shopTabIndex < 0)
                        shopTabIndex = 3;

                    SwitchShopTabs();
                }
                
                if(gameInputManager.RightBumperDown())
                {                    
                    shopTabIndex++;
                    
                    if(shopTabIndex > 3)
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

    public void OnControllerChanged(bool enabled)
    {
        if(enabled)
        {
            gunScrollRect.inertia = false;
            hatScrollRect.inertia = false;
            skinScrollRect.inertia = false;
            upgradesScrollRect.inertia = false;
        }
        else
        {
            gunScrollRect.inertia = true;
            hatScrollRect.inertia = true;
            skinScrollRect.inertia = true;
            upgradesScrollRect.inertia = true;
        }
    }

    public void OnKeyboardOnlyInputChanged(bool keyboardOnly)
    {
        if(!gameInputManager.ControllerConnected())
        {
            if(keyboardOnly)
            {
                gunScrollRect.inertia = false;
                hatScrollRect.inertia = false;
                skinScrollRect.inertia = false;
                upgradesScrollRect.inertia = false;
            }
            else
            {
                gunScrollRect.inertia = true;
                hatScrollRect.inertia = true;
                skinScrollRect.inertia = true;
                upgradesScrollRect.inertia = true;
            }
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
                gunsButton.interactable = false;
                hatsButton.interactable = true;
                skinsButton.interactable = true;
                upgradesButton.interactable = true;

                if(currentShopTab != CurrentShopTab.Guns)
                {
                    currentShopTab = CurrentShopTab.Guns;

                    gunsArea.OpenArea(true);
                    hatsArea.OpenArea(false);
                    skinsArea.OpenArea(false);
                    upgradesArea.OpenArea(false);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
            break;
            case 1:
                gunsButton.interactable = true;
                hatsButton.interactable = false;
                skinsButton.interactable = true;
                upgradesButton.interactable = true;

                if(currentShopTab != CurrentShopTab.Hats)
                {
                    currentShopTab = CurrentShopTab.Hats;

                    gunsArea.OpenArea(false);
                    hatsArea.OpenArea(true);
                    skinsArea.OpenArea(false);
                    upgradesArea.OpenArea(false);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
            break;            
            case 2:
                gunsButton.interactable = true;
                hatsButton.interactable = true;
                skinsButton.interactable = false;
                upgradesButton.interactable = true;

                if(currentShopTab != CurrentShopTab.Skins)
                {
                    currentShopTab = CurrentShopTab.Skins;
                    
                    gunsArea.OpenArea(false);
                    hatsArea.OpenArea(false);
                    skinsArea.OpenArea(true);
                    upgradesArea.OpenArea(false);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    RefreshShopScrollRects();
                }
            break;
            case 3:
                gunsButton.interactable = true;
                hatsButton.interactable = true;
                skinsButton.interactable = true;
                upgradesButton.interactable = false;
                
                if(currentShopTab != CurrentShopTab.Upgrades)
                {
                    currentShopTab = CurrentShopTab.Upgrades;
                    
                    gunsArea.OpenArea(false);
                    hatsArea.OpenArea(false);
                    skinsArea.OpenArea(false);
                    upgradesArea.OpenArea(true);

                    AudioManager.Instance.PlaySound2D(shopTabSwitchSound);
                    ResetShopTabTransitionTimer();
                    //RefreshShopScrollRects();
                }
            break;
        }
    }

    public void SwitchToGunTab()
    {
        if(gunsButton.interactable && shopTabTransitionTimer <= 0)
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

    public void SwitchToSkinTab()
    {
        if(skinsButton.interactable && shopTabTransitionTimer <= 0)
        {
            shopTabIndex = 2;
            SwitchShopTabs();
        }
    }

    public void SwitchToUpgradeTab()
    {
        if(upgradesButton.interactable && shopTabTransitionTimer <= 0)
        {
            shopTabIndex = 3;
            SwitchShopTabs();
        }
    }

    public void EquipItem(ShopItem shopItem)
	{
        itemManager.EquipItem(shopItem);
        SetCustomSkinColorData();
        UpdateCustomSkinData(false);
        OnShopItemsChanged?.Invoke(false);
    }
    
    public void EquipUpgrade(ShopItem shopItem, bool equipped)
	{
        itemManager.EquipUpgrade(shopItem, equipped);
        OnShopItemsChanged?.Invoke(true);
    }

    public void RefreshShop()
    {
        RefreshGemsText();
        UpdateCustomSkinData(true);
        OnShopItemsChanged?.Invoke(false);  
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
            gemsText.text = "<color=yellow><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "\nGems";
		else
            gemsText.text = "<color=yellow><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "\nGem";
    }

    public void RefreshGemsText(string text)
    {
        gemsText.text = text;
    }

    void RefreshHexData()
    {
        bodyColorPicker.RefreshHexData();
        armsAndLegsColorPicker.RefreshHexData();
    }

    void UpdateCustomSkinData(bool updateColor)
    {
        //Custom skin
        if(gameManager.currentUser.skinID == 9999)
            changeSkinColorButton.interactable = true;
        else
            changeSkinColorButton.interactable = false;

        if(!updateColor)
            return;

        for(int i = 0; i < eyes.Length; i++)
            eyes[i].sprite = eyeSprites[(int)gameManager.currentUser.customSkinData[6]];
        
        bodyColorPicker.UpdateCurrentColor(gameManager.currentUser.customSkinData[0], gameManager.currentUser.customSkinData[1],gameManager.currentUser.customSkinData[2]);
        armsAndLegsColorPicker.UpdateCurrentColor(gameManager.currentUser.customSkinData[3], gameManager.currentUser.customSkinData[4],gameManager.currentUser.customSkinData[5]);
    }
    
    void SetCustomSkinColorData()
    {
        gameManager.currentUser.customSkinData[0] = bodyColorPicker.currentHue;
        gameManager.currentUser.customSkinData[1] = bodyColorPicker.currentSaturation;
        gameManager.currentUser.customSkinData[2] = bodyColorPicker.currentValue;
        gameManager.currentUser.customSkinData[3] = armsAndLegsColorPicker.currentHue;
        gameManager.currentUser.customSkinData[4] = armsAndLegsColorPicker.currentSaturation;
        gameManager.currentUser.customSkinData[5] = armsAndLegsColorPicker.currentValue;
    }

    public void ChangeEyes(int amount)
    {
        gameManager.currentUser.customSkinData[6] += amount;    
        
        if(amount > 0)
        {
            if(gameManager.currentUser.customSkinData[6] > eyeSprites.Length - 1)
                gameManager.currentUser.customSkinData[6] = 0;   
        }
        else if(amount < 0)
        {
            if(gameManager.currentUser.customSkinData[6] < 0)
                gameManager.currentUser.customSkinData[6] = eyeSprites.Length - 1;
        }

        for(int i = 0; i < eyes.Length; i++)
            eyes[i].sprite = eyeSprites[(int)gameManager.currentUser.customSkinData[6]];
    }

    public void SaveShopData()
    {
        SetCustomSkinColorData();
        gameManager.SaveUserData();
    }

    public void GetOneThousandGems()
    {
        gameManager.currentUser.gems += 1000;
        gameManager.currentUser.totalGemsCollected += 1000;
        RefreshGemsText();
        SaveShopData();

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

        foreach(var upgrade in itemManager.upgrades)
        {
            if(!gameManager.currentUser.ownedUpgrades.Contains(upgrade.itemID))            
                gameManager.currentUser.ownedUpgrades.Add(upgrade.itemID);
        }
		
        SaveShopData();
        RefreshShop();
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
				RefreshGemsText("<color=red><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "<color=red>\nGems</color>");
			else
				RefreshGemsText("<color=red><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "<color=red>\nGem</color>");

			yield return new WaitForSeconds(0.07f);

			if(gameManager.currentUser.gems != 1)
				RefreshGemsText("<color=yellow><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "<color=white>\nGems</color>");
			else
				RefreshGemsText("<color=yellow><size=28>" + gameManager.currentUser.gems.ToString() + "</size></color>" + "<color=white>\nGem</color>");

			yield return new WaitForSeconds(0.07f);
		}
    }
}