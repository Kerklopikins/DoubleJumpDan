using UnityEngine;

public class MrGobblesAI : MonoBehaviour
{
    public float speed;
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0, 0.125f);
    public Transform wallCheck;
    public float playerRange = 10;
    public float jumpTimer;
    public float jumpHeight;
    public float angrySpeed;
    public Transform dottedLineStartPoint;
    public AudioClip gobble;
    public AudioClip backwards;
    public WatchOutTrigger watchOutTrigger;

    public bool seesPlayer { get; protected set; }
    public Rigidbody2D rb2D { get; protected set; }
    public Player player { get; protected set; }
    public bool playerInRange { get; protected set; }
    float interval = 1;
    float timer;
    Vector2 direction;
    Animator animator;
    float _jumpTimer;
    bool grounded;
    bool hittingWall;
    float _angrySpeed;
    bool triggeredWatchOut;
    float playerDistance;
    Vector3 wallCheckStartingPosition;

    void Start()
    {
        direction = new Vector2(-transform.localScale.x, 0);
        _jumpTimer = jumpTimer;
        rb2D = GetComponent<Rigidbody2D>();
        timer = interval;
        _angrySpeed = angrySpeed;
        animator = GetComponent<Animator>();
        player = LevelManager.Instance.player;
        wallCheckStartingPosition = wallCheck.localPosition;
    }

    void Update()
    {
        playerDistance = Vector3.Distance(player.transform.position, transform.position);

        timer -= Time.deltaTime;

        if(playerRange > 0)
        {
            if(!player.dead)
            {
                if(playerDistance <= playerRange)
                    playerInRange = true;
                else
                    playerInRange = false;

                if(playerInRange)
                    seesPlayer = true;
                else
                    seesPlayer = false;
            }
            else
            {
                playerInRange = false;
                seesPlayer = false;
            }
        }

        grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, 1 << LayerMask.NameToLayer("Collisions"));
        hittingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, 1 << LayerMask.NameToLayer("Collisions"));

        if(!seesPlayer)
            animator.SetFloat("Speed", 1);

        animator.SetBool("Grounded", grounded);

        _jumpTimer -= Time.deltaTime;

        if(_jumpTimer <= 0 && grounded && hittingWall && seesPlayer)
        {
            _jumpTimer = jumpTimer;
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
        }
        
        if(!seesPlayer)
        {
            rb2D.velocity = new Vector2(direction.x * speed, rb2D.velocity.y);
            wallCheck.localPosition = new Vector3(wallCheckStartingPosition.x, wallCheck.localPosition.y, 0);

            if((direction.x < 0 && hittingWall) || (direction.x > 0 && hittingWall))
            {
                direction = -direction;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            triggeredWatchOut = false;
        }

        if(timer <= 0)
            timer = interval;

        if(seesPlayer)
        {
            wallCheck.localPosition = new Vector3(wallCheckStartingPosition.x - 2, wallCheck.localPosition.y, 0);

            if((direction.x < 0 && hittingWall) || (direction.x > 0 && hittingWall))
            {
                direction = -direction;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            rb2D.velocity = new Vector2(0, rb2D.velocity.y);

            if(!triggeredWatchOut)
            {
                if(player.transform.position.x > transform.position.x)
                {
                    transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                    direction = new Vector2(1, 0);
                }
                else if(player.transform.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                    direction = new Vector2(-1, 0);
                }

                triggeredWatchOut = true;
                watchOutTrigger.Activate();
            }

            animator.SetFloat("Speed", 2);

            if(player.transform.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                direction = new Vector2(1, 0);
            }
            else if(player.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                direction = new Vector2(-1, 0);
            }

            rb2D.velocity = new Vector2(direction.x * angrySpeed, rb2D.velocity.y);
        }

        if(rb2D.velocity.x == 0)
            animator.SetBool("Stopped", true);
        else
            animator.SetBool("Stopped", false);

        animator.SetBool("Player In Range", seesPlayer);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(groundCheck != null)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(groundCheck.position, groundCheckSize);
        }

        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, playerRange);
    }
    #endif
}