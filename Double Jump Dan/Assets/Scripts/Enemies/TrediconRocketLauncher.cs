using UnityEngine;

public class TrediconRocketLauncher : MonoBehaviour
{
    [SerializeField] float fireRate = 1;
    [SerializeField] int rocketsToSpawn;
    [SerializeField] float rocketSpeed;
    [SerializeField] int damage;
    [SerializeField] float lifeTime;
    [SerializeField] GameObject rocket;
    [SerializeField] Transform[] rocketFireLocations;
    [SerializeField] GameObject destroyedEffect;
    [SerializeField] AudioClip shootSound;
    
    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    //public SpriteRenderer laser;
    public Player player { get; set; }
    float _fireRate;
    string destroyedEffectPool;

    void Start()
    {
        PoolManager.Instance.CreatePool(rocket.name, rocket, rocketsToSpawn);

        if(destroyedEffect != null)
        {
            PoolManager.Instance.CreatePool(destroyedEffect.name, destroyedEffect.gameObject, rocketsToSpawn);
            destroyedEffectPool = destroyedEffect.name;
        }
    }

    //public void StopShooting()
    //{
        //laser.size = new Vector2(0, laser.size.y);
    //}

    public void Shoot()
    {
        Vector3 difference = player.transform.position - transform.position;
        difference.Normalize();

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if(transform.lossyScale.x > 0)
            transform.rotation = Quaternion.Euler(0, 0, rotZ + 180);
        else if(transform.lossyScale.x < 0)
            transform.rotation = Quaternion.Euler(0, 0, rotZ);

        Vector3 direction = new Vector3();

        if(transform.lossyScale.x < 0)
            direction = rocketFireLocations[0].right;
        else if(transform.lossyScale.x > 0)
            direction = -rocketFireLocations[0].right;

        float shotDistance = 200;

        RaycastHit2D hit;
        Ray2D ray = new Ray2D(rocketFireLocations[0].position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Player"));

        hit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, layerMask);
        //Vector2 targetPoint;

        //laser.size = new Vector2(hit.distance + 0.25f, laser.size.y);

        if(_fireRate > 0)
            _fireRate -= Time.deltaTime;

        if(_fireRate <= 0)
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if(properties.strength > 0)
                    CameraManager.Instance.Shake(properties);

                TrediconRocketProperties rocketProperties = new TrediconRocketProperties();
                    
                rocketProperties.speed = rocketSpeed * -transform.lossyScale.x;
                rocketProperties.damage = damage;
                rocketProperties.lifeTime = lifeTime;
                rocketProperties.position = rocketFireLocations[0].position;
                rocketProperties.rotation = rocketFireLocations[0].rotation;
                rocketProperties.scale = new Vector3(transform.lossyScale.x, 1, 1);
                
                if(destroyedEffect != null)
                    rocketProperties.destroyedEffectPool = destroyedEffectPool;

                if(transform.lossyScale.x > 0)
                    rocketProperties.rotationOffset = 90;
                else if(transform.lossyScale.x < 0)
                    rocketProperties.rotationOffset = -90;
                
                PoolManager.Instance.ReuseObject(rocket.name, rocketProperties);

                AudioManager.Instance.PlaySound2D(shootSound);
                _fireRate = fireRate;
            }
        }
    }
}