using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] AudioClip buttonClick;
    public Color textStartingColor = Color.black;
    public Color disabledColor = Color.black;
    [SerializeField] bool dontChangeText;
    [SerializeField] EventSystem eventSystem;
    
    Text text;
    Button button;
    bool isPointerOver;
    bool isPointerDown;
    bool discontinueInput;
    
    void Start()
    {
        if(!dontChangeText)
        {
            if(GetComponentInChildren<Text>() != null)
                text = GetComponentInChildren<Text>();
        }

        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        isPointerOver = false;
    }

    void Update()
    {
        if(discontinueInput)
            return;

        if(eventSystem != null)
        {
            if(eventSystem.gameObject.activeSelf == false)
            {
                button.enabled = false;
                SetNormal();
                return;
            }
        }

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

    public void SetNormal()
    {
        if(!dontChangeText && text != null)
            text.color = textStartingColor;
    }

    public void SetHighlighted()
    {
        if(!dontChangeText && text != null)
            text.color = Color.white;
    }

    public void SetPressed()
    {
        if(!dontChangeText && text != null)
            text.color = Color.white;
    }

    public void SetDisabled()
    {
        if(!dontChangeText && text != null)
            text.color = disabledColor;
    }

    public void DiscontinueInput(bool _discontinueInput)
    {
        discontinueInput = _discontinueInput;
    }
}