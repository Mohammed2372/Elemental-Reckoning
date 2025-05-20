using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public Collider2D attackZone1;
    public Collider2D attackZone2;
    public Collider2D attackZone3;
    public Collider2D attackZoneSpecial;

    public int dmg1 = 50;
    public int dmg2 = 100;
    public int dmg3 = 200;
    public int dmgSpecial = 300;

    public LayerMask targetLayer;
    private Collider2D[] _hitTargets = new Collider2D[10];

    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
    }

    public void DealDamage(Collider2D zone, int damage)
    {
        int hits = Physics2D.OverlapCollider(zone, new ContactFilter2D
        {
            layerMask = targetLayer,
            useLayerMask = true
        }, _hitTargets);

        for (int i = 0; i < hits; i++)
        {
            if (_hitTargets[i].CompareTag("Target"))
            {
                var health = _hitTargets[i].GetComponent<CharacterStats>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }
    }

    // Animation Event wrappers
    public void Attack1Hit() => DealDamage(attackZone1, dmg1);
    public void Attack2Hit() => DealDamage(attackZone2, dmg2);
    public void Attack3Hit() => DealDamage(attackZone3, dmg3);
    public void SpecialAttack() => DealDamage(attackZoneSpecial, dmgSpecial);

    // Untouchable state control during special
    public void EnterSpecialState()
    {
        if (rb != null) rb.simulated = false;
        if (capsule != null) capsule.enabled = false;
    }

    public void ExitSpecialState()
    {
        if (rb != null) rb.simulated = true;
        if (capsule != null) capsule.enabled = true;
    }
}
