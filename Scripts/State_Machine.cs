using UnityEngine;
using System.Collections.Generic;
using System;
public class StateMachine
{
    public PlayerState CurrentState { get; private set; }
    private Player player;
    public StateMachine(Player player)
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
    }
}

public class IdleState : PlayerState
{
    private float inputBufferTime = 0.1f;
    private float inputBufferTimer;

    public IdleState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

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
    public MoveState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

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
            // Remove the scale-based flipping
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
    public JumpState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // Apply jump force once
        player.SetVelocityY(player.JumpForce);
        player.animator.Play("j_up");
    }

    public override void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.MoveSpeed);
        player.UpdateFacingDirection(inputX);

        // Remove the scale-based flipping

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
    private bool isFastFalling = false;

    public FallState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        isFastFalling = false;
        player.animator.Play("j_down");
    }

    public override void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.MoveSpeed * 0.5f);
        player.UpdateFacingDirection(inputX);

        // Check for air attack
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.airAttackState);
            return;
        }

        // Update fast fall state
        isFastFalling = Input.GetKey(KeyCode.S);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // Apply appropriate fall speed
        if (isFastFalling)
        {
            player.SetVelocityY(-player.FastFallSpeed);
        }
        else
        {
            // Use default gravity/fall speed with multiplier
            float currentVelocityY = player.rb.velocity.y;
            if (currentVelocityY < 0)
            {
                player.rb.velocity += Vector2.up * Physics2D.gravity.y * (player.FallMultiplier - 1) * Time.deltaTime;
            }
            if (currentVelocityY < -player.MoveSpeed) // Cap the fall speed
            {
                player.SetVelocityY(-player.MoveSpeed);
            }
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

    public override void Exit()
    {
        base.Exit();
        isFastFalling = false;
    }
}

public class AirAttackState : PlayerState
{
    private bool isAnimationFinished = false;
    private float freezeTime = 0.2f; // Reduced freeze time for more responsive air attacks
    private float freezeTimer = 0f;
    private attack_point attackPoint;

    public AirAttackState(Player player, StateMachine stateMachine) : base(player, stateMachine) 
    {
        attackPoint = player.GetComponentInChildren<attack_point>(true);
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint component not found in player children!");
        }
    }    

    public override void Enter()
    {
        base.Enter();
        
        isAnimationFinished = false;
        freezeTimer = 0f;
        player.animator.Play("air_atk");
        
        // Freeze player briefly at the start of the attack
        player.SetVelocityX(0);
        player.SetVelocityY(0);
        
        // Use attack index 0 for air attack
        attackPoint?.StartAttack(0);
    }

    public override void HandleInput()
    {
        // No input handling during the initial freeze
        if (freezeTimer <= freezeTime) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.MoveSpeed * 0.5f);
        player.UpdateFacingDirection(inputX);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        freezeTimer += Time.deltaTime;

        // Check if animation is finished
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("air_atk") && stateInfo.normalizedTime >= 1f)
        {
            // Add a small downward velocity after attack
            player.SetVelocityY(-player.MoveSpeed * 0.5f);
            stateMachine.ChangeState(player.fallState);
            return;
        }

        // Keep player frozen during initial attack frames
        if (freezeTimer <= freezeTime)
        {
            player.SetVelocityX(0);
            player.SetVelocityY(0);
        }
    }

    public override void PhysicsUpdate()
    {
        // If player somehow lands during air attack, transition to idle
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        isAnimationFinished = false;
        attackPoint?.EndAttack();
    }
}

public class DashState : PlayerState
{
    private float dashTimeLeft;

    public DashState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        dashTimeLeft = player.DashTime;
        player.animator.Play("roll"); // Play dash animation

        // set dash velocity based on the player's facing direction
        float dash_direction = player.IsFacingRight ? 1f : -1f;
        player.SetVelocityX(dash_direction * player.DashSpeed); // Set dash velocity
        // player.SetVelocityX(dash_direction * player.transform.localScale.x * player.DashSpeed); // Set dash velocity
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
            // check if player is grounded before changing state
            if (player.IsGrounded())
            {
                stateMachine.ChangeState(player.idleState);
            }
            else
            {
                // If not grounded, transition to fall state
                stateMachine.ChangeState(player.fallState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        // Maintain dash velocity in correct direction
        float dash_direction = player.IsFacingRight ? 1f : -1f;
        player.SetVelocityX(dash_direction * player.DashSpeed);
        // player.SetVelocityX(dash_direction * player.transform.localScale.x * player.DashSpeed);
    }

    public override void Exit()
    {
        // Reset velocity when exiting dash
        player.SetVelocityX(0);
    }
}
public class AttackState : PlayerState
{
    private bool attackInProgress = false;
    private string[] attackAnimations = { "1_atk", "2_atk", "3_atk" };
    private attack_point attackPoint;
    private bool isAnimationFinished = false;

    public AttackState(Player player, StateMachine stateMachine) : base(player, stateMachine)
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
        attackInProgress = true;
        isAnimationFinished = false;
        PlayCurrentAttack();
    }

    public override void HandleInput()
    {
        // Input handling is now done in the Player class
    }

    public override void LogicUpdate()
    {
        if (!attackInProgress) return;

        base.LogicUpdate();

        // Get current animation progress
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = stateInfo.normalizedTime % 1f;

        // Check if animation is finished
        if (normalizedTime >= 0.95f && !isAnimationFinished)
        {
            isAnimationFinished = true;
            ResetAttack();
            stateMachine.ChangeState(player.idleState);
        }
    }

    private void PlayCurrentAttack()
    {
        if (attackPoint == null)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        isAnimationFinished = false;
        player.animator.Play(attackAnimations[player.CurrentAttackIndex]);
        player.SetVelocityX(0);
        attackPoint.StartAttack(player.CurrentAttackIndex);
    }

    private void ResetAttack()
    {
        attackInProgress = false;
        isAnimationFinished = false;
        attackPoint?.EndAttack();
    }

    public override void Exit()
    {
        base.Exit();
        ResetAttack();
    }
}
public class TakeHitState : PlayerState
{
    private bool animationCompleted = false;

    public TakeHitState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

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
        base.LogicUpdate();

        // Get the current animation state info
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // If the "take hit" animation has finished, transition out
        if (stateInfo.IsName("take_hit") && stateInfo.normalizedTime >= 1f)
        {
            if (player.Health <= 0)
            {
                stateMachine.ChangeState(player.deathState);
            }
            else if (player.IsGrounded())
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
    private bool hasRespawned = false;
    private float deathTimer = 0f;

    public DeathState(Player player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.animator.Play("death");
        player.SetVelocityX(0);
        player.SetVelocityY(0);
        hasRespawned = false;
        deathTimer = 0f;
    }

    public override void LogicUpdate()
    {
        // Wait for animation to finish
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        deathTimer += Time.deltaTime;

        if (!hasRespawned && stateInfo.IsName("death") && stateInfo.normalizedTime >= 1f)
        {
            hasRespawned = true;
            player.RespawnAtLevelStart();
        }
    }
}
