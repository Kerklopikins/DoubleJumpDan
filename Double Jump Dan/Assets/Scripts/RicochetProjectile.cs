using UnityEngine;

public class RicochetProjectile : MonoBehaviour, IPoolable
{
    [Header("Main Stuff")]
    [SerializeField] int maxBounces = 3;
    [SerializeField] bool destroyWhenInvisible;
    [SerializeField] SpriteRenderer mainSprite;

    [Header("Effects")]
    [SerializeField] AudioClip[] destroyedSounds;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    float speed;
    float lifeTime;
    Vector2 targetPoint;
    Vector2 currentDirection;
    int bounceCount;
    string destroyedEffectPool;
    GiveDamage giveDamage;
    RaycastHit2D lastHit;

    void Awake()
    {
        giveDamage = GetComponent<GiveDamage>();
    }

    void Initialize(Vector2 startPosition, Vector2 direction)
    {
        currentDirection = direction.normalized;
        SetNextTarget(startPosition, currentDirection);
    }
    public void OnObjectReuse(object data)
    {
        giveDamage.hit = false;
        bounceCount = 0;

        RicochetProperties ricochetProperties = (RicochetProperties)data;

        giveDamage.damageToGive = ricochetProperties.damage;
        lifeTime = ricochetProperties.lifeTime;
        transform.position = ricochetProperties.position;
        transform.rotation = ricochetProperties.rotation;
        transform.localScale = ricochetProperties.scale;
        destroyedEffectPool = ricochetProperties.destroyedEffectPool;
        currentDirection = ricochetProperties.direction;

        speed = Mathf.Abs(ricochetProperties.speed);
        
        Initialize(transform.position, currentDirection);

    }

    void Update()
    {
        if(mainSprite != null && !mainSprite.isVisible)
            if(destroyWhenInvisible)
                gameObject.SetActive(false);

        if((lifeTime -= Time.deltaTime) <= 0)
            gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if(giveDamage.hit)
            DestroyProjectile(destroyedEffectPool);
       
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
    
        if(((Vector2)transform.position == targetPoint))
        {
            if(bounceCount < maxBounces && lastHit.collider != null)
                Bounce();
            else
                DestroyProjectile(destroyedEffectPool);
        }
    }

    void SetNextTarget(Vector2 from, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(from, direction, Mathf.Infinity, 1 << LayerMask.NameToLayer("Collisions"));

        if(hit.collider != null)
        {
            targetPoint = hit.point;
            lastHit = hit;
        }
        else
        {
            targetPoint = from + direction * 200;
            lastHit = default;    
        }

    }

    void Bounce()
    {
        bounceCount++;

        Vector2 reflectedDirection = Vector2.Reflect(currentDirection, lastHit.normal);
        currentDirection = reflectedDirection;

        transform.right = transform.localScale.x > 0 ? currentDirection : -currentDirection;
        Vector2 newStart = lastHit.point + lastHit.normal * 0.01f;
        SetNextTarget(newStart, currentDirection);
    }

    void DestroyProjectile(string poolName)
    {
        if(properties.strength > 0)
            CameraManager.Instance.Shake(properties);

        if(destroyedSounds.Length > 0)
            AudioManager.Instance.PlaySound2D(destroyedSounds[Random.Range(0, destroyedSounds.Length)]);
        
        TransformProperties transformProperties = new TransformProperties();
        transformProperties.position = transform.position;
        transformProperties.scale = new Vector3(-transform.lossyScale.x, 1, 1);
        transformProperties.rotation = transform.rotation;

        PoolManager.Instance.ReuseObject(destroyedEffectPool, transformProperties);

        gameObject.SetActive(false);
    }
}

public struct RicochetProperties
{
    public float speed;
    public int damage;
    public float lifeTime;
    public Vector2 position;
    public Quaternion rotation;
    public Vector2 scale;
    public Vector2 direction;
    public string destroyedEffectPool;
}