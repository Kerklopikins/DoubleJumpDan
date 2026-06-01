using UnityEngine;

public class HornyToadAI : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform edgeCheck;
    [SerializeField] float playerRange = 10;
	[SerializeField] ParticleSystem blood;

	Rigidbody2D rb2D;
	Player player;
	bool playerInRange;
    Vector2 direction;
    bool notAtEdge;
    bool hittingWall;
    float playerDistance;
	float _fireRate;

    void Start()
    {
        direction = new Vector2(-transform.localScale.x, 0);
        rb2D = GetComponent<Rigidbody2D>();
        player = LevelManager.Instance.player;
    }

    void Update()
    {
        playerInRange = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, playerRange, 1 << LayerMask.NameToLayer("Player"));
		_fireRate -= Time.deltaTime;
		_fireRate = Mathf.Clamp(_fireRate, 0, 3);

		if(playerInRange)
        {
            if(!player.dead)
            {
				if(_fireRate <= 0)
				{
					blood.Play();
					_fireRate = 3;
				}
            }
        }
        
        hittingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, collisionMask);

        if(hittingWall && direction.x < 0)
            hittingWall = true;
        else if(!hittingWall && direction.x > 0)
            hittingWall = false;
        
        notAtEdge = Physics2D.OverlapCircle(edgeCheck.position, 0.1f, collisionMask);

        if((direction.x < 0 && hittingWall) || (direction.x > 0 && hittingWall || !notAtEdge))
        {
            direction = -direction;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        rb2D.velocity = new Vector2(direction.x * speed, rb2D.velocity.y);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, playerRange);
    }
    #endif
}