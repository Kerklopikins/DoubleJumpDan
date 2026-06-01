using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class BuzzSaw : MonoBehaviour
{
    [Header("Size")]
    [Range(1, 2)]
    [SerializeField] int size;

    [Header("Path Settings")]
    [SerializeField] List<Vector2> points = new List<Vector2>();
    [SerializeField] bool loop;

    [Header("Movement Type")]
    [SerializeField] MovementType movementType;

    [Header("Translate Speed")]
    [SerializeField] float speed = 1;

    [Header("Lerp Movement Time")]
    [SerializeField] float movementTime;

    [Header("Components")]
    [SerializeField] SpriteRenderer sawSprite;
    [SerializeField] Sprite[] sawSprites;
    [SerializeField] CircleCollider2D _collider;

    public enum MovementType { Lerp, Translate }
    Vector2 currentPoint;
    int pointIndex;
    int direction = 1;
    float rawT;
    int lastAdjustment;

    void Start()
    {
        currentPoint = points[0];
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, -1000 * Time.deltaTime);
        
        if(movementType == MovementType.Translate)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentPoint, speed * Time.deltaTime);

            if((Vector2)transform.position == currentPoint)
            {
                if(loop)
                {
                    if(pointIndex == points.Count)
                        pointIndex = -1;
                }
                else
                {
                    if(pointIndex == points.Count)
                        direction = -1;
                    else if(pointIndex <= 0)
                        direction = 1;
                }

                pointIndex += direction;

                if(pointIndex > points.Count - 1)
                    return;

                currentPoint = points[pointIndex];
            }
        }
        if(movementType == MovementType.Lerp)
        {
            rawT += (Time.deltaTime / movementTime) * direction;

            if((Vector2)transform.position == currentPoint)
            {
                rawT = 0;

                if(loop)
                {
                    if(pointIndex == points.Count)
                        pointIndex = -1;
                }
                else
                {
                    if(pointIndex == points.Count)
                        direction = -1;
                    else if(pointIndex <= 0)
                        direction = 1;
                }
                
                pointIndex += direction;

                if(pointIndex > points.Count - 1)
                    return;
                
                currentPoint = points[pointIndex];
            }

            if(Time.timeScale == 0)
                return;
            
            float t = rawT * rawT * (3 - 2 * rawT);
            transform.position = Vector3.Lerp(transform.position, currentPoint, t);
        }
    }
    
    void Resize()
    {
        if(sawSprite == null)
            return;

        if(size == 1)
        {
            sawSprite.sprite = sawSprites[0];     
            _collider.radius = 0.8f;       
        }
        else if(size == 2)
        {
            sawSprite.sprite = sawSprites[1];
            _collider.radius = 1.6f;
        }
    }

    Vector2 SnapVector(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if(lastAdjustment != size)
            EditorApplication.delayCall += Resize;

        lastAdjustment = size;

        if(points == null)
            return;
        
        for(int i = 0; i < points.Count; i++)
        {
            points[i] = SnapVector(points[i]);

            if(points[i] == Vector2.zero)
                points[i] = new Vector2(transform.position.x, transform.position.y);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 boxSize;
        Vector2 guiOffset;

        if(size == 1)
        {
            guiOffset = Vector2.up * 2;
            boxSize = Vector3.one * 2;
        }
        else if(size == 2)
        {
            guiOffset = Vector2.up * 4;
            boxSize = new Vector3(4, 4, 1);
        }
        else
        {
            guiOffset = Vector2.zero;
            boxSize = Vector3.zero;
        }

        for(int i = 0; i < points.Count; i++)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(new Vector3(points[i].x, points[i].y, 0), boxSize);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;

            if(i == 0)
                Handles.Label(points[i] + guiOffset, "Start Point", style);
            else if(i == points.Count - 1)
                Handles.Label(points[i] + guiOffset, "End Point", style);
            else
                Handles.Label(points[i] + guiOffset, "Point (" + i + ")", style);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
        
        for(int i = 0; i < points.Count - 1; i++)
            Gizmos.DrawLine(new Vector3(points[i].x, points[i].y, 0), new Vector3(points[i + 1].x, points[i + 1].y, 0));

        if(loop && points.Count > 0)
            Gizmos.DrawLine(new Vector3(points[points.Count - 1].x, points[points.Count - 1].y, 0), new Vector3(points[0].x, points[0].y, 0));
    }
    #endif
}