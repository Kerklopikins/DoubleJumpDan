using UnityEngine;
using System.Collections;
public class Gun : MonoBehaviour
{
    public float reloadTime;
    public float maxReloadAngle;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public Animator gunAnimator;
    public ProjectileType projectileType;
    public GameObject projectile;
    public Transform[] projectileFirePoints;
    public Transform[] bullets;
    public float barrelLength;
    public GameObject destroyedEffect;
    public float speed = 70;
    public float lifeTime;
    public bool useRaycastEndPoint = true;
    public bool showTrajectory;
    public bool ricochet = false;
    public Vector2 kickMinMax = new Vector2(0.2f, 0.2f);
    public float recoilMoveSettleTime = 0.1f;
    public bool useMuzzleFlash;
    public GameObject flash;
    public float flashTime = 0.05f;
    public bool useSmokeParticles;
	public ParticleSystem smoke;

    public bool ejectShells;
    public Transform shell;
    public bool onlyEjectShellsWhenReloading;
    public Transform shellEjectionPoint;
    public bool hasGlow;
    public SpriteRenderer glowSprite;

    public CameraManager.Properties properties;

    public enum ProjectileType { GameObjectBased, RaycastBased };
    bool reloading;
    float _fireRate;
    Player player;
    Vector3 recoilSmoothDampVelocity;
    Vector3 startingPosition;
    GunInfo gunInfo;
    int collisionMask;
    ProjectileTrajectory projectileTrajectory;
    string destroyedEffectPool;
    GameInputManager gameInputManager;
    void Start()
    {
        startingPosition = transform.localPosition;
        player = GetComponentInParent<Player>();
        gameInputManager = GameInputManager.Instance;

        if(hasGlow)
            player.spriteMaterials.Add(glowSprite);

        if(showTrajectory)
            projectileTrajectory = GetComponent<ProjectileTrajectory>();

        if(useMuzzleFlash)
            flash.SetActive(false);

        gunInfo = GetComponent<GunInfo>();
        gunInfo.Initialize();

        player.OnPlayerRespawn += ForceReload;

        collisionMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Enemies"));
        int destroyedEffectPoolAmount;

        if(gunInfo.startingAmmo < projectileFirePoints.Length)
            destroyedEffectPoolAmount = projectileFirePoints.Length;
        else
            destroyedEffectPoolAmount = (int)gunInfo.startingAmmo;

        if(destroyedEffect != null)
        {
            PoolManager.Instance.CreatePool(destroyedEffect.name, destroyedEffect.gameObject, destroyedEffectPoolAmount);
            destroyedEffectPool = destroyedEffect.name;
        }
        
        if(projectileType == ProjectileType.GameObjectBased)
            PoolManager.Instance.CreatePool(gameObject.name, projectile.gameObject, destroyedEffectPoolAmount);

        if(ejectShells)
        {
            if(gunInfo.startingAmmo < 8)
                PoolManager.Instance.CreatePool(shell.name, shell.gameObject, (int)gunInfo.startingAmmo * 3);
            else
                PoolManager.Instance.CreatePool(shell.name, shell.gameObject, (int)gunInfo.startingAmmo * 2);
        }
        
        Transform bulletsParent = LevelManager.Instance.levelObjects;

        for(int i = 0; i < bullets.Length; i++)
            bullets[i].parent = bulletsParent;
    }

    void Update()
    { 
        if(player.CanHandleInput())
            HandleInput();

        ////////////////////////////////////ADDED
        if(gunInfo.reloadTimer > 0)
            gunInfo.reloadTimer -= Time.deltaTime;

        ////////////////////////////////////ADDED
        if(gunInfo.currentAmmo <= 0 && gunInfo.reloadTimer <= 0 && !reloading)
            StartCoroutine(AnimateReload());

        _fireRate -= Time.deltaTime;
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startingPosition, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
    }

    ////////////////////////////////////ADDED
    public void ForceReload()
    {
        for(int i = 0; i < projectileFirePoints.Length; i++)
        {
            projectileFirePoints[i].localScale = new Vector3(0, 1, 1);
        }

        if(showTrajectory)
            projectileTrajectory.EnableTrajectoryLine((Vector2)projectileFirePoints[0].position + (Vector2)GunDirection(0) * barrelLength, speed, 1);

        if(reloading)
        {
            transform.localEulerAngles = new Vector3(0, 0, -90);
            gunInfo.Reload();
            reloading = false;
        }
    }

    Vector3 GunDirection(int firePointIndex)
    {
        if(transform.lossyScale.x < 0)
            return -projectileFirePoints[firePointIndex].right;
        else if(transform.lossyScale.x > 0)
            return projectileFirePoints[firePointIndex].right;
        else
            return Vector3.zero;
    }

