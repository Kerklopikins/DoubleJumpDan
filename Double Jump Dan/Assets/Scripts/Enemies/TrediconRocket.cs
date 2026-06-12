using System.Collections;
using UnityEngine;

public class TrediconRocket: MonoBehaviour, IPoolable
{
    [Header("Main Stuff")]
    [SerializeField] bool destroyWhenInvisible;
    [SerializeField] ParticleSystem[] particleEffects;
    [SerializeField] float wobbleAmount;
    [SerializeField] float wobbleSpeed;
    [SerializeField] Transform rocketSprite;

    [Header("Effects")]
    [SerializeField] AudioClip destroyedSound;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    float speed;
    float lifeTime;
    Collider2D _collider2D;
    string destroyedEffectPool;
    GiveDamage giveDamage;
    bool hit;
    TransformProperties transformProperties;
    void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        giveDamage = GetComponent<GiveDamage>();
    }
    
    public void OnObjectReuse(object data)
    {
        giveDamage.hit = false;
        hit = false;
        rocketSprite.gameObject.SetActive(true);
        _collider2D.enabled = true;
        particleEffects[0].Play();
        particleEffects[1].Play();

        TrediconRocketProperties rocketProperties = (TrediconRocketProperties)data;

        giveDamage.damageToGive = rocketProperties.damage;
        giveDamage.rotationOffset = rocketProperties.rotationOffset;
        lifeTime = rocketProperties.lifeTime;
        transform.position = rocketProperties.position;
        transform.rotation = rocketProperties.rotation;
        transform.localScale = rocketProperties.scale;
        destroyedEffectPool = rocketProperties.destroyedEffectPool;
        speed = rocketProperties.speed;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        rocketSprite.localRotation = Quaternion.Euler(0, 0, angle);

        if((lifeTime -= Time.deltaTime) <= 0)
            DestroyProjectile();
    }
    void FixedUpdate()
    {
        if(giveDamage.hit || _collider2D.IsTouchingLayers(1 << LayerMask.NameToLayer("Collisions")))
            DestroyProjectile();

        transform.Translate(new Vector2(speed * Time.deltaTime, 0), Space.Self);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(transform.CompareTag("Projectile"))
        {
            if(other.CompareTag("Enemy Projectile"))
            {
                other.GetComponent<Health>().Kill();
                DestroyProjectile();
            }
        }
        else if(transform.CompareTag("Enemy Projectile"))
        {
            if(other.CompareTag("Projectile"))
            {
                other.GetComponent<Health>().Kill();
                DestroyProjectile();
            }
        }
    }

    void OnBecameInvisible()
    {
        if(destroyWhenInvisible)
            DestroyProjectile();
    }
    void DestroyProjectile()
    {
        if(!hit)
        {
            rocketSprite.gameObject.SetActive(false);
            _collider2D.enabled = false;

            particleEffects[0].Stop();
            particleEffects[1].Stop();

            if(properties.strength > 0)
                CameraManager.Instance.Shake(properties);

            if(destroyedSound != null)
                AudioManager.Instance.PlaySound2D(destroyedSound);

            transformProperties.position = transform.position;
            transformProperties.scale = Vector3.one;
            transformProperties.rotation = Quaternion.identity;

            if(!string.IsNullOrEmpty(destroyedEffectPool))
                PoolManager.Instance.ReuseObject(destroyedEffectPool, transformProperties);
            
            StartCoroutine(DelayDestroy());
            hit = true;
        }  
    }

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}

public struct TrediconRocketProperties
{
    public float speed;
    public int damage;
    public float lifeTime;
    public Vector2 position;
    public Quaternion rotation;
    public Vector2 scale;
    public float rotationOffset;
    public string destroyedEffectPool;
}