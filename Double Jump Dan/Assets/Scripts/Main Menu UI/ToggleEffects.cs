using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleEffects : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AudioClip toggleClick;

    Toggle toggle;

    void Start()
    {	
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(toggleClick != null && toggle.interactable)
            AudioManager.Instance.PlaySound2D(toggleClick);
    }
}