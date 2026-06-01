using System.Collections;
using UnityEngine;

public class TrediconAI : MonoBehaviour
{
    [Header("Main Stuff")]
    [SerializeField] float speed;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Transform topWallCheck;
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform edgeCheck;
    [SerializeField] Transform farEdgeCheck;
    [SerializeField] float detectionHeightOffset;
    [SerializeField] float playerRange = 10;
    [SerializeField] float secondsToPauseWhenSeesPlayer;
    [SerializeField] float angrySpeed;
    [SerializeField] TrediconRocketLauncher rocketLauncher;
    [SerializeField] AudioClip transformSound;
    [SerializeField] WatchOutTrigger watchOutTrigger;
    [SerializeField] Collider2D[] colliders;

    float _secondsToPauseWhenSeesPlayer;
    Rigidbody2D rb2D;
    Player player;
    bool notAtEdge;
    Animator animator;
    bool hittingWall;
    float _angrySpeed;
    bool sawPlayer;
    float detectionTimer;
    Health health;
    int layerMask;
    bool playerInLineOfSight;
    State state;
    public enum State { AttackState, NormalState }
    public float climbHeight = 1.2f;
    public float climbEndPositionMultiplier;
    public float decendEndPositionMultiplier;
    public float climbTiltAngle = 35;
    public float decendTiltAngle = 35;
    public float tiltDuration = 0.3f;
    public float climbDuration = 0.5f;
    public float levelDuration = 0.3f;
    public float wallCheckDistance = 0.2f;
    /////1, 0.75
    public Vector3 climbPivotOffset;
    public Vector3 decendPivotOffset;
    bool isClimbing = false;
    int moveDirection = 1;
    Vector2 lastPosition;

    void Start()
    {
        moveDirection = -(int)transform.localScale.x;
        rb2D = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        _angrySpeed = angrySpeed;
        animator = GetComponent<Animator>();
        player = LevelManager.Instance.player;
        layerMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Player"));
        rocketLauncher.player = player;

        detectionTimer = 0.1f;
        state = State.NormalState;
    }

