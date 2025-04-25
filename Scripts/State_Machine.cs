using UnityEngine;
using System.Collections.Generic;
using System;

public class StateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        newState.Enter();
    }
}
public class IdleState : PlayerState
{
    public IdleState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.SetVelocityX(0);
        player.Animator.Play("idle");
    }

    public override void HandleInput()
    {
        if (Input.GetButtonDown("Jump"))
            stateMachine.ChangeState(player.JumpState);
        //else if (Input.GetButtonDown("Fire1"))
            //stateMachine.ChangeState(player.LightAttackState);
        else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0)
            stateMachine.ChangeState(player.MoveState);
    }
}
public class MoveState : PlayerState
{
    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.Play("run");
    }

    public override void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");

        // Flip player based on direction
        if (inputX != 0)
            player.transform.localScale = new Vector3(Mathf.Sign(inputX), 1, 1);

        player.SetVelocityX(inputX * player.moveSpeed);

        if (inputX == 0)
            stateMachine.ChangeState(player.IdleState);

        if (Input.GetButtonDown("Jump"))
            stateMachine.ChangeState(player.JumpState);

        if (Input.GetKeyDown(KeyCode.LeftShift))
            stateMachine.ChangeState(player.DashState);

        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            stateMachine.ChangeState(player.AttackState);

    }

    public override void PhysicsUpdate()
    {
        if (!player.IsGrounded())
            stateMachine.ChangeState(player.JumpState);
    }
}
public class JumpState : PlayerState
{
    private bool hasJumped = false;

    public JumpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        hasJumped = false;
        player.Animator.Play("j_up");
    }

    public override void HandleInput()
    {
        if (!hasJumped)
        {
            player.SetVelocityY(player.jumpForce);
            hasJumped = true;
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        player.SetVelocityX(inputX * player.moveSpeed);

        if (inputX != 0)
            player.transform.localScale = new Vector3(Mathf.Sign(inputX), 1, 1);

        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState);
        }

        // handle air attack 
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.AirAttackState);
        }
    }

    public override void PhysicsUpdate()
    {
        if (player.rb.velocity.y < 0) // Falling
        {
            player.Animator.Play("j_down"); // Play "jump down" animation
        }
    }
}
public class DashState : PlayerState
{
    private float dashTimeLeft;

    public DashState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        dashTimeLeft = player.dashTime;
        player.Animator.Play("Dash");
    }

    public override void LogicUpdate()
    {
        dashTimeLeft -= Time.deltaTime;
        float dir = player.transform.localScale.x;

        player.SetVelocityX(dir * player.dashSpeed);

        if (dashTimeLeft <= 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void HandleInput()
    {
        // You can ignore input during dash or interrupt with some logic if needed
    }
}
public class AttackState : PlayerState
{
    private int currentAttackIndex = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 0.8f;
    private float attackDelayTimer = 0f; // Timer to prevent attacking during animation

    private string[] attackAnimations = { "1_atk", "2_atk", "3_atk" };
    private bool canQueueNextAttack = false;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        PlayCurrentAttackAnimation();
    }

    public override void HandleInput()
    {
        // Prevent input if the attack delay timer is active
        if (attackDelayTimer > 0) return;

        if (Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)) // Check if the attack button is being held
        {
            if (canQueueNextAttack && currentAttackIndex < attackAnimations.Length - 1)
            {
                // Check if the player has acquired the skill for the next attack
                if ((currentAttackIndex == 0 && player.HasSecondAttackSkill) ||
                    (currentAttackIndex == 1 && player.HasThirdAttackSkill))
                {
                    currentAttackIndex++;
                    canQueueNextAttack = false;
                    PlayCurrentAttackAnimation();
                }
            }
        }
    }

    public override void LogicUpdate()
    {
        // Update the attack delay timer
        if (attackDelayTimer > 0)
        {
            attackDelayTimer -= Time.deltaTime;
        }

        comboTimer += Time.deltaTime;

        if (comboTimer > comboResetTime)
        {
            ResetCombo();
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public void AnimationTrigger_CanQueueNext()
    {
        canQueueNextAttack = true;
    }

    public void AnimationTrigger_EndAttack()
    {
        if (!canQueueNextAttack || currentAttackIndex >= attackAnimations.Length - 1)
        {
            ResetCombo();
            stateMachine.ChangeState(player.IdleState);
        }
    }

    private void PlayCurrentAttackAnimation()
    {
        comboTimer = 0f;

        // Play the current attack animation
        string animationName = attackAnimations[currentAttackIndex];
        player.Animator.Play(animationName);

        // Set the attack delay timer based on the animation duration
        attackDelayTimer = GetAnimationClipLength(animationName);
    }

    private void ResetCombo()
    {
        currentAttackIndex = 0;
        comboTimer = 0f;
        canQueueNextAttack = false;
    }

    private float GetAnimationClipLength(string animationName)
    {
        // Retrieve the animation clip length from the Animator
        AnimationClip[] clips = player.Animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 0f; // Default to 0 if the animation clip is not found
    }
}
public class AirAttackState : PlayerState
{
    public AirAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.Play("air_atk"); // Play air attack animation
        player.SetVelocityY(0); // Optional: Stop vertical movement during attack
    }

    public override void HandleInput()
    {
        // No additional input handling during air attack
    }

    public override void LogicUpdate()
    {
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState); // Transition to idle when grounded
        }
        else if (player.rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.JumpState); // Transition back to jump state if still in air
        }
    }
}
