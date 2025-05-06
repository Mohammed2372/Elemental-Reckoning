using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airControlMultiplier = 0.8f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private float lastDashTime = -100f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool hasSecondAttackSkill = false;
    [SerializeField] private bool hasThirdAttackSkill = false;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);


    // Components
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator animator;
    private SpriteRenderer spriteRenderer;

    // State Machine
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public IdleState idleState;
    [HideInInspector] public MoveState moveState;
    [HideInInspector] public JumpState jumpState;
    [HideInInspector] public DashState dashState;
    [HideInInspector] public AttackState attackState;
    [HideInInspector] public AirAttackState airAttackState;
    [HideInInspector] public FallState fallState;
    [HideInInspector] public TakeHitState takeHitState;
    [HideInInspector] public DeathState deathState;

    // variables
    private bool isFacingRight = true;

    // Properties
    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float DashSpeed => dashSpeed;
    public float DashTime => dashTime;
    public bool CanDash => Time.time > lastDashTime + dashCooldown;
    public bool HasSecondAttackSkill => hasSecondAttackSkill;
    public bool HasThirdAttackSkill => hasThirdAttackSkill;
    public int Health => currentHealth;
    public bool IsFacingRight => isFacingRight;

    // Input buffering
    private float lastJumpInputTime;
    private float lastGroundedTime;
    private bool jumpInputConsumed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        InitializeStateMachine();
        currentHealth = maxHealth;
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine(this);

        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        jumpState = new JumpState(this, stateMachine);
        dashState = new DashState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
        airAttackState = new AirAttackState(this, stateMachine);
        fallState = new FallState(this, stateMachine);
        takeHitState = new TakeHitState(this, stateMachine);
        deathState = new DeathState(this, stateMachine);

        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        UpdateTimers();
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }

    private void UpdateTimers()
    {
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }
    }


    #region Public Methods

    public void SetVelocityX(float x)
    {
        rb.velocity = new Vector2(x, rb.velocity.y);
    }

    public void SetVelocityY(float y)
    {
        rb.velocity = new Vector2(rb.velocity.x, y);
    }

    public void SetVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    public bool IsGrounded()
    {
        // Use BoxCast for more reliable ground detection
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            new Vector2(0.5f, 0.1f), // Adjust size to match character's feet
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Debug visualization
        Debug.DrawRay(transform.position, Vector2.down * (groundCheckDistance + 0.05f),
                     hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }
    public bool CanJump()
    {
        // Coyote time implementation
        return Time.time < lastGroundedTime + coyoteTime;
    }

    public bool JumpInputBuffered()
    {
        // Jump buffer implementation
        return Time.time < lastJumpInputTime + jumpBufferTime;
    }

    public void RecordJumpInput()
    {
        lastJumpInputTime = Time.time;
    }

    public void FlipSprite(bool faceRight)
    {
        spriteRenderer.flipX = !faceRight;
    }
    public void UpdateFacingDirection(float moveInput)
    {
        // Only flip if there's movement input
        if (moveInput == 0) return;

        bool shouldFaceRight = moveInput > 0;

        // Only flip if direction changes
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            spriteRenderer.flipX = !isFacingRight;
        }
    }
    public void TakeDamage(int damage)
    {
        if (stateMachine.CurrentState == deathState) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth <= 0)
        {
            stateMachine.ChangeState(deathState);
        }
        else
        {
            stateMachine.ChangeState(takeHitState);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void ResetDashCooldown()
    {
        lastDashTime = Time.time;
    }

    public void UnlockSecondAttack()
    {
        hasSecondAttackSkill = true;
    }

    public void UnlockThirdAttack()
    {
        hasThirdAttackSkill = true;
    }

    #endregion

    #region Animation Events

    public void AnimationTrigger_CanQueueNext()
    {
        if (stateMachine.CurrentState is AttackState attackState)
        {
            attackState.AnimationTrigger_CanQueueNext();
        }
    }

    public void AnimationTrigger_EndAttack()
    {
        if (stateMachine.CurrentState is AttackState attackState)
        {
            attackState.AnimationTrigger_EndAttack();
        }
        else if (stateMachine.CurrentState is AirAttackState airAttackState)
        {
            airAttackState.AnimationTrigger_EndAttack();
        }
    }

    public void AnimationTrigger_EndTakeHit()
    {
        if (stateMachine.CurrentState is TakeHitState takeHitState)
        {
            takeHitState.AnimationTrigger_EndTakeHit();
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw ground check area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckDistance,
                           new Vector3(groundCheckSize.x, groundCheckSize.y, 0));
    }

    #endregion
}