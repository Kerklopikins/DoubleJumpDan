using System.Collections;
using UnityEngine;

public class ChickenGun : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] Transform eggFirePoint;
    [SerializeField] float barrelLength;
    [SerializeField] float reloadTime = 0.3f;
    [SerializeField] float maxReloadAngle = 30;
    [SerializeField] AudioClip[] shootSounds;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] ParticleSystem shotEffect;
    [SerializeField] Sprite normal;
    [SerializeField] Sprite shot;
    [SerializeField] GameObject projectile;
    [SerializeField] GameObject destroyedEffect;
    [SerializeField] float speed = 70;
    [SerializeField] float lifeTime;
    [SerializeField] Vector2 kickMinMax = new Vector2(0.2f, 0.2f);
    public float recoilMoveSettleTime = 0.1f;
    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    bool reloading;
    float _fireRate;
    Player player;
    Vector3 recoilSmoothDampVelocity;
    Vector3 startingPosition;
    GunInfo gunInfo;
    SpriteRenderer spriteRenderer;
    string destroyedEffectPool;
    GameInputManager gameInputManager;
    void Start()
    {
        startingPosition = transform.localPosition;
        player = GetComponentInParent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameInputManager = GameInputManager.Instance;

        gunInfo = GetComponent<GunInfo>();
        gunInfo.Initialize();

        player.OnPlayerRespawn += ForceReload;

        if(destroyedEffect != null)
        {
            PoolManager.Instance.CreatePool(destroyedEffect.name, destroyedEffect.gameObject, (int)gunInfo.startingAmmo);
            destroyedEffectPool = destroyedEffect.name;
        }

        PoolManager.Instance.CreatePool(gameObject.name, projectile.gameObject, (int)gunInfo.startingAmmo);
    }

    void Update()
    {
        _fireRate -= Time.deltaTime;

        if(gunInfo.reloadTimer > 0)
            gunInfo.reloadTimer -= Time.deltaTime;

        if(gunInfo.currentAmmo <= 0 && gunInfo.reloadTimer <= 0 && !reloading)
            StartCoroutine(AnimateReload());

        if(!player.CanHandleInput())
            return;

        Vector3 direction = new Vector3();

        if(transform.lossyScale.x < 0)
            direction = -eggFirePoint.right;
        else if(transform.lossyScale.x > 0)
            direction = eggFirePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)eggFirePoint.position, direction);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)eggFirePoint.position + (Vector2)(direction * barrelLength);

        if(gunInfo.canShoot)
        {
            if(gameInputManager.ShootButton() && _fireRate <= 0 && gunInfo.currentAmmo > 0 && !reloading)
            {
                if(!wallHit)
                    shotEffect.Play();
                    
                gunInfo.reloadTimer = gunInfo.startReloadTimer;
                StartCoroutine(AnimateChicken());
                
                if(properties.strength > 0)
                    CameraManager.Instance.Shake(properties);

                gunInfo.Shoot(1);
            
                RicochetProperties ricochetProperties = new RicochetProperties();
                
                ricochetProperties.speed = speed * player.transform.localScale.x;
                ricochetProperties.damage = gunInfo.damage;
                ricochetProperties.lifeTime = lifeTime;
                ricochetProperties.position = spawnPosition;
                ricochetProperties.rotation = eggFirePoint.rotation;
                ricochetProperties.scale = new Vector3(player.transform.localScale.x, 1, 1);

                if(destroyedEffect != null)
                    ricochetProperties.destroyedEffectPool = destroyedEffectPool;

                float shotDistance = 200;

                RaycastHit2D hit;
                Ray2D ray = new Ray2D(spawnPosition, direction);
                hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, 1 << LayerMask.NameToLayer("Collisions"));

                ricochetProperties.direction = ray.direction;

                PoolManager.Instance.ReuseObject(gameObject.name, ricochetProperties);

                _fireRate = gunInfo.fireRate;
                transform.localPosition -= Vector3.down * Random.Range(kickMinMax.x, kickMinMax.y);
                AudioManager.Instance.PlaySound2D(shootSounds[Random.Range(0, shootSounds.Length)]);
            }
        }

        if(gameInputManager.ReloadButtonDown() && !reloading)
            if(gunInfo.currentAmmo < gunInfo.maxAmmo && gunInfo.reloadTimer <= 0)
                StartCoroutine(AnimateReload());
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startingPosition, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
    }
    public void ForceReload()
    {
        if(reloading)
        {
            transform.localEulerAngles = new Vector3(0, 0, -90);
            gunInfo.Reload();
            reloading = false;
        }
    }
    IEnumerator AnimateReload()
    {
        reloading = true;
        AudioManager.Instance.PlaySound2D(reloadSound);

        float reloadSpeed = 1 / reloadTime;
        float percent = 0;
        Vector3 initialRot = new Vector3(0, 0, -90);

        while(percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;

            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.forward * reloadAngle;

            yield return null;
        }

        gunInfo.Reload();
        reloading = false;
    }
    IEnumerator AnimateChicken()
    {
        spriteRenderer.sprite = shot;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = normal;
        yield return false;
    }

    void OnDrawGizmos()
    {
        if(eggFirePoint == null)
            return;

         Vector3 gunDirection = new Vector3();

        if(transform.lossyScale.x < 0)
            gunDirection = -eggFirePoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = eggFirePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)eggFirePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

		Gizmos.color = new Color(1, 0, 0, 1);

		Gizmos.DrawWireSphere(eggFirePoint.position, 0.25f);
        
        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)eggFirePoint.position + (Vector2)(gunDirection * barrelLength);

        Gizmos.DrawWireSphere(spawnPosition, 0.25f);
    }
}