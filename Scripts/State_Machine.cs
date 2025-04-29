using UnityEngine;
using System.Collections.Generic;
using System;
public class StateMachine
{
    public PlayerState CurrentState { get; private set; }
    private PlayerController player;
    public StateMachine(PlayerController player)
    {
        this.player = player;
    }
    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }
    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        CurrentState.Exit();
        CurrentState = newState;
        newState.Enter();

        // Debugging
        Debug.Log($"State changed to: {newState.GetType().Name}");
    }
}
public class IdleState : PlayerState
{
    private float inputBufferTime = 0.1f;
    private float inputBufferTimer;

    public IdleState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocityX(0);
        player.animator.Play("idle");
    }

    public override void HandleInput()
    {
        // Handle buffered jump input first
        if (inputBufferTimer > 0)
        {
            inputBufferTimer -= Time.deltaTime;
            if (player.IsGrounded())
            {
                stateMachine.ChangeState(player.jumpState);
                return;
            }
        }

        // Check for new jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (player.IsGrounded())
            {
                stateMachine.ChangeState(player.jumpState);
            }
            else
            {
                // Buffer the jump input if not grounded
                inputBufferTimer = inputBufferTime;
            }
            return;
        }

        // Handle other inputs
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.attackState);
        }
        else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
        {
            stateMachine.ChangeState(player.moveState);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && player.CanDash)
        {
            stateMachine.ChangeState(player.dashState);
        }
    }
}
public class MoveState : PlayerState
{
    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.animator.Play("run");
    }

    public override void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        player.UpdateFacingDirection(inputX);

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (player.IsGrounded())
            {
                stateMachine.ChangeState(player.jumpState);
                return;
            }
        }

        // Movement handling
        if (Mathf.Abs(inputX) > 0.1f)
        {
            // Flip player while preserving scale
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            player.transform.localScale = scale;

            player.SetVelocityX(inputX * player.MoveSpeed);
        }
        else
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        //// Other input handling
        // attack
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.attackState);
        }
        // dash
        else if (Input.GetKeyDown(KeyCode.LeftShift) && player.CanDash)
        {
            stateMachine.ChangeState(player.dashState);
        }
    }

    public override void PhysicsUpdate()
    {
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}
public class JumpState : PlayerState
{
    public JumpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // Apply jump force once
        player.SetVelocityY(player.JumpForce);
        player.animator.Play("j_up");
    }

    public override void HandleInput()
    {
        // Handle horizontal movement while jumping
        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.MoveSpeed);
        player.UpdateFacingDirection(inputX);

        // Flip sprite direction without changing scale magnitude
        if (inputX != 0)
        {
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            player.transform.localScale = scale;
        }

        // Check for air attack
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.airAttackState);
        }
    }

    public override void PhysicsUpdate()
    {
        // Transition to fall state when starting to descend
        if (player.rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}

public class FallState : PlayerState
{
    public FallState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.animator.Play("j_down");
    }

    public override void HandleInput()
    {
        // Handle horizontal movement while falling
        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.MoveSpeed);

        // Flip sprite direction without changing scale magnitude
        if (inputX != 0)
        {
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            player.transform.localScale = scale;
        }

        // Check for air attack
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.airAttackState);
        }
    }

    public override void PhysicsUpdate()
    {
        // Return to idle when landing
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}

public class AirAttackState : PlayerState
{
    public AirAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.animator.Play("air_atk");
        player.SetVelocityX(0); // Stop horizontal movement during air attack
    }

    public override void PhysicsUpdate()
    {
        // Continue falling during air attack
        if (player.rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }

    public void AnimationTrigger_EndAttack()
    {
        // Return to falling when air attack completes
        stateMachine.ChangeState(player.fallState);
    }
}
public class DashState : PlayerState
{
    private float dashTimeLeft;

