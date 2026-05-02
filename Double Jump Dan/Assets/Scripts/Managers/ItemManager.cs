using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour 
{
    public List<Item> hats = new List<Item>();
    public List<Item> guns = new List<Item>();
	public List<Item> skins = new List<Item>();

	Player player;
	Animator playerAnimator;
    Transform hatParent;
    Transform arm;
    GameManager gameManager;
	
	void Start()
	{
		gameManager = GameManager.Instance;
		
        if(SceneManager.GetActiveScene().name != "Main Menu")
        {
            player = LevelManager.Instance.player;
			
            playerAnimator = player.GetComponent<Animator>();
            hatParent = player.transform.Find("Legs").Find("Body");
            arm = player.transform.Find("Legs").Find("Body").Find("Arm 2");

			////Emoji Man Skin ID 8676
			if(gameManager.currentUser.skinID != 8676)
				foreach(var hat in hats)
					if(hat.itemID == gameManager.currentUser.hatID)
						InstantiateHat(hat);

            foreach(var gun in guns)
                if(gun.itemID == gameManager.currentUser.gunID)
					InstantiateGun(gun);

			foreach(var skin in skins)
				if(skin.itemID == gameManager.currentUser.skinID)
					ChangeSkin(skin);
        }
    }

	public void InstantiateGun(Item gun)
	{
		var gunGameObject = (GameObject)Instantiate(gun.gameObject, Vector3.zero, Quaternion.identity);
		gunGameObject.transform.parent = arm;
		gunGameObject.transform.localPosition = gun.transform.localPosition;
		gunGameObject.transform.localScale = CorrectedItemScale(gunGameObject.transform);
		gunGameObject.name = gun.gameObject.name.Replace("(Clone)", string.Empty);
		player.spriteMaterials.Add(gunGameObject.GetComponent<SpriteRenderer>());
		GunInfo gunInfo = gunGameObject.GetComponent<GunInfo>();
		player.aimPoint.localPosition = new Vector3(gunInfo.aimPointOffset, player.aimPoint.localPosition.y, 0);
		StatsHUD.Instance.SubscribeToGun(gunInfo);
		GameHUD.Instance.SubscribeToGun(gunInfo);
	}

	public void InstantiateHat(Item hat)
	{
		var hatGameObject = (GameObject)Instantiate(hat.gameObject, Vector3.zero, Quaternion.identity);
		hatGameObject.transform.parent = hatParent;
		hatGameObject.transform.localPosition = hat.transform.localPosition;
		hatGameObject.transform.localScale = CorrectedItemScale(hatGameObject.transform);
		hatGameObject.name = hat.gameObject.name.Replace("(Clone)", string.Empty);
		
		if(hat.itemID != 1111)
			player.spriteMaterials.Add(hatGameObject.GetComponent<SpriteRenderer>());
	}

	public void ChangeSkin(Item skin)
	{
		var skinAnimator = skin.gameObject.GetComponent<Animator>();
		playerAnimator.runtimeAnimatorController = skinAnimator.runtimeAnimatorController;

		//Custom skin
		if(skin.itemID == 9999)
		{
			Color bodyColor = new Color(gameManager.currentUser.customSkinColor[0], gameManager.currentUser.customSkinColor[1], gameManager.currentUser.customSkinColor[2]);
			Color armsAndLegsColor = new Color(gameManager.currentUser.customSkinColor[3], gameManager.currentUser.customSkinColor[4], gameManager.currentUser.customSkinColor[5]);
			
			player.spriteMaterials[0].color = bodyColor;
			
			for(int i = 1; i < 4; i++)
				player.spriteMaterials[i].color = armsAndLegsColor;
		}
	}

    public void EquipItem(ShopItem shopItem)
	{
		switch(shopItem.item.itemType)
		{   
			case Item.ItemType.Hat:
				gameManager.currentUser.hatID = shopItem.item.itemID;
				break;
			case Item.ItemType.Gun:
				gameManager.currentUser.gunID = shopItem.item.itemID;
				break;
			case Item.ItemType.Skin:
				gameManager.currentUser.skinID = shopItem.item.itemID;
				break;
		}
	}

	Vector3 CorrectedItemScale(Transform itemScale)
	{
		return new Vector3(itemScale.transform.localScale.x * player.transform.localScale.x, itemScale.transform.localScale.y, itemScale.transform.localScale.z);
	}
}