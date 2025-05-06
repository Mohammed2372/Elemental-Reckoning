using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyPathfinding))]
public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 6f;
    public float attackCooldown = 10f;
    public int attackDamage = 10;

    private float lastAttackTime = 0f;
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

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
      
    }

    void Attack()
    {
        animator.SetTrigger("attack");

        // Optional: Deal damage
        /*
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            var health = hit.GetComponent<PlayerHealth>();
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
