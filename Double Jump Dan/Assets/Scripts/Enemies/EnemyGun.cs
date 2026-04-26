using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [Header("Main Stuff")]
    [Range(0, 5)]
    [SerializeField] float fireRate = 1;
    [SerializeField] bool shootsAllTheTime;

    [Header("Bullet")]
    [SerializeField] int bulletsToSpawn;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject destroyedEffect;
    public Transform[] bulletFireLocations;
    [SerializeField] float speed;
    [SerializeField] int bulletDamage;
    [SerializeField] float bulletLifeTime;
    [SerializeField] bool useRaycastEndPoint = true;

    [Header("Recoil")]
    [SerializeField] bool recoil;
    [SerializeField] Vector2 kickMinMax = new Vector2(0.2f, 0.2f);
    [Range(0, 1)]
    [SerializeField] float recoilMoveSettleTime = 0.1f;

    [Header("Effects")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] Transform shell;
    [SerializeField] Transform[] shellEjectionPoints;
    [SerializeField] Animator Animator;

    [Header("Muzzle Flash")]
    [SerializeField] GameObject flash;
    [SerializeField] float flashTime = 0.05f;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

	float _fireRate;
    Vector3 recoilSmoothDampVelocity;
    Vector3 startingPosition;
    string destroyedEffectPool;

    void Start()
    {
        if(recoil)
            startingPosition = transform.localPosition;

        if(flash != null)
            flash.SetActive(false);

        PoolManager.Instance.CreatePool(bullet.name, bullet, bulletsToSpawn);

        if(destroyedEffect != null)
        {
            PoolManager.Instance.CreatePool(destroyedEffect.name, destroyedEffect.gameObject, bulletsToSpawn);
            destroyedEffectPool = destroyedEffect.name;
        }
    }

    void LateUpdate()
    {
        if(recoil)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startingPosition, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
    }

    void Update()
    {
        _fireRate -= Time.deltaTime;

        if(shootsAllTheTime)
            Shoot();

        //if(basicEnemyAI.isAStopper)
        //{
         //   if(basicEnemyAI.seesPlayer && basicEnemyAI._secondsToPauseWhenSeesPlayer <= 0)
          //      OnTriggerHold();
          //  else if(basicEnemyAI.seesPlayer && !shootsAllTheTime)
          //      OnTriggerHold();
         //   else if(shootsAllTheTime)
         //       OnTriggerHold();
      //  }
    //    else if(!basicEnemyAI.isAStopper)
     ///   {
      //      if(basicEnemyAI.seesPlayer && !shootsAllTheTime)
     //           OnTriggerHold();
       //     else if(shootsAllTheTime)
       //         OnTriggerHold();
     //   }            
    }

    public void Shoot()
    {
        if(_fireRate <= 0)
        {
            if(Animator != null)
                Animator.SetTrigger("Shoot");

            if(flash != null)
                ActivateMuzzleFlash();
            
            if(properties.strength > 0)
                CameraManager.Instance.Shake(properties);

            for(int i = 0; i < bulletFireLocations.Length; i++)
            {
                if(bullet != null)
                {
                    Vector3 direction = new Vector3();

                    if(transform.lossyScale.x < 0)
                        direction = bulletFireLocations[i].right;
                    else if(transform.lossyScale.x > 0)
                        direction = -bulletFireLocations[i].right;
                    
                    ProjectileProperties projectileProperties = new ProjectileProperties();
                    
                    projectileProperties.speed = speed * -transform.lossyScale.x;
                    projectileProperties.damage = bulletDamage;
                    projectileProperties.lifeTime = bulletLifeTime;
                    projectileProperties.position = bulletFireLocations[i].position;
                    projectileProperties.rotation = bulletFireLocations[i].rotation;
                    projectileProperties.scale = new Vector3(-transform.lossyScale.x, 1, 1);
                    projectileProperties.useRaycast = useRaycastEndPoint;
                    
                    if(destroyedEffect != null)
                        projectileProperties.destroyedEffectPool = destroyedEffectPool;

                    if(useRaycastEndPoint)
                    {
                        float shotDistance = 200;

                            RaycastHit2D hit;
                            Ray2D ray = new Ray2D(bulletFireLocations[i].position, direction);
                            hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, 1 << LayerMask.NameToLayer("Collisions"));
                            Vector2 targetPoint;

                            if(hit)
                                targetPoint = hit.point;
                            else
                                targetPoint = ray.origin + ray.direction * shotDistance;

                            projectileProperties.targetPoint = targetPoint;
                            projectileProperties.direction = ray.direction;
                    }

                   PoolManager.Instance.ReuseObject(bullet.name, projectileProperties);
                }
            }

            if(shell || shellEjectionPoints != null)
            {
                for(int i = 0; i < shellEjectionPoints.Length; i++)
                {
                    Instantiate(shell, shellEjectionPoints[i].position, shellEjectionPoints[i].rotation);
                }
            }

            if(recoil)
                transform.localPosition -= Vector3.right * Random.Range(kickMinMax.x, kickMinMax.y);

            _fireRate = fireRate;
            AudioManager.Instance.PlaySound2D(shootSound);
        }
    }

    public void ActivateMuzzleFlash()
    {
        flash.SetActive(true);
        Invoke("DeactivateMuzzleFlash", flashTime);
    }

    public void DeactivateMuzzleFlash()
    {
        flash.SetActive(false);
    }
}