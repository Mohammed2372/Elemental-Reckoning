using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyPathfinding))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 6f;
    public float attackCooldown = 10f;
    public int attackDamage = 10;

    private float lastAttackTime = -Mathf.Infinity;
    private Animator animator;
    private EnemyPathfinding pathfinding;

    void Start()
    {
        animator = GetComponent<Animator>();
        pathfinding = GetComponent<EnemyPathfinding>();
    }

    void FixedUpdate()
    {
        Transform target = pathfinding.Target;
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;

        if (distanceToTarget <= attackRange && canAttack)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        animator.SetTrigger("attack");

        // Optional: Deal damage to player if within range
        /*
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
        */
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