    void HandleInput()
    {
        if(gunInfo.canShoot)
        {
            if(showTrajectory && !reloading)
                projectileTrajectory.EnableTrajectoryLine((Vector2)projectileFirePoints[0].position + (Vector2)GunDirection(0) * barrelLength, speed, 1);

            if(gunInfo.fireMode == GunInfo.FireMode.Single)
                if(gameInputManager.ShootButtonDown())
                    Shoot();

            if(gunInfo.fireMode == GunInfo.FireMode.Automatic)
                if(gameInputManager.ShootButton())
                    Shoot();

            if(gunInfo.fireMode == GunInfo.FireMode.Burst)
            {
                if(gameInputManager.ShootButton() && !gunInfo.initiatedBurst)
                    Shoot();

                if(gunInfo.initiatedBurst)
                {
                    if(gunInfo.burstShotCounter < gunInfo.shotsPerBurst)
                    {
                        gunInfo._burstCoolDownTimer = gunInfo.burstCoolDownTime;
                        Shoot();
                    }
                    else
                    {
                        gunInfo._burstCoolDownTimer -= Time.deltaTime;
                    }
                }
                
                if(gunInfo._burstCoolDownTimer <= 0)
                {
                    gunInfo.initiatedBurst = false;            
                    gunInfo.burstShotCounter = 0;
                }
            }
        }

		if(gameInputManager.ReloadButtonDown() && !reloading)
            if(gunInfo.currentAmmo < gunInfo.maxAmmo && gunInfo.reloadTimer <= 0)
                StartCoroutine(AnimateReload());
    }

