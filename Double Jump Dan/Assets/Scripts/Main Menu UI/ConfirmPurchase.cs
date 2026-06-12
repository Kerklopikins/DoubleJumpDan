using UnityEngine;
using UnityEngine.UI;

public class ConfirmPurchase : MonoBehaviour 
{
    [SerializeField] Image itemImage;
    [SerializeField] Text confirmPurchaseText;
	[SerializeField] Image itemBackground;
	[SerializeField] Sprite normalItemBackground;
	[SerializeField] Sprite premiumItemBackground;
    [SerializeField] Sprite upgradeBackground;
    [SerializeField] Sprite customSkinItemBackground;

    public ShopItem shopItem { get; set; }
    GameManager gameManager;
    ShopManager shopManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        shopManager = ShopManager.Instance;
    }

	public void Open(Image _itemImage, string confirmPurchaseString, bool premiumItem, bool isUpgrade, bool isCustomSkin)
	{
        itemImage.rectTransform.sizeDelta = new Vector2(_itemImage.rectTransform.sizeDelta.x * 2, _itemImage.rectTransform.sizeDelta.y * 2);
        itemImage.sprite = _itemImage.sprite;
        confirmPurchaseText.text = confirmPurchaseString;

        if(isCustomSkin)
        {
            itemBackground.sprite = customSkinItemBackground;
            return;
        }

        if(isUpgrade)
        {
            itemBackground.sprite = upgradeBackground;
            return;
        }
        
		if(premiumItem)
			itemBackground.sprite = premiumItemBackground;
		else
			itemBackground.sprite = normalItemBackground;
	}
	
	public void BuyItem()
	{
        gameManager.currentUser.gems -= shopItem.item.price;

        if(shopItem.item.itemType == Item.ItemType.Hat)
            gameManager.currentUser.ownedHats.Add(shopItem.item.itemID);

        if(shopItem.item.itemType == Item.ItemType.Gun)
            gameManager.currentUser.ownedGuns.Add(shopItem.item.itemID);

		if(shopItem.item.itemType == Item.ItemType.Skin)
			gameManager.currentUser.ownedSkins.Add(shopItem.item.itemID);

        if(shopItem.item.itemType == Item.ItemType.Upgrade)
			gameManager.currentUser.ownedUpgrades.Add(shopItem.item.itemID);
		
        shopManager.RefreshGemsText();

        if(shopItem.item.itemType != Item.ItemType.Upgrade)
            shopManager.EquipItem(shopItem);
        else
            shopManager.EquipUpgrade(shopItem, true);
            
        shopManager.SaveShopData();
	}
}