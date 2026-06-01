using UnityEngine;
using UnityEngine.UI;

public class ButtonFlashAnimation : MonoBehaviour
{
    [Header("Flash Duration")]
    [SerializeField] float flashDuration;

    [Header("Main Components")]
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite flashSprite;

    [Header("Optional Text")]
    [SerializeField] Text text;

    float flashTimer;
    bool isFlashed;

    void Update()
    {
        if(flashTimer > 0)
        {
            flashTimer -= Time.unscaledDeltaTime;    
        }
        else
        {
            isFlashed = !isFlashed;

            buttonImage.sprite = !isFlashed ? normalSprite : flashSprite;

            if(text != null)
                text.color = !isFlashed ? Color.black : Color.white;

            flashTimer = flashDuration;
        }
    }
}