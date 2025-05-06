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

    public EnemyAI.EnemyType enemyType = EnemyAI.EnemyType.Walking;
    public float maxSpeed = 2.5f;
    public float baseSpeed = 400f;
    private float speed;

    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    public float nextWaypointDistance = 0.5f;

    // Offset the final path point to be slightly to the left/right of the target
    public float pathOffsetX = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        pathfinding = GetComponent<EnemyPathfinding>();
        speed = baseSpeed;
    }

void FixedUpdate()
{
    Path path = pathfinding.CurrentPath;
    Transform target = pathfinding.Target;

    if (path == null || target == null || path.vectorPath.Count == 0)
        return;

   

   if (currentWaypoint >= path.vectorPath.Count)
{
    currentWaypoint = path.vectorPath.Count - 1;
    if (currentWaypoint < 0)
        return;

   
    rb.linearVelocity = Vector2.zero;
    animator.SetFloat("speed", 0);
    return;
}

    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
    if (enemyType == EnemyAI.EnemyType.Walking)
        direction = new Vector2(direction.x, 0);

    // Flip the enemy to face movement direction
    if (direction.x != 0)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    Vector2 force = direction * speed * Time.fixedDeltaTime;
    rb.AddForce(force);

    Vector2 clampedVelocity = rb.linearVelocity;
    clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxSpeed, maxSpeed);
    rb.linearVelocity = new Vector2(clampedVelocity.x, rb.linearVelocity.y);

    float distanceToWaypoint = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
    if (distanceToWaypoint < nextWaypointDistance)
    {
        currentWaypoint++;
    }

    HandleSlopeAndRotation();
    animator.SetFloat("speed", rb.linearVelocity.magnitude);
}


    void HandleSlopeAndRotation()
    {
        if (enemyType == EnemyAI.EnemyType.Walking)
        {
            Vector2 origin = rb.position + Vector2.down * 2.4f;
            float radius = 0.3f;
            float castDistance = 0.1f;
            int groundMask = LayerMask.GetMask("Ground");

            RaycastHit2D hit = Physics2D.CircleCast(origin, radius, Vector2.down, castDistance, groundMask);
            if (hit.collider != null)
            {
                Vector2 normal = hit.normal;
                float angleFromUp = Vector2.Angle(normal, Vector2.up);
                speed = angleFromUp > 1f ? 1000f : baseSpeed;

                float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
    public void ResetWaypoint()
    {
        currentWaypoint = 0;
    }
}
