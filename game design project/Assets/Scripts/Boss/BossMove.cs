using UnityEngine;
using System.Collections; //Take this
public class BossMove : MonoBehaviour
{
    public string playerTag = "Player";
    public float moveSpeed = 3f;

    public bool isAttacking = false;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 localScale;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player with tag '" + playerTag + "' not found!");
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        localScale = transform.localScale;

        // Freeze rotation so it stays upright
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Only move when not attacking
        if (isAttacking)
        {
            animator.SetFloat("speed", 0);
            return;
        }

        // Determine direction
        Vector2 direction = player.position - transform.position;
        direction.y = 0; // Optional: Only move horizontally

        // Move
        Vector2 move = direction.normalized * moveSpeed;
        rb.velocity = new Vector2(move.x, rb.velocity.y); // Note: Use rb.velocity to allow physics-based movement

        // Flip sprite
        if (move.x > 0)
            FaceRight();
        else if (move.x < 0)
            FaceLeft();

        // Update animation speed parameter
        if (animator != null)
        {
            float currentSpeed = Mathf.Abs(rb.velocity.x);
            animator.SetFloat("speed", currentSpeed);
        }
    }

    void FaceRight()
    {
        localScale.x = Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    void FaceLeft()
    {
        localScale.x = -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    // Stop movement (called from FireKnightAttack when attack starts)
    public void StopMovement()
    {
        rb.velocity = Vector2.zero; // Stop movement by setting velocity to zero
        isAttacking = true; // Prevent further movement while attacking
    }

    // Resume movement (called after attack finishes)
    public void ResumeMovement()
    {
        isAttacking = false; // Allow movement again
    }
    // Take this
    public void SetSpeedMultiplier(float multiplier, float duration)
{
    StartCoroutine(ApplySpeedMultiplier(multiplier, duration));
}

     // Take this
    private IEnumerator ApplySpeedMultiplier(float multiplier, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }
}
