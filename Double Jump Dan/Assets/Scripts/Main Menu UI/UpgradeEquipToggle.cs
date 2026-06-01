using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeEquipToggle : MonoBehaviour, IPointerClickHandler
{
    public ShopItem shopItem { get; set; }
    public Toggle toggle { get; set; }

    public void OnPointerClick(PointerEventData eventData)
	{
        if(!toggle.interactable)
            return;
            
        shopItem.OnEquipToggleClicked(toggle.isOn);
    }
}