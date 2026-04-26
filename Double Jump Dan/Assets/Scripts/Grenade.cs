using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour, IPoolable
{
    public float force;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D explosionCollider;
	[SerializeField] AudioClip blownUpSound;
    [SerializeField] GiveDamage giveDamage;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    [Header("Controller Rumble")]
    [SerializeField] float lowRumbleAmount;
    [SerializeField] float highRumbleAmount;
    [SerializeField] float rumbleDuration;

    Rigidbody2D rb2D;
    float lifeTime;
    string destroyedEffectPool;
    bool initiated;
    float lifeTimeAfterCollision = 2.25f;
    Collider2D grenadeCollider;
    int collisionMask;

    void Awake()
    {
        rb2D = GetComponentInParent<Rigidbody2D>();
        grenadeCollider = GetComponent<Collider2D>();
        collisionMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Enemies"));
    }

    public void OnObjectReuse(object data)
    {
        initiated = false;
        lifeTime = 5;

        spriteRenderer.enabled = true;
        grenadeCollider.enabled = true;
        explosionCollider.enabled = false;
        giveDamage.hit = false;
        
        ProjectileProperties projectileProperties = (ProjectileProperties)data;
        
        force = projectileProperties.speed;
        giveDamage.damageToGive = projectileProperties.damage;
        transform.position = projectileProperties.position;
        transform.rotation = projectileProperties.rotation;
        transform.localScale = projectileProperties.scale;
        destroyedEffectPool = projectileProperties.destroyedEffectPool;

        rb2D.velocity = transform.right * force;
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if(initiated)
        {
            if(lifeTimeAfterCollision > 0)
                lifeTimeAfterCollision -= Time.deltaTime;
            else
                gameObject.SetActive(false);
        }

        if(lifeTime <= 0)
            gameObject.SetActive(false);

        if(rb2D.velocity.sqrMagnitude < 0.3f * 0.3f)
            return;

        float angle = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg;

        if(transform.localScale.x == 1)
            transform.rotation = Quaternion.Euler(0, 0, angle);
        else
            transform.rotation = Quaternion.Euler(0, 0, angle + 180);
    }

    void OnObjectReuse()
    {
        ////////////////////////////////////////
        /// WORK ON GRENADE LAUNCHER
        /// WORK ON SHELLS
        /// WORK ON ENEMY GUNS
    }

	void OnCollisionEnter2D(Collision2D other)
	{
        if(((1 << other.gameObject.layer) & collisionMask) != 0)
        {
            lifeTime = 5;
            initiated = true;
            rb2D.velocity = Vector2.zero;
            AudioManager.Instance.PlaySound2D(blownUpSound);
            spriteRenderer.enabled = false;
            grenadeCollider.enabled = false;
            StartCoroutine(Explode());

            TransformProperties transformProperties = new TransformProperties();
            transformProperties.position = transform.position;
            transformProperties.scale = Vector3.one;
            transformProperties.rotation = Quaternion.identity;

            PoolManager.Instance.ReuseObject(destroyedEffectPool, transformProperties);

            if(properties.strength > 0)
                CameraManager.Instance.Shake(properties);

            GameInputManager.Instance.RumbleController(lowRumbleAmount, highRumbleAmount, rumbleDuration);
        }
	}

    IEnumerator Explode()
    {
        explosionCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        explosionCollider.enabled = false;
    }
}