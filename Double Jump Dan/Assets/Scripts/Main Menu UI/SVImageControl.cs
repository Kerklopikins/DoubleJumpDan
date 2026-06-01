using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] Image colorPickerImage;
    [SerializeField] ColorPicker colorPicker;
    [SerializeField] RectTransform pickerTransform;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform rectTransform;
    float deltaX;
    float deltaY;
    float x;
    float y;
    float xNormalized;
    float yNormalized;
    
    public void Initialize()
    {
        deltaX = rectTransform.sizeDelta.x * 0.5f;
        deltaY = rectTransform.sizeDelta.y * 0.5f;
    }

    void UpdateColor(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, mainCanvas.worldCamera, out Vector2 localPosition);

        localPosition = new Vector2(Mathf.Clamp(localPosition.x, -deltaX, deltaX), Mathf.Clamp(localPosition.y, -deltaY, deltaY));
        UpdatePickerPositionAndColor(localPosition, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    void UpdatePickerPositionAndColor(Vector2 position, bool setSV)
    {
        x = position.x + deltaX;
        y = position.y + deltaY; 

        xNormalized = x / rectTransform.sizeDelta.x;
        yNormalized = y / rectTransform.sizeDelta.y;

        pickerTransform.anchoredPosition = position;

        ///Lerps from black to white based on it's normalized y position, it doesn't look the best to be honest
        //colorPickerImage.color = Color.HSVToRGB(0, 0, 1 - yNormalized);
        
        colorPickerImage.color = yNormalized > 0.5f ? Color.black : Color.white;
        
        if(setSV)
            colorPicker.SetSV(xNormalized, yNormalized);
    }

    public void SetColorPickerPosition(Vector2 normalizedPosition, Vector2 satValImageSize)
    {
        UpdatePickerPositionAndColor(new Vector2((normalizedPosition.x * satValImageSize.x) - satValImageSize.x * 0.5f, 
        (normalizedPosition.y * satValImageSize.y) - satValImageSize.y * 0.5f), false);
    }
}