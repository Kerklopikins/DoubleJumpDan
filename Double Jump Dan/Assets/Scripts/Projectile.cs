using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [Header("Main Stuff")]
    [SerializeField] ShootThrough shootThrough;
    [SerializeField] bool destroyWhenInvisible;
    [SerializeField] bool rotateWithTrajectory;
    [SerializeField] SpriteRenderer mainSprite;

    [Header("Effects")]
    [SerializeField] AudioClip[] destroyedSounds;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    public enum ShootThrough { ShootThroughNothing, ShootThroughEnemies, ShootThroughEverything };
    public float speed { get; set; }
    public float lifeTime = 50;
    public bool useRayCast { get; set; }
    Collider2D _collider2D;
    Vector2 projectileTargetPoint;
    Rigidbody2D rb2D;
    string destroyedEffectPool;
    GiveDamage giveDamage;
    ProjectileProperties projectileProperties;
    TransformProperties transformProperties;
    
    void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        giveDamage = GetComponent<GiveDamage>();
    }

    public void OnObjectReuse(object data)
    {
        giveDamage.hit = false;
        
        projectileProperties = (ProjectileProperties)data;

        giveDamage.damageToGive = projectileProperties.damage;
        lifeTime = projectileProperties.lifeTime;
        transform.position = projectileProperties.position;
        transform.rotation = projectileProperties.rotation;
        transform.localScale = projectileProperties.scale;
        useRayCast = projectileProperties.useRaycast;
        destroyedEffectPool = projectileProperties.destroyedEffectPool;

        SetEndPoint(projectileProperties.targetPoint);

        if(!useRayCast)
            speed = projectileProperties.speed;
        else
            speed = Mathf.Abs(projectileProperties.speed);

        if(rotateWithTrajectory)
        {
            float speedAbs = Mathf.Abs(projectileProperties.speed);
            rb2D.velocity = transform.right * speedAbs * transform.localScale.x;
        }
    }

    void Update()
    {
        if(mainSprite != null && !mainSprite.isVisible)
            if(destroyWhenInvisible)
                gameObject.SetActive(false);

        if((lifeTime -= Time.deltaTime) <= 0)
            gameObject.SetActive(false);
        
        if(rotateWithTrajectory)
        {
            if(rb2D.velocity.sqrMagnitude < 0.3f * 0.3f)
                return;

            float angle = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg;

            if(transform.localScale.x == 1)
                transform.rotation = Quaternion.Euler(0, 0, angle);
            else
                transform.rotation = Quaternion.Euler(0, 0, angle + 180);
        }
    }

    public void SetEndPoint(Vector2 targetPoint)
    {
        projectileTargetPoint = targetPoint;

    }

    void FixedUpdate()
    {
        if(shootThrough == ShootThrough.ShootThroughNothing)
        {
            if(giveDamage.hit)
                DestroyProjectile(destroyedEffectPool);

            if(!useRayCast)
                if(_collider2D.IsTouchingLayers(1 << LayerMask.NameToLayer("Collisions")))
                    DestroyProjectile(destroyedEffectPool);
        }
        else if(shootThrough == ShootThrough.ShootThroughEnemies)
        {
            if(!useRayCast)
                if(_collider2D.IsTouchingLayers(1 << LayerMask.NameToLayer("Collisions")))
                    DestroyProjectile(destroyedEffectPool);
        }

        if(useRayCast)
        {
            transform.position = Vector2.MoveTowards(transform.position, projectileTargetPoint, speed * Time.deltaTime);
            
            if((Vector2)transform.position == projectileTargetPoint)
                DestroyProjectile(destroyedEffectPool);
        }
        else
        {
            if(!rotateWithTrajectory)
                transform.Translate(new Vector2(speed * Time.deltaTime, 0), Space.Self);
        }
    }

    void DestroyProjectile(string poolName)
    {
        if(properties.strength > 0)
            CameraManager.Instance.Shake(properties);

        if(destroyedSounds.Length > 0)
            AudioManager.Instance.PlaySound2D(destroyedSounds[Random.Range(0, destroyedSounds.Length)]);
        
        transformProperties.position = transform.position;
        transformProperties.scale = new Vector3(-transform.lossyScale.x, 1, 1);
        transformProperties.rotation = transform.rotation;

        if(!string.IsNullOrEmpty(destroyedEffectPool))
            PoolManager.Instance.ReuseObject(destroyedEffectPool, transformProperties);

        gameObject.SetActive(false);
    }
}