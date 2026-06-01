using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectableEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] SelectableType selectableType;
    [SerializeField] AudioClip buttonClick;
    public Color textStartingColor = Color.black;
    public Color disabledColor = Color.black;
    
    [SerializeField] bool hasText = true;
    [SerializeField] bool dontChangeText;

    public enum SelectableType { Button, Toggle }
    Text text;
    Selectable selectable;
    bool isPointerOver;
    bool isPointerDown;

    void Start()
    {
        if(hasText)
        {
            if(!dontChangeText)
            {
                if(GetComponentInChildren<Text>() != null)
                    text = GetComponentInChildren<Text>();
            }
        }
        
        selectable = GetComponent<Selectable>();
    }
    
    void OnEnable()
    {
        if(selectable == null)
            return;
            
        if(!selectable.interactable)
        {
            SetDisabled();
            return;
        }
        else
        {
            isPointerDown = false;
            isPointerOver = false;

            SetNormal();
        }
    }

    void Update()
    {
        if(!selectable.interactable)
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
        if(buttonClick != null && selectable.interactable)
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
}