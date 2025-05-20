using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class EnemyMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        UpdateAnimator();
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (direction.x > 0 && !facingRight)
        {
            Flip(true);
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip(false);
        }
    }

    void Flip(bool faceRight)
    {
        facingRight = faceRight;
        transform.rotation = Quaternion.Euler(0, faceRight ? 0f : 180f, 0);
    }

    void UpdateAnimator()
    {
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("speed", horizontalSpeed);
    }
}
