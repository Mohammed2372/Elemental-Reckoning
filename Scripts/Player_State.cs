using UnityEngine;

public abstract class PlayerState
{
    protected Player player;
    protected StateMachine stateMachine;
    protected float stateTimer;
    protected bool animationTriggerCalled;

    public PlayerState(Player player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        stateTimer = 0;
        animationTriggerCalled = false;
        // Debug.Log($"Entering {GetType().Name}");
    }

    public virtual void Exit()
    {
        // Clean up any state-specific things
    }

    public virtual void HandleInput() { }
    public virtual void LogicUpdate()
    {
        stateTimer += Time.deltaTime;
    }
    public virtual void PhysicsUpdate() { }

    protected bool AnimationFinished(string animationName)
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1f;
    }

    protected bool IsAnimationPlaying(string animationName)
    {
        return player.animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
}