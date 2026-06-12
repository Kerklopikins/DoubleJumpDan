using System.Collections;
using UnityEngine;

public class WoodedCrate : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] pieces;
    [SerializeField] Vector2 flingHeightMinMax;
    [SerializeField] Vector2 flingEndMinMax;
    [SerializeField] Vector2 durationMinMax;
    [SerializeField] int arcYAmount = 2;
    [SerializeField] float rotationSpeed = 1000;
    SpriteRenderer spriteRenderer;
    Collider2D _collider;
    Health health;
    bool initiated;
    
    void Start()
    {
        health = GetComponent<Health>();  
        spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();  
    }

    void Update()
    {
        if(health.Dead() && !initiated)
        {
            spriteRenderer.enabled = false;
            _collider.enabled = false;

            for(int i = 0; i < pieces.Length; i++)
            {
                pieces[i].gameObject.SetActive(true);
                StartCoroutine(ExplodeCrate(pieces[i].transform));
            }

            initiated = true;
        }       
    }

    IEnumerator ExplodeCrate(Transform piece)
    {
        StartCoroutine(ScalePiecesOverTime(piece));

        float direction = Random.value > 0.5f ? 1 : -1;
        Vector3 startPosition = piece.position;
        float flingHeight = Random.Range(flingHeightMinMax.x, flingHeightMinMax.y);

        Vector3 flingEnd = startPosition + new Vector3(direction * Random.Range(flingEndMinMax.x, flingEndMinMax.y), -10, 0);
        
        float flingDuration = Random.Range(durationMinMax.x, durationMinMax.y);
        float elapsed = 0;

        float previousY = 0, previousY2 = 0;
        float previousX = 0, previousX2 = 0;
        float previousDeltaTime = 0, previousDeltaTime2 = 0;

        while(elapsed < flingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flingDuration);

            float easedT = 1 - (1 - t) * (1 - t);
            float arcY = (1 - Mathf.Pow(t * 2 - 1, arcYAmount)) * flingHeight;

            Vector3 position = Vector3.Lerp(startPosition, flingEnd, easedT);
            
            position.y = startPosition.y + arcY + Mathf.Lerp(0, flingEnd.y - startPosition.y, t * t);
            previousY2 = previousY;
            previousX2 = previousX;
            previousDeltaTime2 = previousDeltaTime;

            previousY = piece.position.y;
            previousX = piece.position.x;
            previousDeltaTime = Time.deltaTime;

            piece.position = position;
            piece.Rotate(0, 0, -direction * rotationSpeed * Time.deltaTime);

            yield return null;
        }

        float velocityY1 = Time.deltaTime > 0 ? (piece.position.y - previousY) / Time.deltaTime : 0;
        float velocityY2 = previousDeltaTime2 > 0 ? (previousY - previousY2) / previousDeltaTime2 : 0;
        
        float initialVelocityY = (velocityY1 + velocityY2) / 2;

        float velocityX1 = Time.deltaTime > 0 ? (piece.position.x - previousX) / Time.deltaTime : 0;
        float velocityX2 = previousDeltaTime2 > 0 ? (previousX - previousX2) / previousDeltaTime2 : 0;
        float initialVelocityX = (velocityX1 + velocityX2) / 2;

        float gravity = -18;
        float fallElapsed = 0;
        float fallDuration = 0.5f;
        Vector3 landedPosition = piece.position;

        while(fallElapsed < fallDuration)
        {
            fallElapsed += Time.deltaTime;
            float yOffset = initialVelocityY * fallElapsed + 0.5f * gravity * fallElapsed * fallElapsed;
            float xOffset = initialVelocityX * fallElapsed;

            piece.position = new Vector3(landedPosition.x + xOffset, landedPosition.y + yOffset, landedPosition.z);
            piece.Rotate(0, 0, -direction * rotationSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(3);
        
        Destroy(gameObject);
    }

    IEnumerator ScalePiecesOverTime(Transform piece)
    {
        float duration = Random.Range(durationMinMax.x * 1.5f, durationMinMax.y * 1.5f);
        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            float t = Mathf.Clamp01(inTime / duration);
            float easedT = 1 - (1 - t) * (1 - t);

            piece.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, easedT);
            yield return null;
        }
    }
}
