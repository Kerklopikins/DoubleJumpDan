using UnityEngine;

public class MainMenuPlayer: MonoBehaviour
{
    [SerializeField] Camera _camera;

    [Header("Movement")]
    public float speed;
    public float accelerationTimeGrounded;
    public float accelerationTimeInAir;
    public float jumpHeight;
    [SerializeField] float coyoteTime;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0, 0.125f);
    [SerializeField] Vector3 groundCheckOffset;
    
    [Header("Rotating Arm")]
    [SerializeField] Transform armTwo;
    public Transform aimPoint;

    [Header("Eye Movement")]
    public Transform pupilsParent;
    public Vector2 maxPupilsOffset;
    public float horizontalSensitivity;
    public float verticalSensitivity;
    public float minThreshold;
    public float maxThreshold;
    public float maxMouseDistance = 5;
    public float smoothSpeed;
    public Transform eyeBrow;

    [Header("Particle Effects")]
    [SerializeField] ParticleSystem doubleJumpParticles;
    [SerializeField] ParticleSystem walkParticles;
    [SerializeField] ParticleSystem landParticles;    

    public bool canHandleInput { get; set; }
    public bool grounded { get; private set; }
    public bool canFall { get; set; }
    public float fallButtonTimer { get; set; }
    bool doubleJump;
    Rigidbody2D rb2D;
    Animator animator;
    float velocityXSmoothing;
    float _coyoteTime;
    float doubleJumpTimer;
    bool canJump;
    float fallButtonDelay = 0.1f;
    bool isGrounded;
    Vector2 input;
    Vector3 lastAimDirection;
    int direction = 1;
    bool wasGroundedLastFrame;
    float previousVelocityY;
    Vector3 pupilsParentStartPosition;
    Vector3 eyeBrowStartPosition;
    float targetX;
    float targetY;
    GameInputManager gameInputManager;
    
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        _coyoteTime = coyoteTime;       
        doubleJumpTimer = 0.15f;

        pupilsParentStartPosition = pupilsParent.localPosition;
        eyeBrowStartPosition = eyeBrow.localPosition;
        gameInputManager = GameInputManager.Instance;
        canHandleInput = true;
    }

    void Update()
    {
        if(grounded)
        {
            _coyoteTime = coyoteTime;
            doubleJump = false;
        }
        else
        {
            _coyoteTime -= Time.deltaTime;
            _coyoteTime = Mathf.Clamp(_coyoteTime, 0, coyoteTime);
        }
        
        if(canHandleInput)
        {
            HandleInput();
            SetAnimationsAndWalkEffects();
        }
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
        if(fallButtonTimer > 0)
            fallButtonTimer -= Time.deltaTime;

        if(grounded && fallButtonTimer <= 0 && gameInputManager.GetVerticalInput() < 0)
            fallButtonTimer = 0;
        else if(!grounded)
            fallButtonTimer = fallButtonDelay;

        if(rb2D.velocityY < 0)
            fallButtonTimer = fallButtonDelay;

        if(!canJump)
            return;

        if(gameInputManager.JumpButtonDown() && _coyoteTime > 0)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
            doubleJumpTimer = 0.15f;
        }

        doubleJumpTimer -= Time.deltaTime;
        doubleJumpTimer = Mathf.Clamp(doubleJumpTimer, 0, 0.15f);

        if(gameInputManager.JumpButtonUp() && !grounded)
            _coyoteTime = 0;

        if(gameInputManager.JumpButtonDown() && !doubleJump && !grounded && _coyoteTime <= 0 && doubleJumpTimer <= 0)
        {
            doubleJumpParticles.Play();
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);
            doubleJump = true;
        }
    }

    void CalculateGroundingAndLanding()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheck.position + groundCheckOffset, groundCheckSize, 0, collisionMask);
        float ySpeed = Mathf.Abs(rb2D.velocityY);
        SetGrounded(hit);

        if(ySpeed >= 0.01 && hit != null && hit.CompareTag("One Way Platform"))
        {
            grounded = false;
            canJump = false;
        }
        else if(ySpeed < 0.01 && hit != null && hit.CompareTag("One Way Platform"))
        {
            grounded = true;
            canJump = true;
        }
        else
        {
            canJump = true;
            grounded = hit;
        }

        if(grounded && !wasGroundedLastFrame)
        {
            float impactSpeed = Mathf.Abs(previousVelocityY);
            
            if(impactSpeed > 1.2f)
                TriggerLandParticles(impactSpeed);
        }

        previousVelocityY = rb2D.velocity.y;
        wasGroundedLastFrame = grounded;
    }

    public void SetGrounded(bool _grounded)
    {
        if(!isGrounded && _grounded)
            fallButtonTimer = fallButtonDelay;

        isGrounded = _grounded;
    }

    void RotateArm()
    {
        Vector3 realMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        float mouseDistance = Vector2.Distance(transform.position, realMousePosition);
        Vector3 difference;

        if(mouseDistance > 2)
        {
            difference = realMousePosition - aimPoint.position;
            difference.Normalize();
            
            lastAimDirection = difference;
        }
        else
        {
            difference = lastAimDirection;
        }

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        armTwo.rotation = Quaternion.Slerp(armTwo.rotation, Quaternion.Euler(0, 0, rotZ + 90), Time.deltaTime * 10);    
    }

    #endregion

    #region Input

    public void HandleInput()
    {
        input = new Vector2(gameInputManager.GetHorizontalInput(), gameInputManager.GetVerticalInput());

        if(!gameInputManager.SprintButton())
        {
            if(input.x > 0)
                direction = 1;
            else if(input.x < 0)
                direction = -1;

            transform.localScale = new Vector3(direction, 1, 1);
        }

        RotateArm();
        RotateEyes();
        Jump();
    }

    public void ResetDownTimer()
    {
        fallButtonTimer = fallButtonDelay;
    }

    #endregion

    #region Effects
    void RotateEyes()
    {
        Vector3 realMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        realMousePosition.z = pupilsParent.position.z;
        Vector3 direction = realMousePosition - transform.position;
        
        direction = new Vector3(direction.x * transform.localScale.x, direction.y, 0);
        float distance = direction.magnitude;
        
        float t = Mathf.Clamp01(distance / maxMouseDistance);
        float thresholdX = Mathf.Lerp(minThreshold, maxThreshold, t) + maxPupilsOffset.x;
        float thresholdY = Mathf.Lerp(minThreshold, maxThreshold, t) + maxPupilsOffset.y;
        
        float scaledX = direction.x * horizontalSensitivity;
        float scaledY = direction.y * verticalSensitivity;
        
        if(scaledX > thresholdX)
            targetX = maxPupilsOffset.x;
        else if(scaledX < -thresholdX)
            targetX = -maxPupilsOffset.x;
        else
            targetX = maxPupilsOffset.x;

        if(scaledY > thresholdY)
            targetY = maxPupilsOffset.y;
        else if(scaledY < -thresholdY)
            targetY = -maxPupilsOffset.y;
        else
            targetY = -maxPupilsOffset.y;

        Vector3 targetPosition = pupilsParentStartPosition + new Vector3(targetX, targetY, 0);
        Vector3 eyebrowTargetPosition = eyeBrowStartPosition + new Vector3(0, targetY, 0);
        
        pupilsParent.localPosition = Vector3.Lerp(pupilsParent.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
                
        if(eyeBrow.gameObject.activeSelf)
            eyeBrow.localPosition = Vector3.Lerp(eyeBrow.localPosition, eyebrowTargetPosition, Time.deltaTime * smoothSpeed);    
    }

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

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(groundCheck.position + groundCheckOffset, groundCheckSize);
    }
    #endif
}