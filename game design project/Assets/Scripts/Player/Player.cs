using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Roll")]
    public float rollForce = 20f;

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;

    [Header("References")]
    public CharacterStats stats;

    [Header("Ground Detection")]
    public LayerMask groundLayer;

    [Header("Collision Settings")]
    public LayerMask targetLayersToIgnoreDuringRoll;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    private PlayerInputActions input;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool defendHeld;
    private bool rollPressed;

    private bool isGrounded = false;
    private float lastDirection = 1f;
    private bool isRolling = false;
    private bool isAttacking = false;

    private float attackTimer = 0f;

    private int playerLayer;

    private void Awake()
    {
        input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        input.Player.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += ctx => jumpPressed = true;
        input.Player.Defend.performed += ctx => defendHeld = true;
        input.Player.Defend.canceled += ctx => defendHeld = false;
        input.Player.Roll.performed += ctx => rollPressed = true;

        input.Player.Attack1.performed += ctx => TriggerAttack(1);
        input.Player.Attack2.performed += ctx => TryManaAttack(2, 25f);
        input.Player.Attack3.performed += ctx => TryManaAttack(3, 50f);
        input.Player.Super_Attack.performed += ctx => TryUltimateAttack();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        playerLayer = gameObject.layer;
    }

    private void Update()
    {
        CheckGrounded();

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        if (!isRolling && !isAttacking)
        {
            HandleMovement();
            HandleJump();
            HandleDefend();
        }

        HandleAnimator();
        HandleRoll();

        jumpPressed = false;
        rollPressed = false;
    }

    private void CheckGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        float extra = 0.1f;
        isGrounded = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, extra, groundLayer);
    }

    private void HandleMovement()
    {
        float moveX = moveInput.x;
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        if (moveX > 0) lastDirection = 1f;
        else if (moveX < 0) lastDirection = -1f;

        transform.rotation = Quaternion.Euler(0, lastDirection == 1f ? 0 : 180, 0);
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded )
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void HandleDefend()
    {
        animator.SetBool("isDefending", defendHeld);
    }

    private void HandleRoll()
    {
        if (rollPressed && isGrounded && !isRolling && !isAttacking )
        {
            animator.SetTrigger("roll");
        }
    }

    private void HandleAnimator()
    {
        animator.SetFloat("speed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("isJumping", rb.velocity.y > 0.1f);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void TriggerAttack(int index)
    {
        if (isRolling || isAttacking || attackTimer > 0f) return;

        isAttacking = true;
        attackTimer = attackCooldown;
        rb.velocity = Vector2.zero;

        animator.SetInteger("attackIndex", index);
        animator.SetTrigger("attack");
    }

    private void TryManaAttack(int index, float manaCost)
    {
        // Exit early if conditions aren't met
        if (stats == null) return;
        if (isRolling || isAttacking || attackTimer > 0f) return; // Cooldown check first
        if (stats.CurrentMana < manaCost)
        {
            Debug.Log("Not enough mana!");
            return;
        }

        // Deduct mana and trigger attack
        stats.UseMana(manaCost);
        TriggerAttack(index);
    }

    private void TryUltimateAttack()
    {
        if (stats == null) return;
        if (!stats.IsUltimateFull) { Debug.Log("Ultimate not ready!"); return; }

        stats.ResetUltimate();
        TriggerAttack(4);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void StartRoll()
    {
        isRolling = true;
        float dir = lastDirection == 1f ? 1f : -1f;
        rb.velocity = new Vector2(dir * rollForce, 0f);

        foreach (var layer in GetLayersFromMask(targetLayersToIgnoreDuringRoll))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, layer, true);
        }
    }

    public void EndRoll()
    {
        isRolling = false;

        foreach (var layer in GetLayersFromMask(targetLayersToIgnoreDuringRoll))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, layer, false);
        }
    }

    private int[] GetLayersFromMask(LayerMask mask)
    {
        List<int> layers = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                layers.Add(i);
            }
        }
        return layers.ToArray();
    }

 

    public bool IsRolling() => isRolling;

    // ─────────────── NEW: Reset "isAttacked" trigger via Animation Event ───────────────
    public void ResetAttackedTrigger()
    {
        animator.ResetTrigger("isAttacked");
    }
}
