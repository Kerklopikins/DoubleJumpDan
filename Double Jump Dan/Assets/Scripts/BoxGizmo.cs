using UnityEngine;
public class BoxGizmo : MonoBehaviour
{
    [SerializeField] Vector3 boxOffset;
    [SerializeField] Vector2 boxSize;
    [SerializeField] Color boxColor = new Color(0, 0, 1, 1);

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        boxColor = new Color(boxColor.r, boxColor.g, boxColor.b, 1);

        Gizmos.color = boxColor;

        if(transform.localScale.x < 0)
            Gizmos.DrawWireCube(new Vector3(transform.position.x + -boxOffset.x, transform.position.y + boxOffset.y), boxSize);
        else if(transform.localScale.x > 0)
            Gizmos.DrawWireCube(new Vector3(transform.position.x + boxOffset.x, transform.position.y + boxOffset.y), boxSize);
    }
    #endif
}