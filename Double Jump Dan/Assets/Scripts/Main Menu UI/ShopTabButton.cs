using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] AudioClip buttonClick;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite highlightedSprite;
    [SerializeField] Sprite disabledSprite;
    Button button;
    bool isPointerOver;
    bool isPointerDown;
    Image buttonImage;
    Text text;
    
	void Start() 
	{
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();        
        text = GetComponentInChildren<Text>();
	}
    
    void Update()
    {
        if(!button.interactable)
        {
            SetDisabled();
            return;
        }

        if(isPointerDown && isPointerOver)
            SetPressed();
        else if(isPointerOver)
            SetHighlighted();
        else if(!isPointerOver && isPointerDown)
            SetPressed();
        else
            SetNormal();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(buttonClick != null && button.interactable)
            AudioManager.Instance.PlaySound2D(buttonClick);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {       
        isPointerDown = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    void SetNormal()
    {
        buttonImage.sprite = normalSprite;
        text.color = Color.black;
    }

    void SetHighlighted()
    {
        buttonImage.sprite = highlightedSprite;
        text.color = Color.white;
    }

    void SetPressed()
    {
        buttonImage.sprite = highlightedSprite;
        text.color = Color.white;
    }

    void SetDisabled()
    {
        buttonImage.sprite = disabledSprite;
        text.color = Color.white;
    }
}