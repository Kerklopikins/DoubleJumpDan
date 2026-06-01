using UnityEngine;

public class SkinColorAndEyes : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] customSkinSprites;
    [SerializeField] Sprite[] eyeSprites;
    [SerializeField] Sprite[] pupilSprites;
    [SerializeField] SpriteRenderer eyes;
    [SerializeField] SpriteRenderer pupils;
    [SerializeField] SpriteRenderer eyeBrow;

    public void SetEyes(int index, bool setOnlyPupils)
    {
        if(!setOnlyPupils)
        {
            eyes.sprite = eyeSprites[index];
            eyes.gameObject.SetActive(true);
        }
        
        pupils.gameObject.SetActive(true);
        pupils.sprite = pupilSprites[index];
    }

    public void SetColor(int index, Color color)
    {
        customSkinSprites[index].color = color;
    }

    public void DisablePupils()
    {
        pupils.gameObject.SetActive(false);
    }
    
    public void SetEyebrow(int index)
    {
        eyeBrow.gameObject.SetActive(index > 0 ? true : false);
    }
}