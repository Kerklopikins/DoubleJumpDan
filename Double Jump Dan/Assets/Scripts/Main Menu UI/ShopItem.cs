using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ShopItem : MonoBehaviour 
{
	public Item item;
	[SerializeField] Sprite normalItemBackground;
	[SerializeField] Sprite premiumItemBackground;
    [SerializeField] ShopManager shopManager;
	[SerializeField] UIScreenManager uIScreenManager;
	[SerializeField] Sprite[] fireRateSprites;
	[SerializeField] AudioClip equippedSound;
	
	Button equipButton;
	Text equipButtonText;
	GameManager gameManager;
	Image itemPicture;
	Text descriptionText;
    Image fireRateImage;
    Text fireModeText;
    RectTransform damageFillPivot;
    Text damageText;
	RectTransform rectTransform;
	Image image;
	ItemManager itemManager;
	Button buyButton;

	ConfirmPurchase confirmPurchase;
    Text priceText;
    Animator confirmPurchaseAnimator;
	bool checkedVisibilityAfterScaling;
	List<GameObject> children = new List<GameObject>();
	float lastStep;
	RectTransform contentHolder;
	RectTransform rect;

	void Awake()
	{
		gameManager = GameManager.Instance;
		itemManager = shopManager.itemManager;
		equipButton = GetComponentInChildren<Button>();
		equipButtonText = equipButton.GetComponentInChildren<Text>();
		rectTransform = GetComponent<RectTransform>();
		image = GetComponent<Image>();
		contentHolder = transform.parent.GetComponent<RectTransform>();
		rect = GetComponent<RectTransform>();
		equipButton.onClick.AddListener(OnEquipButtonClicked);
		shopManager.OnShopItemsChanged += Refresh;
		shopManager.OnShopTabsChanged += UpdateVisibility;

		for(int i = 0; i < transform.childCount; i++)
			children.Add(transform.GetChild(i).gameObject);

		if(item.itemID != 1111)
		{
			buyButton = equipButton.transform.Find("Buy Button").GetComponent<Button>();

			confirmPurchase = shopManager.confirmPurchase;
			priceText = buyButton.GetComponentInChildren<Text>();
			confirmPurchaseAnimator = confirmPurchase.GetComponent<Animator>();

			buyButton.onClick.AddListener(BuyButtonClicked);
		}
		
		if(gameObject.name != "None")
		{
			if(item == null)
				print("Item is null");
			if(item.premiumItem)
				gameObject.GetComponent<Image>().sprite = premiumItemBackground;
			else
				gameObject.GetComponent<Image>().sprite = normalItemBackground;

			itemPicture = transform.Find("Item Image").GetComponent<Image>();
			descriptionText = transform.Find("Description Text").GetComponent<Text>();
		}

		if(item.itemType == Item.ItemType.Gun)
		{
            fireRateImage = transform.Find("Fire Rate Image").GetComponent<Image>();
            fireModeText = transform.Find("Fire Mode Text").GetComponent<Text>();
            damageFillPivot = transform.Find("Damage").GetChild(0).GetComponent<RectTransform>();
            damageText = transform.Find("Damage Text").GetComponent<Text>();

            foreach(var _gun in itemManager.guns)
            {
                GunInfo gunInfo = _gun.GetComponent<GunInfo>();

                if(_gun.itemID == item.itemID)
                {
                    switch(gunInfo._fireRate)
                    {
                        case GunInfo.FireRate.ExtremelySlow:
                            fireRateImage.sprite = fireRateSprites[0];
                            break;
                        case GunInfo.FireRate.VerySlow:
                            fireRateImage.sprite = fireRateSprites[1];
                            break;
                        case GunInfo.FireRate.Slow:
                            fireRateImage.sprite = fireRateSprites[2];
                            break;
                        case GunInfo.FireRate.Normal:
                            fireRateImage.sprite = fireRateSprites[3];
                            break;
                        case GunInfo.FireRate.Fast:
                            fireRateImage.sprite = fireRateSprites[4];
                            break;
                        case GunInfo.FireRate.VeryFast:
                            fireRateImage.sprite = fireRateSprites[5];
                            break;
                        case GunInfo.FireRate.ExtremelyFast:
                            fireRateImage.sprite = fireRateSprites[6];
                            break;
                    }

					float damagePercent = (float)gunInfo.damage / 100;
                    damageFillPivot.localScale = new Vector3(damagePercent, 1, 1);
                    damageText.text = "Damage: " + gunInfo.damage;

                    if(gunInfo.fireMode == GunInfo.FireMode.Single)
                        fireModeText.text = " Fire Mode - Single";
                    else if(gunInfo.fireMode == GunInfo.FireMode.Automatic)
                        fireModeText.text = "Fire Mode - Automatic";
					else if(gunInfo.fireMode == GunInfo.FireMode.Burst)
                        fireModeText.text = "Fire Mode - Burst";
                }
            }
		}

		transform.Find("Title Text").GetComponent<Text>().text = transform.name;

		if(itemPicture != null)
			itemPicture.sprite = item.picture;

		if(descriptionText != null)
			descriptionText.text = item.description;
		
		UpdateVisibility();
		Refresh();
	}

    public void Refresh()
	{
		if(item.itemType == Item.ItemType.Hat)
		{
            if(gameManager.currentUser.hatID == item.itemID)
			{
				equipButton.interactable = false;
				shopManager.currentHatRect = rect;
			}
            else if(gameManager.currentUser.hatID != item.itemID)
				equipButton.interactable = true;

			if(buyButton != null)
			{
				if(gameManager.currentUser.ownedHats.Contains(item.itemID))
					buyButton.gameObject.SetActive(false);
				else
                	buyButton.gameObject.SetActive(true);
			}
		}

		if(item.itemType == Item.ItemType.Gun)
		{		
            if(gameManager.currentUser.gunID == item.itemID)
			{
				equipButton.interactable = false;
				shopManager.currentGunRect = rect;
			}
            else if(gameManager.currentUser.gunID != item.itemID)
				equipButton.interactable = true;

			if(buyButton != null)
			{
				if(gameManager.currentUser.ownedGuns.Contains(item.itemID))
					buyButton.gameObject.SetActive(false);
				else
					buyButton.gameObject.SetActive(true);
			}
		}

		if(item.itemType == Item.ItemType.Skin)
		{
			if(gameManager.currentUser.skinID == item.itemID)
			{
				equipButton.interactable = false;
				shopManager.currentSkinRect = rect;
			}
			else if(gameManager.currentUser.skinID != item.itemID)
				equipButton.interactable = true;

			if(buyButton != null)
			{
				if(gameManager.currentUser.ownedSkins.Contains(item.itemID))
					buyButton.gameObject.SetActive(false);
				else
                	buyButton.gameObject.SetActive(true);
			}
		}

		if(equipButton.interactable)
		{
			equipButtonText.text = "Equip";
			equipButtonText.color = Color.black;
		}
		else
		{
			equipButtonText.text = "Equipped";
			equipButtonText.color = Color.white;
		}
		
		if(item.itemID == 1111)
			return;

		if(gameManager.currentUser.gems >= item.price)
		{
			if(item.price > 0)
			{
				priceText.text = "<color=yellow>" + item.price.ToString() + "</color>" + "\nGems";
				buyButton.interactable = true;
			}
			else
			{
				priceText.text = "Free";
				buyButton.interactable = true;
			}
		}
		else
		{
			if(item.price > 0)
			{
				priceText.text = "<color=red>" + item.price.ToString() + "</color>" + "\nGems";
				buyButton.interactable = false;
			}
		}
	}
	public void OnEquipButtonClicked()
	{
		if(item.itemType == Item.ItemType.Gun)
            shopManager.currentGunRect = rect;

		if(item.itemType == Item.ItemType.Hat)
            shopManager.currentHatRect = rect;

		if(item.itemType == Item.ItemType.Skin)
			shopManager.currentSkinRect = rect;

		shopManager.EquipItem(this);
		AudioManager.Instance.PlaySound2D(equippedSound);
	}

	public void BuyButtonClicked()
	{
        if(gameManager.currentUser.gems >= item.price)
        {
            uIScreenManager.OpenMiniPanel(confirmPurchaseAnimator);
            confirmPurchase.shopItem = this;

			if(item.price > 0)
				confirmPurchase.Open(itemPicture, "Confirm purchase for " + item.gameObject.name + " for <color=yellow> " + item.price + " </color> gems?", item.premiumItem);
			else
				confirmPurchase.Open(itemPicture, "Confirm purchase for " + item.gameObject.name + " for free?", item.premiumItem);
        }
    }

	void Update()
	{
		int currentStep = Mathf.FloorToInt(contentHolder.anchoredPosition.x / 50);

		if(currentStep != lastStep)
		{
			lastStep = currentStep;
			UpdateVisibility();
		}
	}
	
	public void UpdateVisibility()
	{
		bool inRange;

		if(rectTransform.position.x < shopManager.minMaxVisibilityDistance.x || rectTransform.position.x > shopManager.minMaxVisibilityDistance.y)
			inRange = false;
		else
			inRange = true;
		
		bool wasVisible = image.enabled;

		foreach(var child in children)
			child.SetActive(inRange);

		image.enabled = inRange;

		if(inRange && !wasVisible)
			Refresh();
	}
}