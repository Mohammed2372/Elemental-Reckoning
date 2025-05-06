using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class EnemyPathfinding : MonoBehaviour
{
    public Transform target;
    public EnemyAI.EnemyType enemyType = EnemyAI.EnemyType.Walking;

    private Seeker seeker;
    private Path path;
    private Rigidbody2D rb;

    public float updateRate = 0.5f;
    
    public Path CurrentPath => path;
    public Transform Target => target; // Exposed for other scripts

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating(nameof(UpdatePath), 0f, updateRate);
    }

  void UpdatePath()
{
    if (seeker.IsDone() && target != null)
    {
        int graphMask = (enemyType == EnemyAI.EnemyType.Walking)
            ? GraphMask.FromGraphName("Grid Graph")
            : GraphMask.FromGraphName("Point Graph");

        // Compute direction from enemy to target
        float directionToTargetX = target.position.x - transform.position.x;

        // Apply dynamic offset
        float offsetX = 4.5f;
        Vector2 offsetTarget = target.position;
        offsetTarget.x += directionToTargetX > 0 ? -offsetX : offsetX;

        seeker.StartPath(rb.position, offsetTarget, OnPathComplete, graphMask);
    }
}

    void OnPathComplete(Path p)
{
    if (!p.error)
    {
        path = p;

        // Reset the waypoint counter on new path
        var movement = GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.ResetWaypoint();
        }
    }
}
}
