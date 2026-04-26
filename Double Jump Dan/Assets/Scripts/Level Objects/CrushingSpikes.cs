using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class CrushingSpikes : MonoBehaviour
{
    [SerializeField] bool editorMode;
    [Header("Size and Direction and Movement Type")]
    [SerializeField] SizeDirection sizeDirection;
    [Range(1, 30)]
    [SerializeField] int length;

    [Header("Movement")]
    [SerializeField] float movementTime;
    [SerializeField] int endPoint;

    [Header("Movement Delays")]
    [SerializeField] float startDelay;
    [SerializeField] float pauseDuration;

    [Header("Components")]
    [SerializeField] Sprite horizontal;
    [SerializeField] Sprite single;
    [SerializeField] Sprite vertical;
    [SerializeField] Vector2 startSizeSingle;
    [SerializeField] SpriteRenderer spriteRenderer; 
    [SerializeField] BoxCollider2D spikeCollider;
    
    public enum MovementType { Lerp, Translate };
    public enum SizeDirection { Vertical, Horizontal };
    int lastAdjustment;
    Vector2 startPosition;
    int direction = 1;
    float pauseTimer;
    bool pausing;
    float rawT;

    void Start()
    {
        startPosition = transform.position;

        SetSize();
    }

    void Update()
    {
        if(startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            return;
        }
        
        if(pausing)
        {
            pauseTimer += Time.deltaTime;

            if(pauseTimer > pauseDuration)
            {
                pausing = false;
                pauseTimer = 0;
            }

            return;
        }

        rawT += (Time.deltaTime / movementTime) * direction;

        if(rawT > 1)
        {
            rawT = 1;
            direction = -1;
            pausing = true;
        }
        else if(rawT < 0)
        {
            rawT = 0;
            direction = 1;
            pausing = true;
        }

        float t = rawT * rawT * (3 - 2 * rawT);
        Vector2 offset;

        if(sizeDirection == SizeDirection.Horizontal)
            offset = transform.right * Mathf.Lerp(0, endPoint, t);
        else if(sizeDirection == SizeDirection.Vertical)
            offset = transform.up * Mathf.Lerp(0, endPoint, t);
        else
            offset = Vector2.zero;

        transform.position = startPosition + offset;
    }

    void SetSize()
    {
        if(length == 1)
        {
            spriteRenderer.sprite = single;
            spriteRenderer.size = new Vector2(startSizeSingle.x, startSizeSingle.y);
        }

        if(sizeDirection == SizeDirection.Horizontal)
        {
            if(length > 1)
            {
                spriteRenderer.sprite = horizontal;
                spriteRenderer.size = new Vector2(startSizeSingle.x + length, startSizeSingle.y);
            }
        }
        else if(sizeDirection == SizeDirection.Vertical)
        {
            if(length > 1)
            {
                spriteRenderer.sprite = vertical;
                spriteRenderer.size = new Vector2(startSizeSingle.x, startSizeSingle.y + length);
            }
        }

        spikeCollider.size = new Vector2(spriteRenderer.size.x - 0.5f, spriteRenderer.size.y - 0.5f);
    }

    void OnValidate()
    {
        if(!editorMode)
            return;
            
    #if UNITY_EDITOR
        if(length > 1)
            length = (int)Mathf.Round(length / 2) * 2;

        if(lastAdjustment != length)
            EditorApplication.delayCall += SetSize;
        
        lastAdjustment = length;
    #endif
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        
        if(Application.isPlaying)
        {
            Gizmos.DrawCube(startPosition, new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));

            if(sizeDirection == SizeDirection.Horizontal)
                Gizmos.DrawCube(new Vector3(startPosition.x + endPoint, startPosition.y, transform.position.z), new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));
            if(sizeDirection == SizeDirection.Vertical)
                Gizmos.DrawCube(new Vector3(startPosition.x, startPosition.y + endPoint, transform.position.z), new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));
        }
        else
        {
            Gizmos.DrawCube(transform.position, new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));

            if(sizeDirection == SizeDirection.Horizontal)
                Gizmos.DrawCube(new Vector3(transform.position.x + endPoint, transform.position.y, transform.position.z), new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));
            if(sizeDirection == SizeDirection.Vertical)
                Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y + endPoint, transform.position.z), new Vector3(spriteRenderer.size.x - 1.75f, spriteRenderer.size.y - 1.75f, 1));   
        }
#endif

    }
}