    public DashState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        dashTimeLeft = player.DashTime;
        player.animator.Play("roll"); // Play dash animation
        player.SetVelocityX(player.transform.localScale.x * player.DashSpeed); // Set dash velocity
    }

    public override void HandleInput()
    {
        // Handle jump input during dash
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState(player.jumpState);
        }

        // Handle attack input during dash
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.attackState);
        }
    }

    public override void LogicUpdate()
    {
        dashTimeLeft -= Time.deltaTime;

        // Transition to idleState if dash ends and player is grounded
        if (dashTimeLeft <= 0 && player.IsGrounded())
        {
            stateMachine.ChangeState(player.idleState);
        }

        // Transition to FallState if player is not grounded and falling
        if (!player.IsGrounded() && player.rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }

    public override void PhysicsUpdate()
    {
        // Maintain dash velocity
        player.SetVelocityX(player.transform.localScale.x * player.DashSpeed);
    }

    public override void Exit()
    {
        // Reset velocity when exiting dash
        player.SetVelocityX(0);
    }
}
public class AttackState : PlayerState
{
    private int currentAttackIndex = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 0.8f;
    private bool canQueueNextAttack = false;
    private bool attackInProgress = false;
    private string[] attackAnimations = { "1_atk", "2_atk", "3_atk" };
    private attack_point attackPoint;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
        attackPoint = player.GetComponentInChildren<attack_point>(true);
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint component not found in player children!");
        }
    }

    public override void Enter()
    {
        if (attackPoint == null)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        base.Enter();
        comboTimer = 0f;
        canQueueNextAttack = false;
        attackInProgress = true;
        currentAttackIndex = 0;
        PlayCurrentAttack();
    }

    public override void HandleInput()
    {
        if (!attackInProgress) return;

        if (canQueueNextAttack && (Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)))
        {
            if (currentAttackIndex < attackAnimations.Length - 1)
            {
                if ((currentAttackIndex == 0 && player.HasSecondAttackSkill) ||
                    (currentAttackIndex == 1 && player.HasThirdAttackSkill))
                {
                    currentAttackIndex++;
                    canQueueNextAttack = false;
                    PlayCurrentAttack();
                    return; // Important to prevent multiple attacks in one frame
                }
            }
        }
    }

    public override void LogicUpdate()
    {
        if (!attackInProgress) return;

        base.LogicUpdate();
        comboTimer += Time.deltaTime;

        // Only check animation finish if we're still in attack state
        if (attackInProgress && AnimationFinished(attackAnimations[currentAttackIndex]))
        {
            if (!canQueueNextAttack || comboTimer > comboResetTime)
            {
                ResetCombo();
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    private void PlayCurrentAttack()
    {
        if (attackPoint == null)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        comboTimer = 0f;
        player.animator.Play(attackAnimations[currentAttackIndex]);
        player.SetVelocityX(0);
        attackPoint.StartAttack(currentAttackIndex);
    }
    public void AnimationTrigger_CanQueueNext()
    {
        if (!attackInProgress) return;
        canQueueNextAttack = true;
    }
    public void AnimationTrigger_EndAttack()
    {
        if (!attackInProgress) return;

        attackInProgress = false;
        attackPoint?.EndAttack();

        if (!canQueueNextAttack || currentAttackIndex >= attackAnimations.Length - 1 || comboTimer > comboResetTime)
        {
            ResetCombo();
            stateMachine.ChangeState(player.idleState);
        }
    }
    private void ResetCombo()
    {
        currentAttackIndex = 0;
        comboTimer = 0f;
        canQueueNextAttack = false;
        attackInProgress = false;
        attackPoint?.EndAttack();
    }
    public override void Exit()
    {
        base.Exit();
        attackPoint?.EndAttack();
    }
}
public class TakeHitState : PlayerState
{
    private bool animationCompleted = false;

    public TakeHitState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        animationCompleted = false;
        player.animator.Play("take_hit"); // Play "take hit" animation
    }

    public override void HandleInput()
    {
        // No input handling during "take hit"
    }

    public override void LogicUpdate()
    {
        // Transition to DeathState if health is zero
        if (player.Health <= 0)
        {
            stateMachine.ChangeState(player.deathState);
        }

        // Transition to idleState or FallState after animation completes
        if (animationCompleted)
        {
            if (player.IsGrounded())
            {
                stateMachine.ChangeState(player.idleState);
            }
            else
            {
                stateMachine.ChangeState(player.fallState);
            }
        }
    }

    public void AnimationTrigger_EndTakeHit()
    {
        // Called when the "take hit" animation ends
        animationCompleted = true;
    }
}
public class DeathState : PlayerState
{
    public DeathState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.animator.Play("death"); // Play "death" animation
        player.SetVelocityX(0); // Stop movement
        player.SetVelocityY(0); // Stop vertical movement
    }

    public override void HandleInput()
    {
        // Disable all input during death
    }

    public override void LogicUpdate()
    {
        // No transitions from DeathState
    }
}