    void Update()
    {
        if(health.Dead())
        {
            animator.enabled = false;
            watchOutTrigger.gameObject.SetActive(false);

            for(int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;

            rocketLauncher.enabled = false;
            
            if(rb2D != null)
                rb2D.simulated = false;

            this.enabled = false;
            return;
        }

        detectionTimer -= Time.deltaTime;

        Vector2 origin = (Vector2)transform.position + new Vector2(0, detectionHeightOffset);
        Vector2 target = (Vector2)player.transform.position;

        Vector2 difference = target - origin;

        CheckForWall();
        CheckForEdge();

        if(detectionTimer <= 0)
        {
            float sqrDistance = difference.sqrMagnitude;

            if(sqrDistance < playerRange * playerRange)
            {
                Vector2 direction = difference.normalized;

                Ray2D ray = new Ray2D(origin, direction);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, playerRange, layerMask);
                
                if(hit)
                    Debug.DrawLine(ray.origin, hit.point, Color.red);

                if(hit && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
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

        lastPosition = transform.position;

        if(state == State.NormalState && !isClimbing)
            SetNormalState();
        else if(state == State.AttackState)
            SetAttackState();
        
        bool stopped;

        if(lastPosition.x != transform.position.x || lastPosition.y != transform.position.y || isClimbing)
            stopped = false;
        else
            stopped = true;

        animator.SetBool("Stopped", stopped);
        animator.SetBool("Player In Range", playerInLineOfSight);
    }

    void CheckForWall()
    {
        Vector2 origin = wallCheck.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * -moveDirection, wallCheckDistance, layerMask);

        if(state == State.NormalState)
        {
            if(hit.collider != null)
                hittingWall = true;
            else
                hittingWall = false;
        }
        else if(state == State.AttackState)
        {
            Vector2 topWallCheckOrigin = topWallCheck.position;
            RaycastHit2D topWallHit = Physics2D.Raycast(topWallCheckOrigin, Vector2.right * -moveDirection, 0.5f, layerMask);

            if(topWallHit.collider != null)
            {
                hittingWall = true;
                return;
            }
            else
            {
                hittingWall = false;
            }

            if(hit.collider != null && !isClimbing)
                StartCoroutine(ClimbSequence());
        }
    }

    void CheckForEdge()
    {
        RaycastHit2D mainEdgeHit = Physics2D.Raycast(edgeCheck.position, Vector2.down, 0.2f, layerMask);

        if(state == State.NormalState)
        {
            if(mainEdgeHit.collider != null)
                notAtEdge = true;
            else
                notAtEdge = false;
        }
        if(state == State.AttackState)
        {
            RaycastHit2D farEdgeHit = Physics2D.Raycast(farEdgeCheck.position, Vector2.down, 50, layerMask);

            if(farEdgeHit.distance > 2.5f && !isClimbing)
            {
                notAtEdge = false;
                _angrySpeed = 0;
                return;
            }
            else
            {
                notAtEdge = true;
                _angrySpeed = angrySpeed;
            }

            if(mainEdgeHit.collider == null && !isClimbing)
                StartCoroutine(DecendSequence());
        }
    }

    IEnumerator ClimbSequence()
    {
        isClimbing = true;
        yield return RotateTo(climbTiltAngle * -transform.localScale.x, tiltDuration, climbPivotOffset);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(moveDirection * climbEndPositionMultiplier, climbHeight, 0);
        yield return MoveTo(endPosition, climbDuration);

        yield return RotateTo(0, levelDuration, climbPivotOffset);
        isClimbing = false;
    }

    IEnumerator DecendSequence()
    {
        isClimbing = true;
        float pivot = transform.localScale.x > 0 ? decendPivotOffset.x : -decendPivotOffset.x;

        yield return RotateTo(decendTiltAngle * transform.localScale.x, tiltDuration, decendPivotOffset);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(moveDirection * decendEndPositionMultiplier, -climbHeight, 0);
        yield return MoveTo(endPosition, climbDuration);

        yield return RotateTo(0, levelDuration, decendPivotOffset);
        isClimbing = false;
    }
    
    IEnumerator RotateTo(float targetZ, float duration, Vector3 _pivotOffset)
    {
        float startZ = transform.eulerAngles.z;

        if(startZ > 180)
            startZ -= 360;

        Vector3 pivot = transform.TransformPoint(_pivotOffset);

        float inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            float t = Mathf.Clamp01(inTime / duration);
            float easedT = t * t * (3 - 2 * t);
            float angle = Mathf.Lerp(startZ, targetZ, easedT);

            float delta = angle - (transform.eulerAngles.z > 180 ? transform.eulerAngles.z - 360 : transform.eulerAngles.z);
            
            transform.RotateAround(pivot, Vector3.forward, delta);
            //transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, targetZ);
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float t = 0;

        while(t < duration)
        {
            t += Time.deltaTime;
            //float easedT = Mathf.SmoothStep(0, 1, t / duration);
            transform.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }

        transform.position = target;
    }

    void SetNormalState()
    {
        sawPlayer = false;

        _secondsToPauseWhenSeesPlayer = secondsToPauseWhenSeesPlayer;

        if(hittingWall || !notAtEdge)
            moveDirection = -moveDirection;

        transform.localScale = new Vector3(-moveDirection, transform.localScale.y, transform.localScale.z);
        transform.Translate(moveDirection * speed * Time.deltaTime, 0, 0);
    }

    void SetAttackState()
    {
        _secondsToPauseWhenSeesPlayer -= Time.deltaTime;

        if(!notAtEdge || hittingWall)
            _angrySpeed = 0;
        else
            _angrySpeed = angrySpeed;

        if(_secondsToPauseWhenSeesPlayer > 0)
        {
            transform.Translate(0, 0, 0);

            if(!sawPlayer)
            {
                if(!isClimbing)
                {
                    moveDirection = player.transform.position.x > transform.position.x ? 1 : -1;
                    transform.localScale = new Vector3(-moveDirection, transform.localScale.y, transform.localScale.z);
                }
                
                sawPlayer = true;
				watchOutTrigger.Activate();
            }
        }
        else
        {
            rocketLauncher.Shoot();

            float xDistance = Mathf.Abs(player.transform.position.x - transform.position.x);

            if(xDistance > 1)
            {
                if(!isClimbing)
                {
                    moveDirection = player.transform.position.x > transform.position.x ? 1 : -1;
                    transform.localScale = new Vector3(-moveDirection, transform.localScale.y, transform.localScale.z);
                    transform.Translate(moveDirection * _angrySpeed * Time.deltaTime, 0, 0);
                } 
            }
            else
            {
                transform.Translate(0, 0, 0);
            }
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 1);

        Gizmos.DrawWireSphere(transform.TransformPoint(climbPivotOffset), 0.125f);
        Gizmos.DrawWireSphere(transform.TransformPoint(decendPivotOffset), 0.125f);

        Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(0, climbPivotOffset.y, climbPivotOffset.z)), 0.25f);

        Gizmos.DrawWireSphere(topWallCheck.position, 0.125f);
        Gizmos.DrawWireSphere(wallCheck.position, 0.125f);
        Gizmos.DrawWireSphere(edgeCheck.position, 0.125f);
        Gizmos.DrawWireSphere(farEdgeCheck.position, 0.125f);
        
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, detectionHeightOffset, 0), playerRange);
    }
    #endif
}