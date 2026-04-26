using System;
using System.Net.Sockets;
using UnityEngine;

public class CrorgnAI : MonoBehaviour
{
    [Header("Main Stuff")]
    [SerializeField] float speed;
    [SerializeField] SpriteRenderer eye;
    [SerializeField] Sprite[] eyeSprites;
    [SerializeField] Transform bodyPivot;
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform edgeCheck;
    [SerializeField] bool useRigidBody;

    [Header("Player Detection")]
    [SerializeField] float playerRange = 10;

    [SerializeField] float shootStartDelay;

    Rigidbody2D rb2D;
    Player player;
    bool playerInLineOfSight;
    int moveDirection;
    bool notAtEdge;
    bool hittingWall;
    int layerMask;
    EnemyGun enemyGun;
    float wallHitDistance;
    float _speed;
    Animator animator;
    Health health;
    Collider2D _collider;
    float detectionTimer;
    State state;
    public enum State { AttackState, NormalState }
    float _shootStartDelay;

    void Start()
    {
        moveDirection = -(int)transform.localScale.x;
        rb2D = GetComponent<Rigidbody2D>();
        player = LevelManager.Instance.player;
        enemyGun = GetComponent<EnemyGun>();
        animator = GetComponent<Animator>();
        layerMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Player"));
        health = GetComponent<Health>();
        _collider = GetComponent<Collider2D>();
        detectionTimer = 0.1f;
        state = State.NormalState;
    }

    void Update()
    {
        if(health.Dead())
        {
            animator.enabled = false;
            _collider.enabled = false;
            enemyGun.enabled = false;
            
            if(rb2D != null)
                rb2D.simulated = false;

            this.enabled = false;
            return;
        }

        hittingWall = Physics2D.Raycast(wallCheck.position, Vector3.left * transform.localScale.x, wallHitDistance, 1 << LayerMask.NameToLayer("Collisions"));
        notAtEdge = Physics2D.OverlapCircle(edgeCheck.position, 0.1f, 1 << LayerMask.NameToLayer("Collisions"));
        detectionTimer -= Time.deltaTime;

        Vector2 difference = (Vector2)player.transform.position - (Vector2)transform.position;

        if(detectionTimer <= 0)
        {
            float sqrDistance = difference.sqrMagnitude;

            if(sqrDistance < playerRange * playerRange)
            {
                Vector2 direction = difference.normalized;
                Ray2D ray = new Ray2D((Vector2)transform.position, direction);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, playerRange, layerMask);
                
                //if(hit)
                    //Debug.DrawLine(ray.origin, hit.point, Color.red);

                if(hit && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    _shootStartDelay -= Time.deltaTime;

                    if(_shootStartDelay <= 0)
                        enemyGun.Shoot();

                    playerInLineOfSight = true;
                    state = State.AttackState;
                }
                else
                {
                    playerInLineOfSight = false;
                    state = State.NormalState;
                }
            }
            else
            {
                playerInLineOfSight = false;
                state = State.NormalState;
            }

            detectionTimer = 0.1f;
        }

        if(state == State.NormalState)
            SetNormalState();
        else if(state == State.AttackState)
            SetAttackState(difference);
        
        animator.SetBool("Player In Range", playerInLineOfSight);
        transform.localScale = new Vector3(-moveDirection, transform.localScale.y, transform.localScale.z);
    }

    void SetAttackState(Vector3 difference)
    {
        eye.sprite = eyeSprites[0];

        float localDirectionX = transform.lossyScale.x > 0 ? -difference.x : difference.x;
        float rotZ = Mathf.Atan2(difference.y, localDirectionX) * Mathf.Rad2Deg;
        float rotZOffset = transform.lossyScale.x > 0 ? -rotZ : rotZ;

        enemyGun.bulletFireLocations[0].rotation = Quaternion.Euler(0, 0, rotZOffset);

        RotateBody(rotZOffset);            

        wallHitDistance = 4;

        if(!notAtEdge)
            _speed = 0;
        else
            _speed = hittingWall ? _speed = 0 : _speed = speed;

        float xDistance = Mathf.Abs(player.transform.position.x - transform.position.x);

        if(xDistance > 1 && PlayerBehind())
            moveDirection = -moveDirection;

        Move();
    }
    
    void SetNormalState()
    {
        _shootStartDelay = shootStartDelay;

        wallHitDistance = 0.25f;
        _speed = speed;    

        if(hittingWall || !notAtEdge)
            moveDirection = -moveDirection;

        Move();

        eye.sprite = eyeSprites[1];
        RotateBody(0);
    }
    
    void Move()
    {
        if(useRigidBody)
            rb2D.velocity = new Vector2(moveDirection * _speed, rb2D.velocity.y);
        else
            transform.Translate(moveDirection * _speed * Time.deltaTime, 0, 0);

        animator.SetInteger("Speed", (int)_speed);
    }

    bool PlayerBehind()
    {
        if(player.transform.position.x > transform.position.x && moveDirection == -1 || player.transform.position.x < transform.position.x && moveDirection == 1)
            return true;
        else 
            return false;
    }

    void RotateBody(float rotZOffset)
    {
        rotZOffset = Math.Clamp(rotZOffset, -45, 45);
        bodyPivot.rotation = Quaternion.Slerp(bodyPivot.rotation, Quaternion.Euler(0, 0, rotZOffset), Time.deltaTime * 10);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, playerRange);
    }
}