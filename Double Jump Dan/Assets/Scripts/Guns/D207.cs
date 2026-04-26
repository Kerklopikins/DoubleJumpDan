using System.Collections;
using UnityEngine;

public class D207: MonoBehaviour
{
    [SerializeField] SpriteRenderer laser;
    [SerializeField] Transform firePoint;
    [SerializeField] float barrelLength;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float laserFadeTime;
    [SerializeField] SpriteRenderer shotLight;
    [SerializeField] SpriteRenderer laserHitEffect;
    [SerializeField] float reloadTime = 0.3f;
    [SerializeField] float maxReloadAngle = 30;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] Vector2 kickMinMax = new Vector2(0.2f, 0.2f);
    [SerializeField] float recoilMoveSettleTime = 0.1f;
    [SerializeField] SpriteRenderer glowSprite;
    bool reloading;
    float _fireRate;
    Player player;
    float laserFadeSpeed;
    float percent;
    Vector3 recoilSmoothDampVelocity;
    Vector3 startingPosition;
    Transform laserParent;
    GunInfo gunInfo;
    GameInputManager gameInputManager;
	void Start()
	{
        laserParent = LevelManager.Instance.levelObjects;

        laser.transform.parent = laserParent;
        laserHitEffect.transform.parent = laserParent;

        player = LevelManager.Instance.player;
        player.spriteMaterials.Add(glowSprite);
        startingPosition = transform.localPosition;
        gameInputManager = GameInputManager.Instance;
        
        laserFadeSpeed = 1 / laserFadeTime;

        gunInfo = GetComponent<GunInfo>();
        gunInfo.Initialize();
        percent = 1;

        player.OnPlayerRespawn += ForceReload;
    }
	
	void Update() 
	{
        _fireRate -= Time.deltaTime;

        if(gunInfo.reloadTimer > 0)
            gunInfo.reloadTimer -= Time.deltaTime;

        if(gunInfo.currentAmmo <= 0 && gunInfo.reloadTimer <= 0 && !reloading)
            StartCoroutine(AnimateReload());

        if(percent < 1)
        {
            percent += Time.deltaTime * laserFadeSpeed;

            float alpha = Mathf.Lerp(1, 0, percent);

            shotLight.color = new Color(shotLight.color.r, shotLight.color.g, shotLight.color.b, alpha);
            laser.color = new Color(laser.color.r, laser.color.g, laser.color.b, alpha);
            laserHitEffect.color = new Color(laserHitEffect.color.r, laserHitEffect.color.g, laserHitEffect.color.b, alpha);
        }

        if(!player.CanHandleInput())
            return;

         Vector3 direction = new Vector3();
        
        if(transform.lossyScale.x < 0)
            direction = -firePoint.right;
        else if(transform.lossyScale.x > 0)
            direction = firePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)firePoint.position, direction);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, collisionMask);

        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)firePoint.position + (Vector2)(direction * barrelLength);

        if(gunInfo.canShoot)
        {
            if(gameInputManager.ShootButtonDown() && _fireRate <= 0 && gunInfo.currentAmmo > 0 && !reloading)
            {
                gunInfo.reloadTimer = gunInfo.startReloadTimer;

                float shotDistance = 50;
                RaycastHit2D hit;
                Ray2D ray = new Ray2D(spawnPosition, direction);
                hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, collisionMask);

                if(hit)
                {
                    shotDistance = hit.distance;

                    if(hit.collider.GetComponent<Health>() != null)
                        hit.collider.GetComponent<Health>().TakeDamage(gunInfo.damage);
                }

                Vector3 hitPoint = ray.direction * shotDistance;
                Vector3 fixedRotation = new Vector3(0, 0, transform.lossyScale.x > 0 ? 0 : 180);

                if(hit)
                {
                    laser.size = new Vector2(hit.distance + 0.5f, laser.size.y);
                    laserHitEffect.transform.localScale = Vector3.one;
                    laserHitEffect.transform.position = hit.point;
                }
                else
                {
                    laser.size = new Vector2(shotDistance + 0.5f, laser.size.y);
                    laserHitEffect.transform.localScale = Vector3.zero;
                    laserHitEffect.transform.position = hitPoint;
                }

                laser.transform.position = spawnPosition;
                laser.transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z + fixedRotation.z);
                
                percent = 0;
                _fireRate = gunInfo.fireRate;
                gunInfo.Shoot(1);

                transform.localPosition -= Vector3.down * Random.Range(kickMinMax.x, kickMinMax.y);
                AudioManager.Instance.PlaySound2D(shootSound);
            }
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

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startingPosition, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
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

    void OnDrawGizmos()
    {
        if(firePoint == null)
            return;

         Vector3 gunDirection = new Vector3();

        if(transform.lossyScale.x < 0)
            gunDirection = -firePoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = firePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)firePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

		Gizmos.color = new Color(1, 0, 0, 1);

		Gizmos.DrawWireSphere(firePoint.position, 0.25f);
        
        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)firePoint.position + (Vector2)(gunDirection * barrelLength);

        Gizmos.DrawWireSphere(spawnPosition, 0.25f);
    }
}