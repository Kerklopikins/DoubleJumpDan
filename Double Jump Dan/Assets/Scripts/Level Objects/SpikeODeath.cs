using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpikeODeath : MonoBehaviour
{   
    [Header("Editor Mode")]
    [SerializeField] bool editorMode;

    [SerializeField] float movementTime;
    [Range(2, 30)]
    [SerializeField] int length;
    [SerializeField] float pauseDuration;
    [SerializeField] float startDelay;
	[SerializeField] SpriteRenderer bottomSegmant;
    [SerializeField] BoxCollider2D spikeCollider;

    Vector3 localStartPosition;
    Vector3 globalStartPosition;

    int direction = -1;
    float pauseTimer;
    bool pausing;
    float rawT;
    int lastAdjustment;

    void Start()
    {
		SetSize();
    }

    void SetSize()
    {
        localStartPosition = transform.localPosition;
        globalStartPosition = transform.position;

        if(length > 2)
        {
			bottomSegmant.gameObject.SetActive(true);
            bottomSegmant.size = new Vector2(bottomSegmant.size.x, length - 2);
		}
		else
		{
            bottomSegmant.gameObject.SetActive(false);

        }

		spikeCollider.size = new Vector2(spikeCollider.size.x, bottomSegmant.size.y);
		spikeCollider.offset = new Vector2(0, -bottomSegmant.size.y / 2);        
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
        Vector3 offset = transform.up * Mathf.Lerp(0, length, t);

        transform.localPosition = localStartPosition + offset;
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if(!editorMode)
            return;
            
        length = (int)Mathf.Round(length / 2) * 2;

        if(lastAdjustment != length)
            EditorApplication.delayCall += SetSize;
        
        lastAdjustment = length;
    }

    void OnDrawGizmos()
    {
        int _length = length + 2;

        if(Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(globalStartPosition, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);
        }

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 offset = new Vector3(0, _length * 0.5f - 1, 0);
        Gizmos.DrawCube(offset, new Vector3(1, _length, 1));
    }
    #endif
}