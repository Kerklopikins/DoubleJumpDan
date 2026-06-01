using System.Collections;
using UnityEngine;

public class LavaGun : MonoBehaviour
{
    [SerializeField] float reloadTime;
    [SerializeField] float maxReloadAngle = 30;
    [SerializeField] AudioClip spewSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] Transform firePoint;
    [SerializeField] float barrelLength;
    [SerializeField] SpriteRenderer glowSprite;

    ParticleSystem lava;
    Player player;
    float lavaStartEmission;
    ParticleSystem.EmissionModule lavaEmission;
    bool reloading;
    GunInfo gunInfo;
    GameInputManager gameInputManager;

    void Start()
    {
        lava = GetComponent<ParticleSystem>();
        player = LevelManager.Instance.player;
        player.spriteMaterials.Add(glowSprite);
        lavaEmission = lava.emission;
        lavaStartEmission = lavaEmission.rateOverTime.constantMax;
        gameInputManager = GameInputManager.Instance;

        gunInfo = GetComponentInParent<GunInfo>();
        gunInfo.Initialize();

        player.OnPlayerRespawn += ForceReload;
    }

    void Update()
    {        
        if(gunInfo.reloadTimer > 0)
            gunInfo.reloadTimer -= Time.deltaTime;

        if(gunInfo.currentAmmo <= 0 && gunInfo.reloadTimer <= 0 && !reloading)
            StartCoroutine(AnimateReload());

        if(!player.CanHandleInput())
            return;

        if(gunInfo.canShoot)
        {
            if(gameInputManager.ShootButton() && gunInfo.currentAmmo > 0 && !reloading)
            {
                Vector3 gunDirection = new Vector3();
        
                if(transform.lossyScale.x < 0)
                    gunDirection = -firePoint.right;
                else if(transform.lossyScale.x > 0)
                    gunDirection = firePoint.right;

                RaycastHit2D wallHit;
                Ray2D wallRay = new Ray2D((Vector2)firePoint.position, gunDirection);
                wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

                if(!wallHit)
                {
                    gunInfo.reloadTimer = gunInfo.startReloadTimer;
                    gunInfo.Shoot(Time.deltaTime);
                    lavaEmission.rateOverTime = new ParticleSystem.MinMaxCurve(lavaStartEmission);
                }
                else
                {
                    lavaEmission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
                }
            }
            else
                lavaEmission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
        }
        else
            lavaEmission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
        

        if(gameInputManager.ReloadButtonDown() && !reloading)
            if(gunInfo.currentAmmo < gunInfo.maxAmmo && gunInfo.reloadTimer <= 0)
                StartCoroutine(AnimateReload());
    }
    public void ForceReload()
    {
        if(reloading)
        {
            transform.parent.localEulerAngles = new Vector3(0, 0, -90);
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
            transform.parent.localEulerAngles = initialRot + Vector3.forward * reloadAngle;

            yield return null;
        }

        transform.localEulerAngles = initialRot;
        gunInfo.Reload();
        reloading = false;

        yield return null;
    }

    void OnParticleCollision(GameObject other)
    {
        if(other.layer == LayerMask.NameToLayer("Enemies"))
            other.GetComponent<Health>().TakeDamage(gunInfo.damage);

        //if(other.GetComponent<Player>() != null)
           // other.GetComponent<Player>().TakeDamage(gunInfo.damage, 0, false, 0, 0, 0, transform, false, 0, 0);
    }

    #if UNITY_EDITOR
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
    #endif
}