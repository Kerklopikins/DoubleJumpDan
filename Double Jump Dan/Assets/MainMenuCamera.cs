using UnityEngine;

public class MainMenuCamera: MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float accelerationTimeGrounded;
    public float accelerationTimeInAir;
    public float jumpHeight;
    [SerializeField] Vector2 stopTimeMinMax;
    [SerializeField] Vector2 runTimerMinMax;
    [SerializeField] Vector2 jumpTimerMinMax;
    [SerializeField] float doubleJumpDelay;
    [SerializeField] int maxXDistance;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0, 0.125f);
    [SerializeField] Vector3 groundCheckOffset;

    [Header("Particle Effects")]
    [SerializeField] ParticleSystem doubleJumpParticles;
    [SerializeField] ParticleSystem walkParticles;
    [SerializeField] ParticleSystem landParticles;    

    public bool grounded { get; private set; }
    public bool canFall { get; set; }
    public float fallButtonTimer { get; set; }
    bool doubleJump;
    Rigidbody2D rb2D;
    Animator animator;
    float velocityXSmoothing;
    float doubleJumpTimer;
    Vector2 input;
    int direction = 1;
    bool wasGroundedLastFrame;
    float previousVelocityY;
    float runTimer;
    float jumpTimer;
    float stopTimer;
    float doubleJumpProbibility;
    ShopManager shopManager;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        shopManager = GameObject.FindWithTag("Main Menu").GetComponent<ShopManager>();
        shopManager.OnShopItemsChanged += UpdateSkinAndHats;

        runTimer = Random.Range(runTimerMinMax.x, runTimerMinMax.y);
        jumpTimer = Random.Range(jumpTimerMinMax.x, jumpTimerMinMax.y);
        stopTimer = Random.Range(stopTimeMinMax.x, stopTimeMinMax.y);
    }

    void UpdateSkinAndHats()
    {
        
    }

    void Update()
    {
        if(grounded)
        {
            jumpTimer -= Time.deltaTime;
            doubleJump = false;
        }
        
        runTimer -= Time.deltaTime;

        if(runTimer <= 0)
        {
            stopTimer -= Time.deltaTime;

            if(stopTimer <= 0)
            {
                direction = -direction;
                runTimer = Random.Range(runTimerMinMax.x, runTimerMinMax.y);
                stopTimer = Random.Range(stopTimeMinMax.x, stopTimeMinMax.y);
            }

            input = new Vector2(0, input.y);
        }
        else
        {
            input = new Vector2(direction, input.y);
        }

        transform.localScale = new Vector3(direction, 1, 1);
        
        Jump();
        SetAnimationsAndWalkEffects();
    }

    #region PhysicsAndMovement
    void FixedUpdate()
    {
        if(rb2D.velocity.y <= -25)
            rb2D.velocity = new Vector2(rb2D.velocity.x, -25);

        float targetVelocityX = input.x * speed;
        float smoothedX = Mathf.SmoothDamp(rb2D.velocity.x, targetVelocityX, ref velocityXSmoothing, (grounded) ? accelerationTimeGrounded : accelerationTimeInAir);

        if(float.IsNaN(velocityXSmoothing))
            velocityXSmoothing = 0;

        if(float.IsNaN(smoothedX))
            smoothedX = 0;

        rb2D.velocity = new Vector2(smoothedX, rb2D.velocity.y);
        CalculateGroundingAndLanding();
    }

    void Jump()
    {
        if(jumpTimer <= 0 && grounded)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
            doubleJumpTimer = doubleJumpDelay;
            
            doubleJumpProbibility = Random.value;
            jumpTimer = Random.Range(jumpTimerMinMax.x, jumpTimerMinMax.y);
        }

        if(doubleJumpProbibility < 0.5f)
            return;

        doubleJumpTimer -= Time.deltaTime;
        doubleJumpTimer = Mathf.Clamp(doubleJumpTimer, 0, doubleJumpDelay);

        if(!doubleJump && !grounded && doubleJumpTimer <= 0)
        {
            doubleJumpParticles.Play();
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
            doubleJump = true;
        }
    }

    void CalculateGroundingAndLanding()
    {
        grounded = Physics2D.OverlapBox(groundCheck.position + groundCheckOffset, groundCheckSize, 0, collisionMask);

        if(grounded && !wasGroundedLastFrame)
        {
            float impactSpeed = Mathf.Abs(previousVelocityY);
            
            if(impactSpeed > 1.2f)
                TriggerLandParticles(impactSpeed);
        }

        previousVelocityY = rb2D.velocity.y;
        wasGroundedLastFrame = grounded;
    }

    #endregion

    #region Effects

    void SetAnimationsAndWalkEffects()
    {
        float velocityXAbs = Mathf.Abs(rb2D.velocity.x);
        bool isWalkingOnGround = grounded && velocityXAbs > 0.5f;

        ParticleSystem.EmissionModule emission = walkParticles.emission;
        float emmisionRate = isWalkingOnGround ? Mathf.Lerp(10, 30, velocityXAbs / 8) : 0;

        emission.rateOverTime = emmisionRate;

        animator.SetBool("Grounded", grounded);

        if(grounded)
        {
            if(velocityXAbs > 0.3f)
                animator.SetFloat("Speed", velocityXAbs);
            else
                animator.SetFloat("Speed", 0);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    void TriggerLandParticles(float impactSpeed)
    {
        float t = Mathf.Clamp01((impactSpeed - 1f) / 24f);

        int burstAmount = Mathf.RoundToInt(Mathf.Lerp(1, 15, t));

        landParticles.Emit(burstAmount);

        var shape = landParticles.shape;
        shape.radius = Mathf.Lerp(0.5f, 1, t);
    }

    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(groundCheck.position + groundCheckOffset, groundCheckSize);
    }
}