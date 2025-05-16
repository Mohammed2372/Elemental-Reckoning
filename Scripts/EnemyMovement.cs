using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyPathfinding))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyPathfinding pathfinding;

    public float maxSpeed = 2.5f;
    public float nextWaypointDistance = 0.5f;

    private int currentWaypoint = 0;
    private Path currentPath;

    private int health = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = health;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        pathfinding = GetComponent<EnemyPathfinding>();
    }

    void FixedUpdate()
    {
        currentPath = pathfinding.CurrentPath;
        if (currentPath == null || currentPath.vectorPath.Count == 0)
            return;

        if (currentWaypoint >= currentPath.vectorPath.Count)
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("speed", 0f);
            return;
        }

        Vector2 targetPos = currentPath.vectorPath[currentWaypoint];
        Vector2 direction = (targetPos - rb.position).normalized;

        // Walking enemies move only on the X axis
        Vector2 desiredVelocity = new Vector2(direction.x * maxSpeed, rb.velocity.y);
        rb.velocity = desiredVelocity;

        float distance = Vector2.Distance(rb.position, targetPos);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Flip sprite to face movement direction
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        animator.SetFloat("speed", Mathf.Abs(rb.velocity.x));
    }

    public void ResetWaypoint()
    {
        currentWaypoint = 0;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took " + damage + " damage. Current health: " + currentHealth);
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
