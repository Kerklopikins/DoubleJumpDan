using UnityEngine;

public class BasicEnemyAI : MonoBehaviour
{
    [Header("Main Stuff")]
    [SerializeField] float speed;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0, 0.125f);
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform edgeCheck;
    [SerializeField] bool useRigidBody;

    [Header("Player Detection")]
    [SerializeField] float playerRange = 10;
    [SerializeField] DetectionType detectionType;

    [Header("Jumper")]
    [SerializeField] float jumpTimer;
    [SerializeField] float jumpHeight;
 
    [Header("Stopper")]
    [SerializeField] bool isAStopper;
    [SerializeField] float secondsToPauseWhenSeesPlayer;
    [SerializeField] float angrySpeed;
    [SerializeField] bool onlyCheckEdgeWhenHappy;
    [SerializeField] bool chases;
    [SerializeField] WatchOutTrigger watchOutTrigger;
    
    enum DetectionType { AllAround, UseRaycast, Front};
    public float _secondsToPauseWhenSeesPlayer { get; protected set; }
    public bool seesPlayer { get; protected set; }
    public Rigidbody2D rb2D { get; protected set; }
    public Player player { get; protected set; }
    public bool playerInRange { get; protected set; }
    float interval = 1;
    float timer;
    Vector2 direction;
    bool notAtEdge;
    Animator animator;
    float _jumpTimer;
    bool grounded;
    bool hittingWall;
    float _angrySpeed;
    bool triggeredWatchOut;
    float playerDistance;
    int raycastMask;
    float lastPositionX;
    Health health;
    Collider2D _collider;
    void Start()
    {
        direction = new Vector2(-transform.localScale.x, 0);
        _jumpTimer = jumpTimer;
        _secondsToPauseWhenSeesPlayer = secondsToPauseWhenSeesPlayer;
        rb2D = GetComponent<Rigidbody2D>();
        timer = interval;
        _angrySpeed = angrySpeed;
        animator = GetComponent<Animator>();
        player = LevelManager.Instance.player;
        raycastMask = LayerMask.GetMask("Collisions", "Player");
        health = GetComponent<Health>();
        _collider = GetComponent<Collider2D>();

        lastPositionX = transform.position.x;
    }

    void Update()
    {
        if(health.Dead())
        {
            animator.enabled = false;
            _collider.enabled = false;

            if(rb2D != null)
                rb2D.simulated = false;

            this.enabled = false;
            return;
        }

        lastPositionX = transform.position.x;

        if(detectionType != DetectionType.UseRaycast)
            playerDistance = Vector3.Distance(player.transform.position, transform.position);
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, playerRange, raycastMask);

            if(hit && hit.collider.CompareTag("Player"))
                playerInRange = true;
            else
                playerInRange = false;
        }

        timer -= Time.deltaTime;

        if(playerRange > 0)
        {
            if(!player.dead)
            {
                if(detectionType != DetectionType.UseRaycast)
                {
                    if(playerDistance <= playerRange)
                        playerInRange = true;
                    else
                        playerInRange = false;
                }

                if(detectionType == DetectionType.Front)
                {
                    if(playerInRange)
                    {
                        if(rb2D.velocity.x < 0 && player.transform.position.x < transform.position.x || rb2D.velocity.x > 0 && player.transform.position.x > transform.position.x)
                            seesPlayer = true;
                        else
                            seesPlayer = false;
                    }
                    else
                    {
                        seesPlayer = false;
                    }
                }
                else
                {
                    if(playerInRange)
                        seesPlayer = true;
                    else
                        seesPlayer = false;
                }
            }
            else
            {
                playerInRange = false;
                seesPlayer = false;
            }
        }

        if(groundCheck != null)
            grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, collisionMask);
        
        hittingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, collisionMask);

        if(hittingWall && direction.x < 0)
            hittingWall = true;
        else if(!hittingWall && direction.x > 0)
            hittingWall = false;

        if(groundCheck != null)
            if(animator != null && jumpHeight > 0)
                if(animator.GetBool("Grounded"))
                    animator.SetBool("Grounded", grounded);

        if(groundCheck != null)
        {
            if(jumpHeight > 0)
            {
                _jumpTimer -= Time.deltaTime;

                if(_jumpTimer <= 0)
                    _jumpTimer = jumpTimer;

                if(_jumpTimer == jumpTimer && grounded)
                    rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
            }
        }
        
        if(edgeCheck != null)
        {
            notAtEdge = Physics2D.OverlapCircle(edgeCheck.position, 0.1f, collisionMask);

            if(isAStopper)
            {
                if(seesPlayer)
                {
                    if(onlyCheckEdgeWhenHappy)
                    {
                        if(!notAtEdge)
                            angrySpeed = _angrySpeed;
                    }
                    else if(!onlyCheckEdgeWhenHappy)
                    {
                        if(!notAtEdge)
                        {
                            angrySpeed = 0;
                            lastPositionX = transform.position.x;
                        }
                    }
                }
                
                if(notAtEdge)
                {
                    angrySpeed = _angrySpeed;
                }
            }

            if((direction.x < 0 && hittingWall) || (direction.x > 0 && hittingWall || !notAtEdge))
            {
                direction = -direction;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            if((direction.x < 0 && hittingWall) || (direction.x > 0 && hittingWall))
            {
                direction = -direction;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }

        if(isAStopper)
        {
            if(!seesPlayer)
            {
                if(useRigidBody)
                    rb2D.velocity = new Vector2(direction.x * speed, rb2D.velocity.y);
                else
                    transform.Translate(direction.x * speed * Time.deltaTime, 0, 0);
            }
        }
        else if(!isAStopper)
        {
            if(useRigidBody)
                rb2D.velocity = new Vector2(direction.x * speed, rb2D.velocity.y);
            else
                transform.Translate(direction.x * speed * Time.deltaTime, 0, 0);
        }

        if(isAStopper)
        {
            if(timer <= 0)
                timer = interval;

            if(playerInRange)
                _secondsToPauseWhenSeesPlayer -= Time.deltaTime;
            else
                _secondsToPauseWhenSeesPlayer = secondsToPauseWhenSeesPlayer;

            if(seesPlayer && _secondsToPauseWhenSeesPlayer > 0)
            {
                if(useRigidBody)
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

                    if(watchOutTrigger != null)
                        watchOutTrigger.Activate();
                }                    
            }

            if(seesPlayer && _secondsToPauseWhenSeesPlayer <= 0)
            {
                if(chases)
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

                    if(hittingWall)
                        angrySpeed = 0;
                }

                if(useRigidBody)
                    rb2D.velocity = new Vector2(direction.x * angrySpeed, rb2D.velocity.y);
                else
                    transform.Translate(direction.x * angrySpeed * Time.deltaTime, 0, 0);
            }

            if(useRigidBody)
            {
                if(rb2D.velocity.x == 0)
                    animator.SetBool("Stopped", true);
                else
                    animator.SetBool("Stopped", false);
            }
            else
            {
                float distanceFromLastPos = Mathf.Abs(transform.position.x - lastPositionX);

                if(distanceFromLastPos <= 0)
                    animator.SetBool("Stopped", true);
                else
                    animator.SetBool("Stopped", false);
            }
            
            animator.SetBool("Player In Range", seesPlayer);
            
            if(!seesPlayer)
                triggeredWatchOut = false;
        }
    }
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
}