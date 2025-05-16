using System;
using UnityEngine;
using System.Collections.Generic;

public class attack_point : MonoBehaviour
{
    private AttackHitbox hitboxManager;
    private int currentAttackIndex = -1;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    private Collider2D[] attackColliders;

    private void Start()
    {
        hitboxManager = GetComponent<AttackHitbox>();
        if (hitboxManager == null)
        {
            Debug.LogError("AttackHitbox component not found!");
        }

        attackColliders = GetComponents<Collider2D>();
        for (int i = 0; i < attackColliders.Length; i++)
        {
            var collider = attackColliders[i];
            collider.isTrigger = true;
        }
    }

    public void StartAttack(int attackIndex)
    {
        currentAttackIndex = attackIndex;
        hitboxManager?.EnableHitbox(attackIndex);
        hitEnemies.Clear();
    }

    public void EndAttack()
    {
        currentAttackIndex = -1;
        hitboxManager?.DisableAllHitboxes();
        hitEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"OnTriggerEnter2D: Collided with name: {other.name}, tag: {other.tag}");
        if (currentAttackIndex == -1) return;

        if ((other.CompareTag("Enemy") || other.CompareTag("Earth_Enemy")) && !hitEnemies.Contains(other))
        {
            Debug.Log("Enemy detected with tag " + other.tag);
            int damage = hitboxManager.GetDamage(currentAttackIndex);
            var enemy = other.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                hitEnemies.Add(other);
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Enemy script not found on the collider.");
            }
        }
    }

    public void FlipColliders(bool facingRight)
    {
        foreach (var collider in attackColliders)
        {
            var t = collider.transform;
            Vector3 localPos = t.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (facingRight ? 1 : -1);
            t.localPosition = localPos;
        }
    }
}
