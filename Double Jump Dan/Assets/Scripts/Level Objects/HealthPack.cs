using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField] Transform healthPackSprite;
    [SerializeField] GameObject collectEffect;
    [SerializeField] AudioClip collectSound;
    [SerializeField] Sprite defaultSprite;

    Player player;
    bool collected;
    float inTime = 0;
    Vector3 startPosition;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Start()
    {
        player = LevelManager.Instance.player;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        startPosition = transform.position;
        animator = GetComponent<Animator>();

        player.OnPlayerRespawn += PlayerRespawn;
    }

    void Update()
    {
        if(collected)
        {
            inTime += Time.deltaTime;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(1, 0, inTime / 0.75f));
            //transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, inTime / 0.6f);
            transform.position = Vector3.Lerp(startPosition, StatsHUD.Instance.healthTransform.position, inTime / 0.75f);
            inTime += Time.deltaTime;
            
            if(inTime > 0.75f)
            {
                if(!player.dead)
                {
                    player.GiveHealth(player.health);
                    Destroy(transform.parent.gameObject);
                }
            }
        }
    }

    void PlayerRespawn()
    {
        if(this == null)
            return; 
        
        collected = false;
        inTime = 0;
        transform.position = startPosition;
        spriteRenderer.color = spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            if(player._health == player.health)
                return;

            AudioManager.Instance.PlaySound2D(collectSound);
            collectEffect.SetActive(true);

            animator.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            
            collected = true;
        }
    }
}