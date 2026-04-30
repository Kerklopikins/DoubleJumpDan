using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Main Stuff")]
    public bool nonEnemy = false;
    public int health;
    [SerializeField] bool invincible;
    [SerializeField] GameObject healthBar;
    [SerializeField] Transform fill;

    [Header("Effects")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip destroyedSound;
    [SerializeField] GameObject destroyedEffect;
    [SerializeField] List<SpriteRenderer> spriteMaterials = new List<SpriteRenderer>();

    [Header("Gems")]
    [SerializeField] int gemsToGive;
    
    [Header("Camera Shake")]
    public CameraManager.Properties properties;

    [Header("Controller Rumble")]
    [SerializeField] float lowRumbleAmount;
    [SerializeField] float highRumbleAmount;
    [SerializeField] float rumbleDuration;
    
    public Vector3 effectScale { private get; set; }
    public Vector3 effectRotation { private get; set; }
    public float destroyedEffectRotation { get; set; }
    float spreadRadius = 4; //For gem explosion
    float popHeight = 4; //For gem explosion
    float duration = 0.75f; //For gem explosion
    float magnetDuration = 0.75f; //For gem explosion
    int I;
    float hurtTimer = 0.125f;
    float _hurtTimer;
    int startingHealth;
    ObjectOpitimizer objectOpitimizer;
    bool dead;
    Camera _camera;

    void Start()
    {
        startingHealth = health;
        _camera = LevelManager.Instance.mainCamera;

        if(GetComponentInParent<ObjectOpitimizer>() != null)
        {
            objectOpitimizer = GetComponentInParent<ObjectOpitimizer>();
            return;
        }

        if(GetComponent<ObjectOpitimizer>() != null)
            objectOpitimizer = GetComponent<ObjectOpitimizer>();
    }
    
    void Update()
    {
        if(_hurtTimer > 0)
            _hurtTimer -= Time.deltaTime;  

        if(healthBar == null)
            return;

        if(transform.localScale.x < 0)
            healthBar.transform.localScale = new Vector3(-1, 1, 1);
        else if(transform.localScale.x > 0)
            healthBar.transform.localScale = new Vector3(1, 1, 1);

        float _startingHealth = startingHealth;
        var healthPercent = health / _startingHealth;
        fill.transform.localScale = new Vector2(healthPercent, 1);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(invincible)
            return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Projectiles"))
        {
            GiveDamage giveDamage = other.GetComponent<GiveDamage>();
            TakeDamage(giveDamage.damageToGive);
            giveDamage.hit = true;
        }
    }
    
    void OnCollisionStay2D(Collision2D other)
    {
        if(invincible)
            return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Projectiles"))
        {
            GiveDamage giveDamage = other.gameObject.GetComponent<GiveDamage>();
            TakeDamage(giveDamage.damageToGive);
            giveDamage.hit = true;
        }
    }
    public void TakeDamage(int damage)
    {
        if(_hurtTimer > 0)
            return;

        _hurtTimer = hurtTimer;
        health -= damage;

        //FloatingNumberProperties numberProperties = new FloatingNumberProperties();
        //numberProperties.number = damage;
        //numberProperties.position = transform.position;
        //numberProperties.plusOrMinus = false;
        //numberProperties.color = Color.red;
        
        //PoolManager.Instance.ReuseObject("Floating Numbers", numberProperties);

        if(health > 0)
            if(hurtSound != null)
                AudioManager.Instance.PlaySound2D(hurtSound);

        if(I == 0 && spriteMaterials.Count > 0)
            StartCoroutine(Flash());

        if(health <= 0 && !dead)
        {
            Kill();
            dead = true;
        }
    }

    public void Kill()
    {
        health = 0;
        AudioManager.Instance.PlaySound2D(destroyedSound);
        GameInputManager.Instance.RumbleController(lowRumbleAmount, highRumbleAmount, rumbleDuration);

        if(properties.strength > 0)
            CameraManager.Instance.Shake(properties);

        if(objectOpitimizer != null)
        {
            objectOpitimizer.UnsubscribeFromPlayer();
            objectOpitimizer.enabled = false;
        }

        if(gemsToGive > 0)
            ExplodeGems(transform.position);

        if(destroyedEffect != null)
            Instantiate(destroyedEffect, transform.position, Quaternion.identity);

        if(healthBar != null)
            healthBar.SetActive(false);

        if(!nonEnemy)
        {
            ScreenEffectsManager.Instance.HitStop(0.125f, 0.25f);
            GameManager.Instance.currentUser.totalEnemiesKilled += 1;
            StartCoroutine(FlingAnimation(gameObject));
        }
    }

    public bool Dead()
    {
        if(health <= 0)
            return true;
        else
            return false;
    }

    public void IgnoreHurtTimer()
    {
        _hurtTimer = 0;
    }

    IEnumerator Flash()
    {
        for(int i = 0; i < 4; i++)
        {
            I = i;

            foreach(SpriteRenderer sprite in spriteMaterials)
                sprite.material.SetFloat("_FlashAmount", 1);

            yield return new WaitForSeconds(0.07f);

            foreach(SpriteRenderer sprite in spriteMaterials)
                sprite.material.SetFloat("_FlashAmount", 0);

            yield return new WaitForSeconds(0.07f);

            if(i == 3)
                I = 0;
        }
    }

    #region GiveGems
    void ExplodeGems(Vector3 originalPosition)
    {
        StartCoroutine(InstaintiateGems(originalPosition));
    }

    Vector3 GetRandomOffset()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(0.5f, spreadRadius);
        return new Vector3(randomCircle.x, randomCircle.y, 0);
    }
    
    IEnumerator InstaintiateGems(Vector3 originalPosition)
    {
        for(int i = 0; i < gemsToGive; i++)
        {
            TransformProperties transformProperties = new TransformProperties();
            transformProperties.position = originalPosition;
            transformProperties.scale = Vector3.zero;
            transformProperties.rotation = Quaternion.identity;

            GameObject gem = PoolManager.Instance.ReuseObject("Gems", transformProperties);
            SpriteRenderer gemSprite = gem.GetComponent<SpriteRenderer>();

            gemSprite.sprite = GameManager.Instance.gemSprites[UnityEngine.Random.Range(0, GameManager.Instance.gemSprites.Length)];
            Vector3 targetOffset = GetRandomOffset();
            StartCoroutine(PopGem(gem, originalPosition, originalPosition + targetOffset, gemSprite));
            yield return new WaitForSeconds(0.0125f);
        }
    }

    IEnumerator PopGem(GameObject gem, Vector3 start, Vector3 end, SpriteRenderer gemSprite)
    {
        float elapsed = 0;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float easedT = EaseBackOut(t);
            float arcY = Mathf.Sin(Mathf.Pow(t, 0.8f) * Mathf.PI) * popHeight;

            Vector3 flatPos = Vector3.Lerp(start, end, easedT);
            flatPos.y += arcY;

            float tScale = elapsed / duration * 3;
            gem.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, tScale);

            gem.transform.position = flatPos;
            yield return null;
        }

        gem.transform.position = end;

        yield return StartCoroutine(SinkGem(gem));
        yield return StartCoroutine(MagnetToPoint(gem, gemSprite));
    }

    IEnumerator SinkGem(GameObject gem)
    {
        float sinkDuration = 0.4f;
        float sinkAmount = 0.3f;
        float elapsed = 0;
        Vector3 start = gem.transform.position;
        Vector3 sinkTarget = start - new Vector3(UnityEngine.Random.Range(sinkAmount * 2, -sinkAmount * 2), sinkAmount, 0);

        while(elapsed < sinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sinkDuration);

            float easedT = -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

            gem.transform.position = Vector3.Lerp(start, sinkTarget, easedT);
            yield return null;
        }
    }

    IEnumerator MagnetToPoint(GameObject gem, SpriteRenderer gemSprite)
    {
        float elapsed = 0;
        Vector3 start = gem.transform.position;

        while(elapsed < magnetDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / magnetDuration);

            float easedT = t * t;

            gemSprite.color = new Color(gemSprite.color.r, gemSprite.color.g, gemSprite.color.b, Mathf.Lerp(1, 0, easedT));
            gem.transform.localEulerAngles += Vector3.forward * 1000 * Time.deltaTime;
            gem.transform.position = Vector3.Lerp(start, StatsHUD.Instance.gemIcon.position, easedT);
            yield return null;
        }

        LevelManager.Instance.AddGems(1);
        gem.SetActive(false);
        gemSprite.color = new Color(gemSprite.color.r, gemSprite.color.g, gemSprite.color.b, 1);
    }

    float EaseBackOut(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1f + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t- 1, 2);
    }
    #endregion

    #region EnemyFling
    IEnumerator FlingAnimation(GameObject enemy)
    {
        float direction = UnityEngine.Random.value > 0.5f ? 1 : -1;
        Vector3 startPosition = enemy.transform.position;
        float flingHeight = UnityEngine.Random.Range(7, 10);

        Vector3 flingEnd = startPosition + new Vector3(direction * UnityEngine.Random.Range(8, 12), -10, 0);
        
        float flingDuration = 1.5f;
        float elapsed = 0;

        float previousY = 0, previousY2 = 0;
        float previousX = 0, previousX2 = 0;
        float previousDeltaTime = 0, previousDeltaTime2 = 0;

        while(elapsed < flingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flingDuration);

            float easedT = t * t;
            float arcY = (1 - Mathf.Pow(t * 2 - 1, 2)) * flingHeight;

            Vector3 position = Vector3.Lerp(startPosition, flingEnd, easedT);
            position.y = startPosition.y + arcY + Mathf.Lerp(0, flingEnd.y - startPosition.y, t * t);
            
            previousY2 = previousY;
            previousX2 = previousX;
            previousDeltaTime2 = previousDeltaTime;

            previousY = enemy.transform.position.y;
            previousX = enemy.transform.position.x;
            previousDeltaTime = Time.deltaTime;

            enemy.transform.position = position;
            enemy.transform.Rotate(0, 0, direction * 500 * Time.deltaTime);

            yield return null;
        }

        float velocityY1 = Time.deltaTime > 0 ? (enemy.transform.position.y - previousY) / Time.deltaTime : 0;
        float velocityY2 = previousDeltaTime2 > 0 ? (previousY - previousY2) / previousDeltaTime2 : 0;
        
        float initialVelocityY = (velocityY1 + velocityY2) / 2;

        float velocityX1 = Time.deltaTime > 0 ? (enemy.transform.position.x - previousX) / Time.deltaTime : 0;
        float velocityX2 = previousDeltaTime2 > 0 ? (previousX - previousX2) / previousDeltaTime2 : 0;
        float initialVelocityX = (velocityX1 + velocityX2) / 2;

        float gravity = -18;
        float fallElapsed = 0;
        float fallDuration = 4f;

        Vector3 landedPosition = enemy.transform.position;

        while(fallElapsed < fallDuration)
        {
            fallElapsed += Time.deltaTime;
            float yOffset = initialVelocityY * fallElapsed + 0.5f * gravity * fallElapsed * fallElapsed;
            float xOffset = initialVelocityX * fallElapsed;
            
            enemy.transform.position = new Vector3(landedPosition.x + xOffset, landedPosition.y + yOffset, landedPosition.z);
            enemy.transform.Rotate(0, 0, direction * 500 * Time.deltaTime);

            if(enemy.transform.position.y < _camera.transform.position.y - _camera.orthographicSize * 2)
                fallDuration = 0;

            yield return null;
        }
        
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(3);
        
        Destroy(gameObject);
    }
    #endregion
}