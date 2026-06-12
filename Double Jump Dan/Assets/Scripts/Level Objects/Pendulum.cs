using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class Pendulum : MonoBehaviour
{   
    [Header("Editor Mode")]
    [SerializeField] bool editorMode;
    [SerializeField] float speed;
    [Range(4, 30)]
    [SerializeField] int length;
    [SerializeField] float phaseOffset;
    [SerializeField] float startRotation;
    [SerializeField] float endRotation;
    [SerializeField] Transform pivot;
	[SerializeField] SpriteRenderer middleSegmant;
    [SerializeField] Transform endSpike;
    [SerializeField] BoxCollider2D spikeCollider;
    
    int lastAdjustment;

    void Start()
    {
        SetSize();
    }

    void SetSize()
    {
        middleSegmant.size = new Vector2(middleSegmant.size.x, length);
        endSpike.transform.localPosition = new Vector2(0, -length + 1);

        spikeCollider.size = new Vector2(spikeCollider.size.x, middleSegmant.size.y + 2);
        spikeCollider.offset = new Vector2(0, -middleSegmant.size.y / 2 - 1);
    }
    
    void Update()
    {
        float t = Mathf.Sin(Time.time * speed + phaseOffset) * 0.5f + 0.5f;
        float angle = Mathf.Lerp(startRotation, endRotation, t);
        pivot.transform.localEulerAngles = new Vector3(0, 0, angle);
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
        float _length = length + 3;
        Vector3 offset = new Vector3(0, -_length * 0.5f, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y - 1), new Vector3(6, 2, 1));

        ////Segmant
        Gizmos.matrix = Matrix4x4.TRS(pivot.position, pivot.rotation, Vector3.one);
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(offset, new Vector3(0.75f, _length, 1));

        ////End part
        Gizmos.DrawCube(new Vector3(0, -_length + 1, 0), new Vector3(6, 2, 1));

        ////Start Rotation Gizmos
        ////Segmant
        Gizmos.matrix = Matrix4x4.TRS(pivot.position, Quaternion.Euler(0, 0, startRotation), Vector3.one);
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(offset, new Vector3(0.75f, _length, 1));

        ////End part
        Gizmos.DrawCube(new Vector3(0, -_length + 1, 0), new Vector3(6, 2, 1));

        ////End Rotation Gizmos
        ////Segmant
        Gizmos.matrix = Matrix4x4.TRS(pivot.position, Quaternion.Euler(0, 0, endRotation), Vector3.one);
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(offset, new Vector3(0.75f, _length, 1));

        ////End part
        Gizmos.DrawCube(new Vector3(0, -_length + 1, 0), new Vector3(6, 2, 1));
    }
    #endif
}