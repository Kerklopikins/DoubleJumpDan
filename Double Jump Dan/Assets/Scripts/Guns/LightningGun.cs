using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningGun : MonoBehaviour
{
    [SerializeField] float reloadTime;
    [SerializeField] float maxReloadAngle = 30;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform lightningSpawnPoint;
    [SerializeField] float barrelLength;
    [SerializeField] int boltsToSpawn;
    [SerializeField] int length;
    [SerializeField] Sprite startSegment;
    [SerializeField] Sprite endSegment;
    [SerializeField] GameObject bolt;
    [SerializeField] float boltDuration;
    [SerializeField] float crazyness;
    [SerializeField] float lengthDivisor = 9.65f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] float hitTimer;
    [SerializeField] GameObject lightningSparks;
    [SerializeField] SpriteRenderer glowSprite;
    [SerializeField] float shakeDuration;
    [SerializeField] float shakeAmount;

    List<Transform> bolts = new List<Transform>();
    List<Transform> newBoltsParents = new List<Transform>();
    float timer;
    Player player;
    bool readyToShoot;
    bool shot;
    float _fireRate;
    GunInfo gunInfo;
    bool reloading;
    RaycastHit2D hit;
    Vector3 startPosition;
    GameInputManager gameInputManager;
    float _hitTimer;
    TransformProperties hitEffectProperties;

    void Start()
    {
        player = LevelManager.Instance.player;
        player.spriteMaterials.Add(glowSprite);
        startPosition = transform.localPosition;
        gameInputManager = GameInputManager.Instance;

        gunInfo = GetComponent<GunInfo>();
        gunInfo.Initialize();

        player.OnPlayerRespawn += ForceReload;
        player.OnPlayerKilled += PlayerKilledOrLevelFinished;
        LevelManager.Instance.OnLevelFinished += PlayerKilledOrLevelFinished;
        
        for(int i = 0; i < boltsToSpawn; i++)
        {
            if(i == 0)
            {
                var _bolt = (GameObject)Instantiate(bolt, Vector3.zero, Quaternion.identity);
                _bolt.transform.parent = lightningSpawnPoint;
                _bolt.transform.position = lightningSpawnPoint.position;
                newBoltsParents.Add(_bolt.transform.Find("Next Lightning Parent"));
                _bolt.transform.localScale = new Vector3(length / boltsToSpawn, 1, 1);
                bolts.Add(_bolt.transform);
                _bolt.GetComponent<SpriteRenderer>().sprite = startSegment;
            }
            else
            {
                var _bolt = (GameObject)Instantiate(bolt, Vector3.zero, Quaternion.identity);
                _bolt.transform.parent = lightningSpawnPoint;
                _bolt.transform.position = newBoltsParents[i - 1].position;
                newBoltsParents.Add(_bolt.transform.Find("Next Lightning Parent"));
                _bolt.transform.localScale = new Vector3(length / boltsToSpawn, 1, 1);
                bolts.Add(_bolt.transform);

                if(i == boltsToSpawn - 1)
                    _bolt.GetComponent<SpriteRenderer>().sprite = endSegment;
            }
        }
        
        timer = boltDuration;
        
        lightningSpawnPoint.gameObject.SetActive(false);
        PoolManager.Instance.CreatePool(lightningSparks.name, lightningSparks, (int)gunInfo.startingAmmo + (int)gunInfo.startingAmmo / 2);
    }

    void Update()
    {
        if(!readyToShoot && !player.dead)
            _fireRate -= Time.deltaTime;

        if(gunInfo.reloadTimer > 0)
            gunInfo.reloadTimer -= Time.deltaTime;

        if(!player.CanHandleInput())
            return;

        if(gunInfo.currentAmmo <= 0 && gunInfo.reloadTimer <= 0 && !reloading)
            StartCoroutine(AnimateReload());
        
        if(gameInputManager.ReloadButtonDown() && !reloading)
            if(gunInfo.currentAmmo < gunInfo.maxAmmo && gunInfo.reloadTimer <= 0)
                StartCoroutine(AnimateReload());

        Vector3 gunDirection = new Vector3();
        
        if(transform.lossyScale.x < 0)
            gunDirection = -firePoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = firePoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)firePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));

        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)firePoint.position + (Vector2)(gunDirection * barrelLength);

        if(_fireRate <= 0)
        {
            if(!shot)
                readyToShoot = true;
            
            hit = Physics2D.Raycast(spawnPosition, transform.right * player.transform.localScale.x, length, collisionMask);
            lightningSpawnPoint.transform.position = spawnPosition;
            
            if(gunInfo.canShoot)
            {
                if(gameInputManager.ShootButton() && gunInfo.currentAmmo > 0 && !reloading)
                {
                    if(!shot)
                    {
                        gunInfo.reloadTimer = gunInfo.startReloadTimer;
                        
                        gunInfo.Shoot(1);
                        StopCoroutine(Vibrate());
                        StartCoroutine(Vibrate());
                        AudioManager.Instance.PlaySound2D(shootSound);
                    }

                    shot = true;
                    readyToShoot = false;
                }
            }

            if(!readyToShoot)
            {
                timer -= Time.deltaTime;

                if(timer <= 0)
                {
                    lightningSpawnPoint.gameObject.SetActive(true);

                    for(int i = 0; i < bolts.Count; i++)
                    {
                        if(hit)
                            bolts[i].transform.localScale = new Vector3(hit.distance / lengthDivisor, 1, 1);
                        else
                            bolts[i].transform.localScale = new Vector3(length / lengthDivisor, 1, 1);

                        if(i > 0)
                            bolts[i].transform.position = newBoltsParents[i - 1].transform.position;

						bolts[i].transform.eulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + Random.Range(crazyness, -crazyness) + transform.eulerAngles.z + 90);
                    }

                    timer = boltDuration;
                }
            }

            if(_fireRate <= -0.3f)
            {
                _fireRate = gunInfo.fireRate;
                readyToShoot = false;
                shot = false;
            }
        }
        else
            lightningSpawnPoint.gameObject.SetActive(false);


        if(lightningSpawnPoint.gameObject.activeSelf == true)
        {
            if(hit.collider != null)
            {                  
                if(_hitTimer > 0)
                {
                    _hitTimer -= Time.deltaTime;
                }
                else
                {
                    hitEffectProperties.position = hit.point;
                    hitEffectProperties.scale = Vector3.one;
                    hitEffectProperties.rotation = Quaternion.identity;

                    PoolManager.Instance.ReuseObject(lightningSparks.name, hitEffectProperties);

                    if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                        hit.collider.GetComponent<Health>().TakeDamage(gunInfo.damage);

                    _hitTimer = hitTimer;
                } 
            }
        }
        else
        {
            _hitTimer = 0;
        }
    }

    void PlayerKilledOrLevelFinished()
    {
        _fireRate = 0;
        readyToShoot = true;
        shot = false;
        lightningSpawnPoint.gameObject.SetActive(false);    
    }

    void ForceReload()
    {
        if(reloading)
        {
            transform.localEulerAngles = new Vector3(0, 0, -90);
            gunInfo.Reload();
            reloading = false;
        }

        transform.localPosition = startPosition;
    }

    IEnumerator Vibrate()
    {
        float timer = 0;

        while(timer < shakeDuration)
        {
            if(player.dead || LevelManager.Instance.FinishedLevel())
                break;

            timer += Time.deltaTime;

            Vector3 offset = Random.insideUnitCircle * shakeAmount;

            if(Time.timeScale > 0)
                transform.localPosition = startPosition + offset;

            yield return null;
        }

        if(!player.dead && !LevelManager.Instance.FinishedLevel())
            transform.localPosition = startPosition;
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
            if(player.dead || LevelManager.Instance.FinishedLevel())
                break;

            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;

            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.forward * reloadAngle;

            yield return null;
        }

        if(!player.dead && !LevelManager.Instance.FinishedLevel())
        {
            transform.localEulerAngles = initialRot;
            gunInfo.Reload();
            reloading = false;
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(lightningSpawnPoint == null)
            return;

         Vector3 gunDirection = new Vector3();

        if(transform.lossyScale.x < 0)
            gunDirection = -lightningSpawnPoint.right;
        else if(transform.lossyScale.x > 0)
            gunDirection = lightningSpawnPoint.right;

        RaycastHit2D wallHit;
        Ray2D wallRay = new Ray2D((Vector2)firePoint.position, gunDirection);
        wallHit = Physics2D.Raycast(wallRay.origin, wallRay.direction, barrelLength, 1 << LayerMask.NameToLayer("Collisions"));
        
		Gizmos.color = new Color(1, 0, 0, 1);

		Gizmos.DrawWireSphere(firePoint.position, 0.25f);
        
        Vector2 spawnPosition = wallHit ? wallHit.point : (Vector2)firePoint.position + (Vector2)(gunDirection * barrelLength);

        Gizmos.DrawWireSphere(spawnPosition, 0.25f);

        // Gizmos.color = Color.blue;
        // RaycastHit2D hit;
        // hit = Physics2D.Raycast(spawnPosition, transform.right * player.transform.localScale.x, length, collisionMask);
        // Gizmos.DrawLine(spawnPosition, hit ? hit.point : transform.right * player.transform.localScale.x * 10);
    }
    #endif
}