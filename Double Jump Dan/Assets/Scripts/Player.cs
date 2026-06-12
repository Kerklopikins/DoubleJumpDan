using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Player: MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 10;
    public float sprintSpeed = 13;
    public float inAirSprintSpeed;
    public float accelerationTimeGrounded;
    public float accelerationTimeInAir;
    public float jumpHeight;
    [SerializeField] float coyoteTime;
    [SerializeField] Transform body;
    [SerializeField] Vector2 bodyRotateMinMax;
    [SerializeField] float bodyRotateSpeed;

    [Header("Health")]
    public int health;
    public bool invincible;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0, 0.125f);
    [SerializeField] Vector3 groundCheckOffset;
    
    [Header("Rotating Arm")]
    [SerializeField] Transform armTwo;
    public Transform aimPoint;

    [Header("Eye Movement")]
    [SerializeField] Transform pupilsParent;
    [SerializeField] Vector2 pupilsParentStartPosition;
    [SerializeField] Vector2 maxPupilsOffset;
    [SerializeField] float horizontalSensitivity;
    [SerializeField] float verticalSensitivity;
    [SerializeField] float minThreshold;
    [SerializeField] float maxThreshold;
    [SerializeField] float maxMouseDistance = 5;
    [SerializeField] float smoothSpeed;
    [SerializeField] Transform eyeBrow;

    [Header("Effects")]
    [SerializeField] GameObject destroyedEffect;
    [SerializeField] GameObject hurtEffect;
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip destroyedSound;
    [SerializeField] SpriteRenderer weeSprite;
    [SerializeField] Sprite[] weeSprites;
    [SerializeField] AudioClip weeSound;
    
    [Header("Controller Rumble")]
    [SerializeField] float lowHurtRumble;
    [SerializeField] float highHurtRumble;
    [SerializeField] float hurtRumbleDuration;
    [SerializeField] float lowDeathRumble;
    [SerializeField] float highDeathRumble;
    [SerializeField] float deathRumbleDuration;

    [Header("Particle Effects")]
    [SerializeField] ParticleSystem doubleJumpParticles;
    [SerializeField] ParticleSystem walkParticles;
    [SerializeField] ParticleSystem landParticles;    

    [Header("Misc")]
    public List<SpriteRenderer> spriteMaterials = new List<SpriteRenderer>();

    [Header("Camera Shake")]
    public CameraManager.Properties properties;
    
    //Event Actions
    public event Action OnPlayerHurt;
    public event Action OnPlayerKilled;
    public event Action OnPlayerRespawn;
    public event Action OnPlayerHealthChange;
    public event Action OnPlayerTeleported;

    //Physics
    float _speed;
    public bool grounded { get; private set; }
    public float fallButtonTimer { get; set; }
    public int direction { get; private set; }
    float velocityXSmoothing;
    bool doubleJump;
    float _fallTimer;
    float _coyoteTime;
    float doubleJumpTimer;
    bool canJump;
    float fallButtonDelay = 0.1f;
    bool isGrounded;
    float _knockBackInputDelayTimer;
    bool wasGroundedLastFrame;
    float previousVelocityY;
    
    //Input
    Vector2 input; //For movement
    int xInput; //For movement
    int yInput; //For movement
    float smoothedX;
    Vector3 lastAimDirection; //For arm
    Vector3 difference; //For arm
    public Transform crosshairs { get; set; }
    bool teleporting = false;
    float bodyTargetRotation;
    float currentBodyRotation;

    //Animation
    float walkAnimationSpeed; //For speeding up when sprinting
    float walkAnimationT; //For speeding up when sprinting
    float velocityXAbs; //For animation and particle systems

    //Rotating Eyes
    Vector2 eyeBrowStartPosition;
    Vector3 eyeLookDirection;
    float targetX;
    float targetY;

    //Health, damage, lives, kill, respawn
    public SpriteRenderer gunSpriteRenderer { get; set; }
    public bool dead { get; private set; }
    public int _health { get; private set; }
    public int lives { get; set; }
    float hurtTimer = 0.125f;
    float _hurtTimer;

    //Gun fling when killed or finished level
    public Transform gunTransform { get; set; }
    public Vector2 gunStartPosition { get; set; }
    bool flingRight = true;
    bool flingedGun;

    //Finish Level
    Camera _camera;
    float finishLevelJumpTimer;
    float finishLevelInTime;
    float finalPositionX;
    bool firstFinishJump;

    //Flash
    WaitForSeconds flashSpeed = new WaitForSeconds(0.07f);

    //Game HUD
    public bool gameHUDPaused { get; set; }
    public bool gameHUDFrozen { get; set; }

    //Wee effect
    float weeEffectFallThreshold = 2.5f; //How long to fall until wee is activated
    bool animatedWee;
    bool canAnimateWee = true;
    
    //Particle Effects
    float walkParticlesEmmisionRate;
    bool isWalkingOnGround;

    //Camera
    public bool canFollow { get; set; } //Called when killed or finished level

    //References
    GameManager gameManager;
    Rigidbody2D rb2D;
    GameInputManager gameInputManager;
    Animator animator;
    Collider2D _collider2D;
    ShadowDanTracer shadowDanTracer;
    MaterialPropertyBlock materialPropertyBlock;
    
    void Awake()
    {
        lives = 3;
	}

    void Start()
    {
        _health = health;
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        gameManager = GameManager.Instance;
        _camera = LevelManager.Instance.mainCamera;

        direction = 1;

        canFollow = true;
        _coyoteTime = coyoteTime;       
        doubleJumpTimer = 0.15f;
        _fallTimer = weeEffectFallThreshold;
        shadowDanTracer = GetComponent<ShadowDanTracer>();
        shadowDanTracer.enabled = false;
        materialPropertyBlock = new MaterialPropertyBlock();

        eyeBrowStartPosition = eyeBrow.localPosition;
        gameInputManager = GameInputManager.Instance;
        
        if(gameManager.lockAiming)
        {
            pupilsParent.localPosition = new Vector3(pupilsParentStartPosition.x + maxPupilsOffset.x, pupilsParentStartPosition.y -maxPupilsOffset.y, 0);
            eyeBrow.localPosition = new Vector2(eyeBrowStartPosition.x, eyeBrowStartPosition.y - maxPupilsOffset.y);
        }

        CheckForUpgrades();
    }

    void CheckForUpgrades()
    {
        //The Red Cross
        if(gameManager.currentUser.equippedUpgrades.Contains(6487))
        {
            health += health / 2;
            _health = health;
        }

        //The Golden Heart
        if(gameManager.currentUser.equippedUpgrades.Contains(5480))
            lives = 4;
    }

    void Update()
    {
        _knockBackInputDelayTimer -= Time.deltaTime;
        _knockBackInputDelayTimer = Mathf.Clamp(_knockBackInputDelayTimer, 0, 5);

        if(_hurtTimer > 0)
            _hurtTimer -= Time.deltaTime;    

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
        
        if(CanHandleInput())
            HandleInput();

        if(CanAnimate())
            SetAnimationsAndWalkEffects();

        if(LevelManager.Instance.FinishedLevel())
            FinishLevel();

        if(teleporting)
        {
            animator.SetFloat("Speed", 0);
            animator.SetBool("Grounded", true);
        }
    }

    #region PhysicsAndMovement
    void FixedUpdate()
    {
        if(rb2D.velocity.y <= -25)
            rb2D.velocity = new Vector2(rb2D.velocity.x, -25);
        
        smoothedX = Mathf.SmoothDamp(rb2D.velocity.x, input.x * _speed, ref velocityXSmoothing, grounded ? accelerationTimeGrounded : accelerationTimeInAir);

        /////MAKE SURE THIS ISNT MESSING ANYTHING UP
        if(float.IsNaN(velocityXSmoothing) || dead || Mathf.Abs(velocityXSmoothing) < 0.0005f)
            velocityXSmoothing = 0;

        if(float.IsNaN(smoothedX) || dead)
            smoothedX = 0;

        if(!teleporting && !dead)
            rb2D.velocity = new Vector2(smoothedX, rb2D.velocity.y);
        else
            rb2D.velocity = Vector2.zero;

        CalculateGroundingAndLanding();
    }

    void Jump()
    {
        if(fallButtonTimer > 0)
            fallButtonTimer -= Time.deltaTime;

        if(grounded && fallButtonTimer <= 0 && gameInputManager.VerticalMoveInput() < -gameInputManager.VerticalInputSensitivity)
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
            
            gameManager.currentUser.totalJumps += 1;
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

            gameManager.currentUser.totalJumps += 1;
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

    public void AddVelocity(Vector2 direction, int force)
    {
        rb2D.velocity = direction * force;
    }

    public void CancelYVelocity()
    {
        rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
    }

    #endregion
    
    #region RotateArm

    void RotateArm()
    {
        if(gameInputManager.ControllerConnected() && crosshairs != null)
        {            
            difference = crosshairs.position - aimPoint.position;
            difference.Normalize();
        }
        else
        {
            //Mouse distance
            if(Vector2.Distance(transform.position, gameInputManager.RealMousePosition()) > 2)
            {
                difference = gameInputManager.RealMousePosition() - aimPoint.position;
                difference.Normalize();
                
                lastAimDirection = difference;
            }
            else
            {
                difference = lastAimDirection;
            }
        }

        armTwo.rotation = Quaternion.Slerp(armTwo.rotation, Quaternion.Euler(0, 0, (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg) + 90), Time.deltaTime * 10);       
    }

    #endregion

    #region Input
    public bool CanHandleInput()
    {
        if(!dead && !gameHUDPaused && !gameHUDFrozen && !LevelManager.Instance.FinishedLevel() && _knockBackInputDelayTimer <= 0 && !teleporting)
            return true;
        else
            return false;
    }

    public bool CanAnimate()
    {
        if(!dead && !gameHUDPaused && !gameHUDFrozen && !LevelManager.Instance.FinishedLevel() && !teleporting)
            return true;
        else
            return false;
    }

    public void HandleInput()
    {
        if(gameInputManager.HorizontalMoveInput() > gameInputManager.HorizontalInputSensitivity)
            xInput = 1;
        else if(gameInputManager.HorizontalMoveInput() < -gameInputManager.HorizontalInputSensitivity)
            xInput = -1;
        else
            xInput = 0;

        if(gameInputManager.VerticalMoveInput() > gameInputManager.VerticalInputSensitivity)
            yInput = 1;
        else if(gameInputManager.VerticalMoveInput() < -gameInputManager.VerticalInputSensitivity)
            yInput = -1;
        else
            yInput = 0;

        input = new Vector2(xInput, yInput);

        if(gameInputManager.SprintButton())
            _speed = grounded ? sprintSpeed : inAirSprintSpeed;
        else
            _speed = walkSpeed;

        if(input.x > 0)
            direction = 1;
        else if(input.x < 0)
            direction = -1;

        transform.localScale = new Vector3(direction, 1, 1);   
        
        if(!gameManager.lockAiming)
            RotateArm();
        else
            armTwo.rotation = Quaternion.Euler(0, 0, direction == 1 ? 90 : -90); 

        RotateEyes();
        Jump();
    }

    IEnumerator ScaleCrosshairs(Vector3 from, Vector3 to)
    {
        float inTime = 0;
        float duration = 0.35f;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            
            float t = inTime / duration;
            crosshairs.localScale = Vector3.Lerp(from, to, t);   

            yield return null;
        }

        crosshairs.localScale = to;
    }

    public void ResetDownTimer()
    {
        fallButtonTimer = fallButtonDelay;
    }

    #endregion

    #region RotateEyes
    void RotateEyes()
    {
        if(!gameManager.lockAiming)
        {
            if(gameInputManager.ControllerConnected())
            {
                eyeLookDirection = crosshairs.position - transform.position;
            }
            else
            {
                Vector3 realMousePosition = gameInputManager.RealMousePosition();

                realMousePosition.z = pupilsParent.position.z;
                eyeLookDirection = realMousePosition - transform.position;
            }
            
            eyeLookDirection = new Vector3(eyeLookDirection.x * transform.localScale.x, eyeLookDirection.y, 0);
            
            float t = Mathf.Clamp01(eyeLookDirection.magnitude / maxMouseDistance);
            float thresholdX = Mathf.Lerp(minThreshold, maxThreshold, t) + maxPupilsOffset.x;
            float thresholdY = Mathf.Lerp(minThreshold, maxThreshold, t) + maxPupilsOffset.y;
            
            float scaledX = eyeLookDirection.x * horizontalSensitivity;
            float scaledY = eyeLookDirection.y * verticalSensitivity;
            
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
        }
        else
        {
            targetX = maxPupilsOffset.x;
            targetY = yInput > 0 ? maxPupilsOffset.y : -maxPupilsOffset.y; 
        }
        
        pupilsParent.localPosition = Vector2.Lerp(pupilsParent.localPosition, pupilsParentStartPosition + new Vector2(targetX, targetY), Time.deltaTime * smoothSpeed);
                
        if(eyeBrow.gameObject.activeInHierarchy)
            eyeBrow.localPosition = Vector2.Lerp(eyeBrow.localPosition, eyeBrowStartPosition + new Vector2(0, targetY), Time.deltaTime * smoothSpeed);    
    }
    #endregion

    //#region RotateBody
    //void RotateBody()
    //{
        //if(gameInputManager.ControllerConnected() && crosshairs != null)
        //{            
            //difference = crosshairs.position - transform.position;
            //difference.Normalize();
        //}
        //else
        //{
            //float mouseDistance = Vector2.Distance(transform.position, gameInputManager.GetRealMousePosition());
            
            //if(mouseDistance > 2)
            //{
                //difference = gameInputManager.GetRealMousePosition() - transform.position;
                //difference.Normalize();
            //}
            //else
            //{
                //difference = Vector3.zero;
            //}
        //}
        
        //float localDirectionX = direction > 0 ? difference.x : -difference.x;
        //float rotZ = Mathf.Atan2(-difference.y, localDirectionX) * Mathf.Rad2Deg;
        //float rotZOffset;

        //if(difference.x != 0 || difference.y != 0)
        //{
            //if(gameInputManager.GetRealMousePosition().x > transform.position.x && direction == -1)
                //rotZOffset = 0;
            //else if(gameInputManager.GetRealMousePosition().x < transform.position.x && direction == 1)
                //rotZOffset = 0;
            //else
                //rotZOffset = transform.lossyScale.x > 0 ? -rotZ : rotZ;
        //}
        //else
        //{
            //rotZOffset = 0;
        //}

        //rotZOffset = Math.Clamp(rotZOffset, bodyRotateMinMax.x, bodyRotateMinMax.y);
        //currentBodyRotation = Mathf.Lerp(currentBodyRotation, rotZOffset, bodyRotateSpeed * Time.deltaTime);

        //body.rotation = Quaternion.Euler(0, 0, currentBodyRotation);
    //}
    //#endregion

    #region AnimationAndWalkEffects
    void SetAnimationsAndWalkEffects()
    {
        velocityXAbs = Mathf.Abs(rb2D.velocity.x);
        isWalkingOnGround = grounded && velocityXAbs > 0.5f;

        ParticleSystem.EmissionModule emission = walkParticles.emission;
        walkParticlesEmmisionRate = isWalkingOnGround ? Mathf.Lerp(10, 30, velocityXAbs / 8) : 0;
        
        emission.rateOverTime = walkParticlesEmmisionRate;

        animator.SetBool("Grounded", grounded);
        
        if(weeSprite.gameObject.activeInHierarchy)
            weeSprite.sprite = weeSprites[transform.localScale.x > 0 ? 1 : 0];
            
        if(!grounded)
        {
            if(_fallTimer > 0)
                _fallTimer -= Time.deltaTime;
            else
            {
                if(canAnimateWee && !animatedWee)
                {
                    AudioManager.Instance.PlaySound2D(weeSound);
                    StartCoroutine(AnimateWeeEffect(1));
                    animatedWee = true;
                }
            }
        }
        else
        {
            _fallTimer = weeEffectFallThreshold;
            
            if(canAnimateWee)
            {
                StartCoroutine(AnimateWeeEffect(-1));
                animatedWee = false;
            }
        }
        
        if((velocityXAbs - 0.1f) > walkSpeed && velocityXAbs < sprintSpeed)
        {
            walkAnimationT = Mathf.InverseLerp(walkSpeed, sprintSpeed, velocityXAbs);
            walkAnimationSpeed = Mathf.Lerp(1, 1.45f, walkAnimationT);
            
            if(grounded)
                bodyTargetRotation = gameInputManager.SprintButton() ? bodyRotateMinMax.x : 0;
            else
                bodyTargetRotation = 0;
        }
        else if((velocityXAbs - 0.1f) < walkSpeed || velocityXAbs == walkSpeed)
        {
            walkAnimationSpeed = 1;

            if(grounded)
            {
                if(input.y > 0.1f)
                    bodyTargetRotation = bodyRotateMinMax.y;
                else if(input.y < -0.1f)
                    bodyTargetRotation = bodyRotateMinMax.x;        
                else
                    bodyTargetRotation = 0;
            }
            else
            {
                bodyTargetRotation = 0;
            }
        }
        else
        {
            walkAnimationSpeed = 1.45f;
            bodyTargetRotation = grounded ? bodyRotateMinMax.x : 0;
        }

        animator.SetFloat("Walk Speed", walkAnimationSpeed);

        currentBodyRotation = Mathf.Lerp(currentBodyRotation, direction == 1 ? bodyTargetRotation : -bodyTargetRotation, bodyRotateSpeed * Time.deltaTime);
        body.rotation = Quaternion.Euler(0, 0, currentBodyRotation);     

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
    
    #endregion

    #region WeeEffect
    IEnumerator AnimateWeeEffect(int direction)
    {
        canAnimateWee = false;

        float inTime = 0;
        float duration = 0.25f;

        Vector3 from = direction > 0 ? Vector3.zero : Vector3.one;
        Vector3 to = direction > 0 ? Vector3.one : Vector3.zero;

        if(direction > 0)
            weeSprite.gameObject.SetActive(true);

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            float t = inTime / duration;
            float smoothT = 1 - Mathf.Pow(1 - t, 4);

            weeSprite.transform.localScale = Vector3.Lerp(from, to, smoothT);

            yield return null;
        }

        weeSprite.transform.localScale = to;

        if(direction < 0)
            weeSprite.gameObject.SetActive(false);

        canAnimateWee = true;
    }
    
    #endregion

    #region LandParticles
    void TriggerLandParticles(float impactSpeed)
    {
        float t = Mathf.Clamp01((impactSpeed - 1f) / 24f);

        //Burst amount
        landParticles.Emit(Mathf.RoundToInt(Mathf.Lerp(1, 15, t)));

        ParticleSystem.ShapeModule shape = landParticles.shape;
        shape.radius = Mathf.Lerp(0.5f, 1, t);
    }

    #endregion

    #region DamageFlashing

    IEnumerator Flash()
    {
        for(int i = 0; i < 4; i++)
        {
            if(dead)
            {
                gunSpriteRenderer.material.SetFloat("_FlashAmount", 0);
                ApplySpriteMaterialProperties(0);
                break;
            }
            
            ApplySpriteMaterialProperties(1);
            gunSpriteRenderer.material.SetFloat("_FlashAmount", 1);
            
            yield return flashSpeed;

            if(!dead)
                ApplySpriteMaterialProperties(0);

            gunSpriteRenderer.material.SetFloat("_FlashAmount", 0);

            yield return flashSpeed;
        }
    }

    void ApplySpriteMaterialProperties(float flashAmount)
    {
        foreach(SpriteRenderer spriteMaterial in spriteMaterials)
        {
            spriteMaterial.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_FlashAmount", flashAmount);
            spriteMaterial.SetPropertyBlock(materialPropertyBlock);
        }
    }

    void ApplySpriteMaterialProperties(float flashAmount, float alpha)
    {
        foreach(SpriteRenderer spriteMaterial in spriteMaterials)
        {
            spriteMaterial.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_FlashAmount", flashAmount);
            materialPropertyBlock.SetColor("_Color", new Color(spriteMaterial.material.color.r, spriteMaterial.material.color.g, spriteMaterial.material.color.b, alpha));
            spriteMaterial.SetPropertyBlock(materialPropertyBlock);
        }
    }

    void ApplySpriteMaterialProperties(float flashAmount, float alpha, float applyStencilMasking)
    {
        foreach(SpriteRenderer spriteMaterial in spriteMaterials)
        {
            spriteMaterial.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_FlashAmount", flashAmount);
            materialPropertyBlock.SetColor("_Color", new Color(spriteMaterial.material.color.r, spriteMaterial.material.color.g, spriteMaterial.material.color.b, alpha));
            materialPropertyBlock.SetFloat("_EnableStencilMasking", applyStencilMasking);
            spriteMaterial.SetPropertyBlock(materialPropertyBlock);
        }
    }

    #endregion

    #region Health
    public void GiveHealth(int healthToGive)
    {
        _health = _health + healthToGive;

        if(_health >= health)
            _health = health;

        OnPlayerHealthChange?.Invoke();
    }

    #endregion

    #region DamageAndDeath
    public void TakeDamage(int damage, float inputDelay, bool giveKnockBack, int xKnockBack, int yKnockBack, float xOffset, Transform otherTransform, bool rotationBasedKnockBack, int knockBack, float rotationOffset)
    {
        if(invincible || _hurtTimer > 0)
            return;
        
        if(giveKnockBack)
        {
            _knockBackInputDelayTimer = inputDelay;
            rb2D.velocity = Vector2.zero;

            if(rotationBasedKnockBack)
            {
                Quaternion _rotationOffset = Quaternion.Euler(0, 0, rotationOffset);

                Vector2 knockBackDirection = _rotationOffset * otherTransform.up;
                rb2D.AddForce(knockBackDirection * knockBack, ForceMode2D.Impulse);

                doubleJump = true;
            }
            else
            {
                float xDistance = Mathf.Abs(otherTransform.position.x - transform.position.x);

                if(xDistance <= xOffset)
                {
                    rb2D.AddForce(new Vector2(0, yKnockBack * 1.5f), ForceMode2D.Impulse);
                    doubleJump = true;
                }
                else
                {
                    doubleJump = true;

                    if(otherTransform.position.x > transform.position.x)
                        rb2D.AddForce(new Vector2(-xKnockBack, yKnockBack), ForceMode2D.Impulse);
                    else if(otherTransform.position.x < transform.position.x)
                        rb2D.AddForce(new Vector2(xKnockBack, yKnockBack), ForceMode2D.Impulse);
                }
            }
        }

        //FloatingNumberProperties numberProperties = new FloatingNumberProperties();
        //numberProperties.number = damage;
        //numberProperties.position = transform.position;
        //numberProperties.plusOrMinus = false;
        //numberProperties.color = Color.red;
        //numberProperties.instantKill = damage == health ? true : false;
        
        //PoolManager.Instance.ReuseObject("Floating Numbers", numberProperties);

        Instantiate(hurtEffect, transform.position, Quaternion.identity);

        _health -= damage;

        OnPlayerHurt?.Invoke();

        if(properties.strength > 0)
            CameraManager.Instance.Shake(properties);

        AudioManager.Instance.PlaySound2D(hurtSound);
        StartCoroutine(Flash());

        if(_health > 0)
            gameInputManager.RumbleController(lowHurtRumble, highHurtRumble, hurtRumbleDuration);

        if(_health <= 0 && !dead)
            Kill();

        _hurtTimer = hurtTimer;
    }

    public void Kill()
    {
        dead = true;        
        _collider2D.enabled = false;
        rb2D.bodyType = RigidbodyType2D.Kinematic;
        canFollow = false;
        animator.enabled = false;
        _health = 0;
        AudioManager.Instance.PlaySound2D(destroyedSound);
        Instantiate(destroyedEffect, transform.position, Quaternion.identity);
        lives -= 1;
        weeSprite.gameObject.SetActive(false);
        walkParticles.Stop();
        
        if(gameInputManager.ControllerConnected() && crosshairs != null)
            StartCoroutine(ScaleCrosshairs(Vector3.one, Vector3.zero));

        OnPlayerKilled?.Invoke();

        gameManager.currentUser.totalDeaths += 1;
        gameInputManager.RumbleController(lowDeathRumble, highDeathRumble, deathRumbleDuration);

        StartCoroutine(FlingGun());
        StartCoroutine(KillCo());
    }

    IEnumerator KillCo()
    {
        ApplySpriteMaterialProperties(1, 1, 1);
        
        float inTime = 0;
        float duration = 1.25f;
        float alpha = 1;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            
            float t = inTime / duration;
            float smoothT = Mathf.Pow(inTime / duration, 3);

            Vector3 from = new Vector3(direction, 1, 1);
            Vector3 to = new Vector3(3 * direction, 3, 1);

            transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, -125) * direction, smoothT);
            transform.localScale = Vector2.Lerp(from, to, smoothT);
            alpha = Mathf.Lerp(1, 0, smoothT);
            
            ApplySpriteMaterialProperties(1, alpha);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1);
        
        if(lives > 0)
            StartCoroutine(RespawnCo());
    }

    #endregion
    
    #region Respawning
    IEnumerator RespawnCo()
    {
        LevelManager.Instance.Respawn();
        OnPlayerRespawn?.Invoke();

        ApplySpriteMaterialProperties(1, 0, 0);

        animator.enabled = true;
        pupilsParent.localPosition = new Vector2(pupilsParentStartPosition.x + maxPupilsOffset.x, pupilsParentStartPosition.y -maxPupilsOffset.y);
        eyeBrow.localPosition = new Vector2(eyeBrowStartPosition.x, eyeBrowStartPosition.y - maxPupilsOffset.y);
        
        rb2D.position = transform.position;

        bodyTargetRotation = 0;
        currentBodyRotation = 0;
        body.localEulerAngles = Vector3.zero;
        
        gunTransform.parent = armTwo;
        gunTransform.localScale = Vector3.one;        

        float inTime = 0;
        float duration = 0.75f;

        yield return new WaitForSecondsRealtime(0.75f);

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;

            armTwo.localRotation = Quaternion.Euler(0, 0, 90);
            gunTransform.localPosition = gunStartPosition;
            gunTransform.localEulerAngles = new Vector3(0, 0, -90);

            animator.SetBool("Grounded", true);
            animator.SetFloat("Speed", 0);

            float t = inTime / duration;
            float smoothT = 1 - Mathf.Pow(1 - t, 4);

            ApplySpriteMaterialProperties(Mathf.Lerp(1, 0, smoothT), Mathf.Lerp(0, 1, smoothT));
            
            transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -125), Vector3.zero, smoothT);
            transform.localScale = Vector2.Lerp(Vector2.one * 3, Vector2.one, smoothT);

            yield return null;
        }

        transform.localScale = new Vector2(1, 1);
        transform.localEulerAngles = Vector3.zero;
        
        ApplySpriteMaterialProperties(0, 1);
        
        if(gameInputManager.ControllerConnected() && crosshairs != null)
            StartCoroutine(ScaleCrosshairs(Vector3.zero, Vector3.one));

        direction = 1;
        _knockBackInputDelayTimer = 0;
        canFollow = true;
        walkParticles.Play();

        _health = health;

        yield return new WaitForFixedUpdate();
        _collider2D.enabled = true;
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        dead = false;
    }
    
    #endregion    

    #region Teleporting
    public void Teleporting()
    {
        teleporting = true;
        invincible = true;

        bodyTargetRotation = 0;
        currentBodyRotation = 0;
        body.localEulerAngles = Vector3.zero;
    }

    public void Teleported(Vector3 transportPoint)
    {
        transform.position = new Vector2(transportPoint.x, transportPoint.y - 0.125f);

        OnPlayerTeleported?.Invoke();

        invincible = false;
        teleporting = false;
    }

    #endregion

    #region FinishLevel
    void FinishLevel()
    {
        invincible = true;
        canFollow = false;
        StartCoroutine(ScaleCrosshairs(Vector3.one, Vector3.zero));

        ParticleSystem.EmissionModule emission = walkParticles.emission;
        emission.rateOverTime = 0;

        animator.SetBool("Grounded", grounded);
        animator.SetFloat("Speed", 0);
        
        float duration = 0.75f;

        pupilsParent.localPosition = Vector2.Lerp(pupilsParent.localPosition, new Vector2(pupilsParentStartPosition.x + maxPupilsOffset.x, pupilsParentStartPosition.y -maxPupilsOffset.y), Time.deltaTime * smoothSpeed);

        if(eyeBrow.gameObject.activeInHierarchy)
            eyeBrow.localPosition = Vector2.Lerp(eyeBrow.localPosition, new Vector2(eyeBrowStartPosition.x, eyeBrowStartPosition.y - maxPupilsOffset.y), Time.deltaTime * smoothSpeed);    

        currentBodyRotation = Mathf.Lerp(currentBodyRotation, 0, bodyRotateSpeed * Time.deltaTime);
        body.rotation = Quaternion.Euler(0, 0, currentBodyRotation);
        
        if(!flingedGun)
        {
            StartCoroutine(FlingGun());
            flingedGun = true;
        }

        if(canAnimateWee)
        {
            StartCoroutine(AnimateWeeEffect(-1));
            animatedWee = false;
        }
        
        if(finishLevelInTime < duration)
        {
            finishLevelInTime += Time.unscaledDeltaTime;
            float smoothT = Mathf.SmoothStep(0, 1, finishLevelInTime / duration);

            rb2D.velocity = new Vector2(Mathf.Lerp(rb2D.velocity.x, 0, smoothT), rb2D.velocity.y);
            finalPositionX = transform.position.x;
        }
        else
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            transform.position = new Vector2(finalPositionX, transform.position.y);
        }

        if(grounded)
        {
            finishLevelJumpTimer -= Time.deltaTime;

            if(finishLevelJumpTimer <= 0)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, jumpHeight);

                if(firstFinishJump)
                    transform.localScale = new Vector2(-transform.localScale.x, 1);

                finishLevelJumpTimer = 0.25f;
                firstFinishJump = true;
            }
        }

        armTwo.localEulerAngles = new Vector3(0, 0, grounded ? 90 : 180);
    }

    #endregion

    IEnumerator FlingGun()
    {
        gunTransform.parent = null;
        flingRight = !flingRight;

        int direction = flingRight ? 1 : -1;
        Vector3 startPosition = gunTransform.position;
        //float flingHeight = UnityEngine.Random.Range(10, 18);
        float flingHeight = UnityEngine.Random.Range(9, 12);
        Vector3 flingEnd = startPosition + new Vector3(direction * UnityEngine.Random.Range(10, 18), -10, 0);
        
        float flingDuration = 1.5f;
        float elapsed = 0;

        float previousY = 0, previousY2 = 0;
        float previousX = 0, previousX2 = 0;
        float previousDeltaTime = 0, previousDeltaTime2 = 0;

        while(elapsed < flingDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flingDuration);

            float easedT = t * t;
            float arcY = (1 - Mathf.Pow(t * 2 - 1, 2)) * flingHeight;

            Vector3 position = Vector3.Lerp(startPosition, flingEnd, easedT);
            position.y = startPosition.y + arcY + Mathf.Lerp(0, flingEnd.y - startPosition.y, easedT);

            previousY2 = previousY;
            previousX2 = previousX;
            previousDeltaTime2 = previousDeltaTime;

            previousY = gunTransform.position.y;
            previousX = gunTransform.position.x;
            previousDeltaTime = Time.unscaledDeltaTime;

            gunTransform.position = position;
            gunTransform.Rotate(0, 0, -direction * 500 * Time.unscaledDeltaTime);

            yield return null;
        }

        if(!dead)
        {
            float velocityY1 = Time.unscaledDeltaTime > 0 ? (gunTransform.position.y - previousY) / Time.unscaledDeltaTime : 0;
            float velocityY2 = previousDeltaTime2 > 0 ? (previousY - previousY2) / previousDeltaTime2 : 0;
            
            float initialVelocityY = (velocityY1 + velocityY2) / 2;

            float velocityX1 = Time.unscaledDeltaTime > 0 ? (gunTransform.position.x - previousX) / Time.unscaledDeltaTime : 0;
            float velocityX2 = previousDeltaTime2 > 0 ? (previousX - previousX2) / previousDeltaTime2 : 0;
            float initialVelocityX = (velocityX1 + velocityX2) / 2;

            float gravity = -18;
            float fallElapsed = 0;
            float fallDuration = 4f;

            Vector3 landedPosition = gunTransform.position;

            while(fallElapsed < fallDuration)
            {
                fallElapsed += Time.unscaledDeltaTime;
                float yOffset = initialVelocityY * fallElapsed + 0.5f * gravity * fallElapsed * fallElapsed;
                float xOffset = initialVelocityX * fallElapsed;
                
                gunTransform.position = new Vector3(landedPosition.x + xOffset, landedPosition.y + yOffset, landedPosition.z);
                gunTransform.Rotate(0, 0, -direction * 500 * Time.unscaledDeltaTime);

                if(gunTransform.position.y < _camera.transform.position.y - _camera.orthographicSize * 2)
                    fallDuration = 0;

                yield return null;
            }   
        }
    }

    public void EnableShadowDanTracer()
    {
        if(!shadowDanTracer.enabled)
            shadowDanTracer.enabled = true;
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(groundCheck.position + groundCheckOffset, groundCheckSize);
    }
    #endif
}