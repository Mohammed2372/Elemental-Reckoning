using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airControlMultiplier = 0.8f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float fastFallSpeed = 20f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashTime = 0.5f;
    private float lastDashTime = -100f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);

    [Header("Respawn Settings")]
    public Transform levelStartPoint; // Assign in Inspector
    public float deathRespawnDelay = 2f; // Time to wait before respawn (optional)

    [Header("Attack Settings")]
    [SerializeField] private int currentAttackIndex = 0;
    [SerializeField] private float firstAttackCooldown = 1f;
    [SerializeField] private float secondAttackCooldown = 5f;
    [SerializeField] private float thirdAttackCooldown = 10f;
    private float[] lastAttackTimes = new float[3]; // Track last time each attack was used
    public int CurrentAttackIndex => currentAttackIndex;

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
    public int Health => currentHealth;
    public bool IsFacingRight => isFacingRight;
    public float FastFallSpeed => fastFallSpeed;
    public float FallMultiplier => fallMultiplier;

    // Input buffering
    private float lastJumpInputTime;
    private float lastGroundedTime;
    private bool jumpInputConsumed = false;
    private attack_point attackPoint; // Assuming you have an AttackPoint script or similar

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        InitializeStateMachine();
        currentHealth = maxHealth;

        // Initialize attack timers
        for (int i = 0; i < lastAttackTimes.Length; i++)
        {
            lastAttackTimes[i] = -100f; // Set to a large negative value so attacks are available at start
        }
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
        HandleAttackInput();
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();

        // Test: Press P to damage the player by 1
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(10);
        }
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpInputTime = Time.time;
            jumpInputConsumed = false;
        }
    }

    private void HandleAttackInput()
    {
        // Handle air attacks first - only J key works in air
        if (!IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTimes[0] + firstAttackCooldown)
            {
                lastAttackTimes[0] = Time.time;
                stateMachine.ChangeState(airAttackState);
            }
            return; // Exit early to prevent ground attacks while in air
        }

        // Handle ground attacks only when grounded and not in an attack state
        if (!(stateMachine.CurrentState is AttackState))
        {
            if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTimes[0] + firstAttackCooldown)
            {
                lastAttackTimes[0] = Time.time;
                currentAttackIndex = 0;
                stateMachine.ChangeState(attackState);
            }
            else if (Input.GetKeyDown(KeyCode.K) && Time.time >= lastAttackTimes[1] + secondAttackCooldown)
            {
                lastAttackTimes[1] = Time.time;
                currentAttackIndex = 1;
                stateMachine.ChangeState(attackState);
            }
            else if (Input.GetKeyDown(KeyCode.L) && Time.time >= lastAttackTimes[2] + thirdAttackCooldown)
            {
                lastAttackTimes[2] = Time.time;
                currentAttackIndex = 2;
                stateMachine.ChangeState(attackState);
            }
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

    public void UpdateFacingDirection(float moveInput)
    {
        if (Mathf.Abs(moveInput) < 0.1f) return;

        bool shouldFaceRight = moveInput > 0;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            // Simply flip the sprite based on direction
            spriteRenderer.flipX = !shouldFaceRight;

            // Update attack point position if it exists
            if (attackPoint != null)
            {
                Vector3 localPos = attackPoint.transform.localPosition;
                localPos.x = Mathf.Abs(localPos.x) * (isFacingRight ? 1 : -1);
                attackPoint.transform.localPosition = localPos;
                attackPoint.FlipColliders(isFacingRight);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (stateMachine.CurrentState == deathState) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        // Update health bar UI
        Player_Health playerHealth = GetComponent<Player_Health>();
        if (playerHealth != null)
        {
            playerHealth.UpdateHealthBar(currentHealth, maxHealth);
        }

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
        // Update health bar UI
        Player_Health playerHealth = GetComponent<Player_Health>();
        if (playerHealth != null)
        {
            playerHealth.Heal(0); // 0 because we've already added health
        }
    }

    public void ResetDashCooldown()
    {
        lastDashTime = Time.time;
    }


    public void RespawnAtLevelStart()
    {
        if (levelStartPoint != null)
        {
            transform.position = levelStartPoint.position;
            currentHealth = maxHealth;
            Player_Health playerHealth = GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                playerHealth.UpdateHealthBar(currentHealth, maxHealth);
            }
            stateMachine.ChangeState(idleState);
        }
    }

    #endregion

    #region Animation Events

    [SerializeField]
    public void AnimationTrigger_EndTakeHit()
    {
        Debug.Log("Animation Event: End Take Hit"); // Debug log to verify it's being called
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