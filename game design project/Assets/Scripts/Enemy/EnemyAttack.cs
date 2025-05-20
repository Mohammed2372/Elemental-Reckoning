using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Collider2D attackZone;
    public int damage = 10;
    public float attackCooldown = 1.5f;
    public LayerMask targetLayer;

    private Animator animator;
    private float nextAttackTime;
    private Transform target;

    private EnemyMove enemyMovement; // Reference to movement script

    void Start()
    {
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        enemyMovement = GetComponent<EnemyMove>();
    }

    void Update()
    {
        if (Time.time >= nextAttackTime && PlayerInRange())
        {
            animator.SetTrigger("attack");

            // Disable movement & flipping during attack
            if (enemyMovement != null)
                enemyMovement.enabled = false;

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    bool PlayerInRange()
    {
        Collider2D[] results = new Collider2D[1];
        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = targetLayer
        };

        int count = Physics2D.OverlapCollider(attackZone, filter, results);

        return count > 0 && results[0].CompareTag("Player");
    }

    // Animation Event: Call this at the end of attack animation
    public void OnAttackEnd()
    {
        // Re-enable movement & flipping
        if (enemyMovement != null)
            enemyMovement.enabled = true;
    }

    // Animation Event
    public void DealDamage()
    {
        Collider2D[] results = new Collider2D[1];
        int hits = Physics2D.OverlapCollider(attackZone, new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = targetLayer
        }, results);

        if (hits > 0)
        {
            Collider2D playerCollider = results[0];
            if (playerCollider != null)
            {
                Player playerScript = playerCollider.GetComponent<Player>();
                if (playerScript != null && !playerScript.IsRolling())
                {
                    CharacterStats stats = playerCollider.GetComponent<CharacterStats>();
                    if (stats != null)
                    {
                        stats.TakeDamage(damage);
                    }
                }
            }
        }
    }
}
