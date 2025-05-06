using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class EnemyPathfinding : MonoBehaviour
{
    public Transform target;

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
            GraphMask graphMask = GraphMask.FromGraphName("Grid Graph");

            // Compute direction from enemy to target
            float directionToTargetX = target.position.x - transform.position.x;

            // Apply dynamic offset to prevent enemy clumping
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

            // Reset waypoint in movement controller
            var movement = GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.ResetWaypoint();
            }
        }
    }
}