    void Shoot()
    {
        bool canFireProjectile = true;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)projectileFirePoints[0].position, GunDirection(0));
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, collisionMask);

        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)projectileFirePoints[0].position + (Vector2)(GunDirection(0) * barrelLength);

        if(gunInfo.currentAmmo <= 0 || reloading)
            canFireProjectile = false;

        if(canFireProjectile && _fireRate <= 0)
        {
            gunInfo.initiatedBurst = true;
                ///////////////////////ADDED
            gunInfo.reloadTimer = gunInfo.startReloadTimer;

            if(gunAnimator != null)
                gunAnimator.SetTrigger("Shoot");

            if(useMuzzleFlash && !wallHit)
                ActivateMuzzleFlash();

            if(properties.strength > 0)
                CameraManager.Instance.Shake(properties);

            gunInfo.Shoot(1);
            
            for(int i = 0; i < projectileFirePoints.Length; i++)
            {
                if(projectileType == ProjectileType.RaycastBased)
                {
                    float shotDistance = 50;
                    RaycastHit2D hit;
                    Ray2D ray = new Ray2D(spawnPosition, GunDirection(i));
                    hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, collisionMask);
                    ///////////////////////////////////////////////WHY IS IT NOT ACTIVATING AT START
                    Vector3 fixedRotation = new Vector3(0, 0, transform.lossyScale.x > 0 ? 0 : 180);
                    
                    bullets[i].transform.position = spawnPosition;
                    bullets[i].transform.eulerAngles = new Vector3(0, 0, projectileFirePoints[i].transform.eulerAngles.z + fixedRotation.z);

                    if(hit)
                    {
                        bullets[i].localScale = new Vector3(hit.distance * 0.5f, 1, 1);

                        if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                        {                            
                            Health health = hit.collider.GetComponent<Health>();
                            health.IgnoreHurtTimer();

                            health.TakeDamage(gunInfo.damage);
                            health.effectScale = new Vector3(transform.lossyScale.x, 1, 1);
                            health.effectRotation = new Vector3(0, 0, transform.eulerAngles.z);

                            RaycastHitEffect(hit.point);
                        }
                        else
                        {
                            RaycastHitEffect(hit.point);
                        }
                    }
                    else
                    {
                        bullets[i].localScale = new Vector3(shotDistance * 0.5f, 1, 1);
                    }
                    
                    StartCoroutine(DeactivateBullet());
                }
                else if(projectileType == ProjectileType.GameObjectBased)
                {
                    if(!ricochet)
                    {
                        ProjectileProperties projectileProperties = new ProjectileProperties();
                    
                        projectileProperties.speed = speed * player.transform.localScale.x;
                        projectileProperties.damage = gunInfo.damage;
                        projectileProperties.lifeTime = lifeTime;;
                        projectileProperties.position = spawnPosition;
                        projectileProperties.rotation = projectileFirePoints[i].rotation;
                        projectileProperties.scale = new Vector3(player.transform.localScale.x, 1, 1);
                        projectileProperties.useRaycast = useRaycastEndPoint;

                        if(destroyedEffect != null)
                            projectileProperties.destroyedEffectPool = destroyedEffectPool;

                        if(useRaycastEndPoint)
                        {
                            float shotDistance = 200;

                            RaycastHit2D hit;
                            Ray2D ray = new Ray2D(spawnPosition, GunDirection(i));
                            hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, 1 << LayerMask.NameToLayer("Collisions"));
                            Vector2 targetPoint;

                            if(hit)
                                targetPoint = hit.point;
                            else
                                targetPoint = ray.origin + ray.direction * shotDistance;

                            projectileProperties.targetPoint = targetPoint;
                            projectileProperties.direction = ray.direction;
                        }

                        PoolManager.Instance.ReuseObject(gameObject.name, projectileProperties);
                    }
                    else
                    {
                        RicochetProperties ricochetProperties = new RicochetProperties();
                    
                        ricochetProperties.speed = speed * player.transform.localScale.x;
                        ricochetProperties.damage = gunInfo.damage;
                        ricochetProperties.lifeTime = lifeTime;;
                        ricochetProperties.position = spawnPosition;
                        ricochetProperties.rotation = projectileFirePoints[i].rotation;
                        ricochetProperties.scale = new Vector3(player.transform.localScale.x, 1, 1);

                        if(destroyedEffect != null)
                            ricochetProperties.destroyedEffectPool = destroyedEffectPool;

                        float shotDistance = 200;

                        RaycastHit2D hit;
                        Ray2D ray = new Ray2D(spawnPosition, GunDirection(i));
                        hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, 1 << LayerMask.NameToLayer("Collisions"));

                        ricochetProperties.direction = ray.direction;

                        PoolManager.Instance.ReuseObject(gameObject.name, ricochetProperties);
                    }
                }
            }

            if(ejectShells)
                if(!onlyEjectShellsWhenReloading)
                    EjectShell();

            _fireRate = gunInfo.fireRate;
            transform.localPosition -= Vector3.down * Random.Range(kickMinMax.x, kickMinMax.y);
            AudioManager.Instance.PlaySound2D(shootSound);
        }
    }

    void RaycastHitEffect(Vector2 hitPoint)
    {
        TransformProperties properties = new TransformProperties();
        properties.position = hitPoint;
        properties.scale = new Vector3(-transform.lossyScale.x, 1, 1);
        properties.rotation = transform.rotation;

        PoolManager.Instance.ReuseObject(destroyedEffectPool, properties);
    }
    
    void EjectShell()
    {
        TransformProperties properties = new TransformProperties();
        properties.position = shellEjectionPoint.position;
        properties.rotation = shellEjectionPoint.rotation;

        PoolManager.Instance.ReuseObject(shell.name, properties);
    }
    public void ActivateMuzzleFlash()
    {
		if(useSmokeParticles)
			smoke.Play();
		
        flash.SetActive(true);
        Invoke("DeactivateMuzzleFlash", flashTime);
    }

    public void DeactivateMuzzleFlash()
    {
        flash.SetActive(false);
    }

    //////////////////////////ADDED
    IEnumerator AnimateReload()
    {
        reloading = true;

        if(showTrajectory)
            projectileTrajectory.DisableTrajectoryLine();

        AudioManager.Instance.PlaySound2D(reloadSound);

        float reloadSpeed = 1 / reloadTime;
        float percent = 0;
        Vector3 initialRot = new Vector3(0, 0, -90);

        if(ejectShells)
        {
            if(onlyEjectShellsWhenReloading)
            {
                if(shell || shellEjectionPoint != null)
                {
                    if(gunInfo.currentAmmo <= 0)
                        for(int i = 0; i < gunInfo.maxAmmo; i++)
                            EjectShell();
                    else
                        for(int i = 0; i < gunInfo.maxAmmo - gunInfo.currentAmmo; i++)
                            EjectShell();
                }
            }
        }

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

        yield return null;
    }

    IEnumerator DeactivateBullet()
    {
        yield return new WaitForSeconds(0.02f);

        for(int i = 0; i < projectileFirePoints.Length; i++)
            bullets[i].localScale = new Vector3(0, 1, 1);
    }

    void OnDrawGizmos()
    {
        if(projectileFirePoints[0] == null)
            return;

         Vector3 gunDirection = new Vector3();

        if(transform.lossyScale.x < 0)
            gunDirection = -projectileFirePoints[0].right;
        else if(transform.lossyScale.x > 0)
            gunDirection = projectileFirePoints[0].right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)projectileFirePoints[0].position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

		Gizmos.color = new Color(1, 0, 0, 1);

		Gizmos.DrawWireSphere(projectileFirePoints[0].position, 0.25f);
        
        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)projectileFirePoints[0].position + (Vector2)(gunDirection * barrelLength);

        Gizmos.DrawWireSphere(spawnPosition, 0.25f);
    }
}