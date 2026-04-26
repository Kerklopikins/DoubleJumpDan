using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

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
    [SerializeField] RectTransform gunsContent;
    [SerializeField] RectTransform hatsContent;
    [SerializeField] RectTransform skinsContent;
    [SerializeField] float itemWidth;
    public Vector2 minMaxVisibilityDistance;
    public ItemManager itemManager;

    public event Action OnShopItemsChanged;
    public event Action OnShopTabsChanged;
    public RectTransform currentGunRect { get; set; }
    public RectTransform currentHatRect { get; set; }
    public RectTransform currentSkinRect { get; set; }
    public int currentItemIndex { get; set; }
    CurrentShopTab currentShopTab = CurrentShopTab.Guns;
    public enum CurrentShopTab { Guns, Hats, Skins }
    GameManager gameManager;
    GameInputManager gameInputManager;
    MainMenuManager mainMenuManager;
    int gunsCount;
    int hatsCount;
    int skinsCount;
    bool scrollViewMoved;

    void Awake()
    {
        Instance = this;       
    }
    void Start()
    {
        gameManager = GameManager.Instance;
        gameInputManager = GameInputManager.Instance;
        mainMenuManager = GetComponent<MainMenuManager>();

        gunsCount = itemManager.guns.Count - 1;
        hatsCount = itemManager.hats.Count - 1;
        skinsCount = itemManager.skins.Count - 1;
    }

    void Update()
    {
        /////////////////////////////////
        /// MAKE BUMPERS SO YOU CAN HOLD
        /// |THEM
        /// Make cursor actually dissapear
        /// Make screenshots Scroll
        /// Add controller sensitivity to settigs
        /// Make cursor not move when pressing keyboard arrows
        /// 
        ///
        ///
        /// 
        
        if(shop.activeSelf == false)
            return;

        if(gameInputManager.ControllerConnected())
        {
            if(Mathf.Abs(gameInputManager.AimDirection().x) > 0.1f)
            {
                scrollViewMoved = true;

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

            if(gameInputManager.LeftBumperDown())
            {
                if(!scrollViewMoved)
                {               
                    currentItemIndex--;
                }
                else
                {
                    CenterScrollRectsAndUpdateScrollIndex();
                    scrollViewMoved = false;
                }

                switch(currentShopTab)
                {   
                    case CurrentShopTab.Guns:
                        ScrollItems(gunsCount, gunsContent);          
                        break;
                    case CurrentShopTab.Hats:
                        ScrollItems(hatsCount, hatsContent);                    
                        break;
                    case CurrentShopTab.Skins:
                        ScrollItems(skinsCount, skinsContent);                   
                        break;
                }
            }
            else if(gameInputManager.RightBumperDown())
            {
                if(!scrollViewMoved)
                {               
                    currentItemIndex++;
                }
                else
                {
                    CenterScrollRectsAndUpdateScrollIndex();
                    scrollViewMoved = false;
                }

                switch(currentShopTab)
                {   
                    case CurrentShopTab.Guns:
                        ScrollItems(gunsCount, gunsContent);          
                        break;
                    case CurrentShopTab.Hats:
                        ScrollItems(hatsCount, hatsContent);                    
                        break;
                    case CurrentShopTab.Skins:
                        ScrollItems(skinsCount, skinsContent);                   
                        break;
                }
            }
        }
    }   

    void ScrollItems(int itemCount, RectTransform content)
    {            
        currentItemIndex = Mathf.Clamp(currentItemIndex, 0, itemCount - 2);
        content.anchoredPosition = new Vector2(-(currentItemIndex * itemWidth), content.anchoredPosition.y);      
    }
    
    public void UpdateScrollIndex(int index, int itemCount)
    {
        currentItemIndex = index - 2;
        currentItemIndex = Mathf.Clamp(currentItemIndex, 0, itemCount - 2);
    }

    public void EquipItem(ShopItem shopItem)
	{
        itemManager.EquipItem(shopItem);
        OnShopItemsChanged?.Invoke();
    }

    public void RefreshShop()
    {
        RefreshGemsText();
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
        
        CenterScrollRectsAndUpdateScrollIndex();
    }
    
    void CenterScrollRectsAndUpdateScrollIndex()
    {
        switch(currentShopTab)
        {   
            case CurrentShopTab.Guns:
                CenterShopScrollRect(gunScrollRect, currentGunRect);
                UpdateScrollIndex(currentGunRect.transform.GetSiblingIndex() + 1, gunsCount);
                break;
            case CurrentShopTab.Hats:
                CenterShopScrollRect(hatScrollRect, currentHatRect);
                UpdateScrollIndex(currentHatRect.transform.GetSiblingIndex() + 1, hatsCount);
                break;
            case CurrentShopTab.Skins:
                CenterShopScrollRect(skinScrollRect, currentSkinRect);
                UpdateScrollIndex(currentSkinRect.transform.GetSiblingIndex() + 1, skinsCount);
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
    
    public void SwitchToGunTab()
    {
        currentShopTab = CurrentShopTab.Guns;
        RefreshShopScrollRects();
    }

    public void SwitchToSkinTab()
    {
        currentShopTab = CurrentShopTab.Skins;
        RefreshShopScrollRects();
    }

    public void SwitchToHatTab()
    {
        currentShopTab = CurrentShopTab.Hats;
        RefreshShopScrollRects();
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