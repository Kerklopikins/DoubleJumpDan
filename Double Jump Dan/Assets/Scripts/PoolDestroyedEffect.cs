using System.Collections;
using UnityEngine;

public class PoolDestroyedEffect: MonoBehaviour, IPoolable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] animationSprites;
    [SerializeField] float animationSpeed;

    TransformProperties properties;
    
    public void OnObjectReuse(object data)
    {
        properties = (TransformProperties)data;
        transform.position = properties.position;
        transform.localScale = properties.scale;
        transform.rotation = properties.rotation;

        StartCoroutine(AnimateDestroyedEffect());
    }

    IEnumerator AnimateDestroyedEffect()
    {
        for(int i = 0; i < animationSprites.Length; i++)
        {
            spriteRenderer.sprite = animationSprites[i];
            yield return new WaitForSeconds(animationSpeed);
        }

        gameObject.SetActive(false);
    }
}