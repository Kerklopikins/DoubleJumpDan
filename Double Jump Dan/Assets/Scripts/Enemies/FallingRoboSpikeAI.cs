using UnityEngine;

public class FallingRoboSpikeAI : MonoBehaviour
{
    [SerializeField] float spinSpeed;
    [SerializeField] Transform rotatingSprite;
	[SerializeField] Sprite fallingSprite;
	[SerializeField] SpriteRenderer eye;
	[SerializeField] Sprite redEye;
    [SerializeField] WatchOutTrigger watchOutTrigger;

    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
	Health health;
    GiveDamage giveDamage;
    bool hitPlayer;
	bool triggeredWatchOut;
    Vector3 startPosition;
    Collider2D _collider;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
		spriteRenderer = rotatingSprite.GetComponent<SpriteRenderer>();
		health = GetComponent<Health>();
        giveDamage = GetComponent<GiveDamage>();
        _collider = GetComponent<Collider2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        if(health.Dead())
        {
            _collider.enabled = false;
            watchOutTrigger.gameObject.SetActive(false);
            
            if(rb2D != null)
                rb2D.simulated = false;

            this.enabled = false;
            return;
        }

        if(giveDamage.hit)
            health.TakeDamage(health.health);
       
        if(hitPlayer)
        {
            if(!triggeredWatchOut)
            {
                eye.sprite = redEye;
                watchOutTrigger.Activate();
                triggeredWatchOut = true;
            }

            rb2D.isKinematic = false;
            rotatingSprite.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
            spriteRenderer.sprite = fallingSprite;

            Destroy(gameObject, 5);
        }
    }
    void FixedUpdate()
    {
        RaycastHit2D hit;
        int layerMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Player"));
        hit = Physics2D.Raycast(startPosition, Vector3.down, 100, layerMask);

        if(hit && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            hitPlayer = true;

        if(transform.position.y < hit.point.y)
            health.TakeDamage(health.health);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        RaycastHit2D hit;
        hit = Physics2D.Raycast(transform.position, Vector3.down, 100, 1 << LayerMask.NameToLayer("Collisions"));

		Gizmos.color = new Color(1, 0, 0, 1);

		if(hit.point.x == 0)
		{
			Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 50, 0));
			return;
		}
		
        Gizmos.DrawLine(transform.position, hit.point);
    }
    #endif
}