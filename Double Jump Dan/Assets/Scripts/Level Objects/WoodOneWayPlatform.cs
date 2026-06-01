using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(OneWayPlatform))]
public class WoodOneWayPlatform : MonoBehaviour
{
    [SerializeField] bool editorMode;
    [Range(-8, 8)]
    [SerializeField] int length;

    [Header("Components")]
    [SerializeField] Sprite rightSprite;
    [SerializeField] Sprite leftSprite;
    [SerializeField] Sprite rightSupport;
    [SerializeField] Sprite leftSupport;
    [SerializeField] Vector2 startSizeSingle;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer supportSpriteRenderer;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] OneWayPlatform oneWayPlatform;

    int lastAdjustment;

    void Start()
    {
        SetSize();
    }

    void SetSize()
    {
        if(length > 0)
        {
            spriteRenderer.sprite = rightSprite;
            supportSpriteRenderer.sprite = rightSupport;

            if(length == 1)
                spriteRenderer.size = new Vector2(startSizeSingle.x, startSizeSingle.y);  
            else if(length > 1)
                spriteRenderer.size = new Vector2(startSizeSingle.x + length, startSizeSingle.y);

            oneWayPlatform.centerPointOffset = spriteRenderer.bounds.size.x / 2;
            boxCollider.offset = new Vector2(spriteRenderer.bounds.size.x / 2, 0);
        }
        else if(length < 0)
        {
            spriteRenderer.sprite = leftSprite;
            supportSpriteRenderer.sprite = leftSupport;

            if(length == -1)
                spriteRenderer.size = new Vector2(startSizeSingle.x, startSizeSingle.y);
            else if(length < -1)
                spriteRenderer.size = new Vector2(startSizeSingle.x + Mathf.Abs(length), startSizeSingle.y);

            oneWayPlatform.centerPointOffset = -spriteRenderer.bounds.size.x / 2;
            boxCollider.offset = new Vector2(-spriteRenderer.bounds.size.x / 2, 0);
        }

        if(length == 0)
        {
            spriteRenderer.size = new Vector2(0, startSizeSingle.y);
            boxCollider.size = new Vector2(0, spriteRenderer.size.y);
            supportSpriteRenderer.transform.localScale = new Vector2(0, 1);
            oneWayPlatform.centerPointOffset = 0;
        }
        else if(length != 0)
        {
            boxCollider.size = new Vector2(spriteRenderer.bounds.size.x, spriteRenderer.size.y);
            supportSpriteRenderer.transform.localScale = Vector2.one;
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if(!editorMode)
            return;

        if(length > 0)
        {
            if(length > 1)
                length = (int)Mathf.Round(length / 2) * 2;
        }
        else if(length < 0)
        {
            if(length < -1)
                length = (int)Mathf.Round(length / 2) * 2;
        }
        
        if(lastAdjustment != length)
            EditorApplication.delayCall += SetSize;

        lastAdjustment = length;
    }

    void OnDrawGizmos()
    {
        if(length != 0)
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f);
        
            int boxOffset = length > 0 ? 1 : -1;
            float platformBoxOffset = length > 0 ? spriteRenderer.size.x / 2 : -spriteRenderer.size.x / 2;
            
            Gizmos.DrawCube(transform.position + new Vector3(boxOffset, 0, 0), Vector3.one * 2);
            Gizmos.DrawCube(spriteRenderer.transform.position + new Vector3(platformBoxOffset, 0, 0), spriteRenderer.size);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.position, new Vector3(4, 2, 1));
        }
    }
    #endif
}