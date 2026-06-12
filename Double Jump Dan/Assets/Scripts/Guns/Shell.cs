using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour, IPoolable
{
    [SerializeField] float force;
    [SerializeField] float rotationSpeed;
    [SerializeField] float lifetime = 4;
    [SerializeField] float fadetime = 2;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2D;
    float _lifeTime;
    TransformProperties properties;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnObjectReuse(object data)
    {
        rb2D.velocity = Vector2.zero;
        rb2D.angularVelocity = 0;
        
        properties = (TransformProperties)data;

        transform.position = properties.position;
        transform.rotation = properties.rotation;
        
        Vector3 direction = Quaternion.Euler(0, 0, Random.Range(75, 105)) * transform.right;

        _lifeTime = lifetime;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        rb2D.velocity = direction * force;

        if(rb2D.velocity.x < 0)
            rb2D.angularVelocity = rotationSpeed;
        else if(rb2D.velocity.x > 0)
            rb2D.angularVelocity = -rotationSpeed;

        StartCoroutine(Fade());
    }
    
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(_lifeTime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;

        while(percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(1, 0, percent));
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}