using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // player variables
    [Header("Player Settings")]
    public Rigidbody2D rb;
    public Animator Animator;
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    [HideInInspector] public bool HasSecondAttackSkill = false; // Set to true when the player acquires the second attack skill
    [HideInInspector] public bool HasThirdAttackSkill = false;  // Set to true when the player acquires the third attack skill


    [Header("Ground Settings")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    // state machine variables
    [HideInInspector] public StateMachine StateMachine;
    [HideInInspector] public IdleState IdleState;
    [HideInInspector] public MoveState MoveState;
    [HideInInspector] public JumpState JumpState;
    [HideInInspector] public DashState DashState;
    [HideInInspector] public AttackState AttackState;
    [HideInInspector] public AirAttackState AirAttackState;
    //[HideInInspector] public LightAttackState LightAttackState;
    //[HideInInspector] public SpecialAttackState SpecialAttackState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();

        StateMachine = new StateMachine();

        IdleState = new IdleState(this, StateMachine);
        MoveState = new MoveState(this, StateMachine);
        JumpState = new JumpState(this, StateMachine);
        DashState = new DashState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        AirAttackState = new AirAttackState(this, StateMachine); // Add this line
        //LightAttackState = new LightAttackState(this, StateMachine);
        //SpecialAttackState = new SpecialAttackState(this, StateMachine);
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }
    private void Update()
    {
        StateMachine.CurrentState.HandleInput();
    }
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    public void SetVelocityX(float x)
    {
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }
    public void SetVelocityY(float y)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, y);
    }
    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
    }
    public void AnimationTrigger_CanQueueNext()
    {
        if (StateMachine.CurrentState is AttackState attackState)
            attackState.AnimationTrigger_CanQueueNext();
    }
    public void AnimationTrigger_EndAttack()
    {
        if (StateMachine.CurrentState is AttackState attackState)
            attackState.AnimationTrigger_EndAttack();
    }

}
