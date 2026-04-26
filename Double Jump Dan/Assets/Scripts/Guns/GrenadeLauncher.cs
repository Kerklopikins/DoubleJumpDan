using System;
using System.Collections;
using UnityEngine;

public class GrenadeLauncher : MonoBehaviour
{
    [SerializeField] Grenade grenade;
    [SerializeField] float grenadeShotForce;
    [SerializeField] GameObject shotForce;
    [SerializeField] Transform shotForceFillPivot;
    [SerializeField] Transform grenadeFirePoint;
    [SerializeField] float barrelLength;
    [SerializeField] GameObject grenadeDestroyedEffect;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] Vector2 kickMinMax = new Vector2(0.2f, 0.2f);
    [SerializeField] float recoilMoveSettleTime = 0.1f;
    [SerializeField] float reloadTime = 0.3f;
    [SerializeField] float maxReloadAngle = 30;
    
    float _fireRate;
    float _shotForce;
    int direction = 1;
    Player player;
    GunInfo gunInfo;
    bool reloading;
    Vector3 recoilSmoothDampVelocity;
    Vector3 startingPosition;
    ProjectileTrajectory projectileTrajectory;
    string destroyedEffectPool;
    int collisionMask;
    GameInputManager gameInputManager;

	void Start()
    {
        player = LevelManager.Instance.player;
        projectileTrajectory = GetComponent<ProjectileTrajectory>();
        
        startingPosition = transform.localPosition;
        grenade.force = grenadeShotForce;
        gameInputManager = GameInputManager.Instance;

        gunInfo = GetComponent<GunInfo>();
        gunInfo.Initialize();

        player.OnPlayerRespawn += ForceReload;
        collisionMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Enemies"));

        PoolManager.Instance.CreatePool(grenadeDestroyedEffect.name, grenadeDestroyedEffect.gameObject, (int)gunInfo.startingAmmo / 2);        
        PoolManager.Instance.CreatePool(gameObject.name, grenade.gameObject, (int)gunInfo.startingAmmo / 2);
        destroyedEffectPool = grenadeDestroyedEffect.name;
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

        Vector3 gunDirection = new Vector3();
        
        if(transform.lossyScale.x < 0)
            gunDirection = -grenadeFirePoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = grenadeFirePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)grenadeFirePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, collisionMask);

        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)grenadeFirePoint.position + (Vector2)(gunDirection * barrelLength);
        
        if(gunInfo.canShoot)
        {
            if(gameInputManager.ShootButton() && _fireRate <= 0 && gunInfo.currentAmmo > 0 && !reloading)
            {
                shotForce.SetActive(true);

                if(_shotForce >= 2)
                    direction = -1;
                else if(_shotForce <= 1)
                    direction = 1;
                
                _shotForce = Mathf.Clamp(_shotForce, 1, 2);
                _shotForce += 2 * Time.deltaTime * direction;

                shotForceFillPivot.localScale = new Vector3(_shotForce - 1, 1, 1);

                if(!wallHit)
                    projectileTrajectory.EnableTrajectoryLine(spawnPosition, grenade.force, _shotForce);
                else
                    projectileTrajectory.DisableTrajectoryLine();
            }
            else
            {
                shotForce.SetActive(false);
                projectileTrajectory.DisableTrajectoryLine();
            }

            if(gameInputManager.ShootButtonUp() && _fireRate <= 0 && gunInfo.currentAmmo > 0 && !reloading)
            {                
                gunInfo.reloadTimer = gunInfo.startReloadTimer;

                ProjectileProperties projectileProperties = new ProjectileProperties();
                
                projectileProperties.speed = grenade.force * player.transform.localScale.x * _shotForce;
                projectileProperties.damage = gunInfo.damage;
                projectileProperties.position = spawnPosition;
                projectileProperties.rotation = grenadeFirePoint.rotation;
                projectileProperties.scale = player.transform.localScale;
                projectileProperties.destroyedEffectPool = destroyedEffectPool;

                PoolManager.Instance.ReuseObject(gameObject.name, projectileProperties);

                _shotForce = 1;
                _fireRate = gunInfo.fireRate;
                gunInfo.Shoot(1);

                transform.localPosition -= Vector3.down * UnityEngine.Random.Range(kickMinMax.x, kickMinMax.y);
                AudioManager.Instance.PlaySound2D(shootSound);
            }
        }
        else
        {
            shotForce.SetActive(false);
            projectileTrajectory.DisableTrajectoryLine();
            _shotForce = 0;
        }

        if(gameInputManager.ReloadButtonDown() && !reloading)
            if(gunInfo.currentAmmo < gunInfo.maxAmmo && gunInfo.reloadTimer <= 0)
                StartCoroutine(AnimateReload());
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

        yield return null;
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startingPosition, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
    }
    
    void OnDrawGizmos()
    {
        if(grenadeFirePoint == null)
            return;

         Vector3 gunDirection = new Vector3();

        if(transform.lossyScale.x < 0)
            gunDirection = -grenadeFirePoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = grenadeFirePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)grenadeFirePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

		Gizmos.color = new Color(1, 0, 0, 1);

		Gizmos.DrawWireSphere(grenadeFirePoint.position, 0.25f);
        
        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)grenadeFirePoint.position + (Vector2)(gunDirection * barrelLength);

        Gizmos.DrawWireSphere(spawnPosition, 0.25f);
    }
}