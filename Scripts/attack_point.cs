using System;
using UnityEngine;

public class attack_point : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerController player;
    private bool isAttacking = false;
    private int currentAttackIndex = 0;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void StartAttack(int attackIndex)
    {
        currentAttackIndex = attackIndex;
        isAttacking = true;
        // Adjust damage based on attack combo (optional)
        damage = 10f + (5f * attackIndex);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking) return;

        // Check if the collided object is in the enemy layer
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            Console.WriteLine($"Hit enemy: {collision.name}");
            // Assuming enemies have a TakeDamage method
            // collision.GetComponent<Enemy>()?.TakeDamage(damage);
        }
    }
}
