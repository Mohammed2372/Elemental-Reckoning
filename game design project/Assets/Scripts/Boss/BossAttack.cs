using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAttack : MonoBehaviour
{
    [Header("Attack Zones (Assign in Inspector)")]
    public Transform attackZone1;
    public Transform attackZone2;
    public Transform attackZone3;
    public Transform attackZone4;

    [Header("Special Attack Zones")]
    public Transform stunZone;
    public Transform burnZone;

    [Header("Damage Values")]
    public float damage1 = 40f;
    public float damage2 = 35f;
    public float damage3 = 100f;
    public float damage4 = 300f;

    [Header("Special Attack Settings")]
    public float burnDamage = 10f;
    public float burnDuration = 3f;
    public float burnInterval = 1f;
    public float stunDuration = 2f;

    [Header("Attack Control")]
    public float attackCooldown = 1.5f;
    public float attackDecisionRange = 4.5f;

    private bool canAttack = true;
    private bool isAttacking = false;

    private Transform player;
    private Animator animator;
    private BossMove enemyMovement;
    private CharacterStats bossHealth;

    private Coroutine cooldownRoutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        enemyMovement = GetComponent<BossMove>();
        bossHealth = GetComponent<CharacterStats>();
    }

    void Update()
    {
        if (canAttack && !isAttacking && player != null)
        {
            float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);

            if (horizontalDistance <= attackDecisionRange)
            {
                FacePlayerByScale();
                DecideAndStartAttack();
            }
        }
    }

    void DecideAndStartAttack()
    {
        if (!canAttack || isAttacking || player == null) return;

        enemyMovement.StopMovement();

        isAttacking = true;
        canAttack = false;

        float healthPercentage = bossHealth.GetCurrentHealthPercentage();

        int attackIndex = -1;

        if (healthPercentage > 75f)
        {
            attackIndex = 0;
        }
        else if (healthPercentage > 50f)
        {
            attackIndex = Random.Range(0, 4);
        }
        else if (healthPercentage > 25f)
        {
            attackIndex = Random.Range(2, 4);
        }
        else
        {
            attackIndex = 3;
        }

        switch (attackIndex)
        {
            case 0:
                animator.SetTrigger("atk1");
                break;
            case 1:
                animator.SetTrigger("atk2");
                break;
            case 2:
                animator.SetTrigger("atk3");
                break;
            case 3:
                animator.SetTrigger("atk*");
                break;
        }

        // Attack end and cooldown handled via animation event
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;

        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        enemyMovement.ResumeMovement();
    }

    private void FacePlayerByScale()
    {
        if (player == null) return;

        float bossScaleX = transform.localScale.x;
        bool playerIsToLeft = player.position.x < transform.position.x;

        if (playerIsToLeft && bossScaleX > 0)
        {
            FlipScale();
        }
        else if (!playerIsToLeft && bossScaleX < 0)
        {
            FlipScale();
        }
    }

    private void FlipScale()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void DealDamagePart1() => DealDamageFromZone(attackZone1, damage1);
    public void DealDamagePart2() => DealDamageFromZone(attackZone2, damage2);
    public void DealDamagePart3() => DealDamageFromZone(attackZone3, damage3);
    public void DealDamagePart4() => DealDamageFromZone(attackZone4, damage4);
    public void DealStunAttack() => StunFromZone(stunZone, stunDuration);
    public void DealBurnAttack() => BurnFromZone(burnZone, burnDamage, burnDuration, burnInterval);

    private void DealDamageFromZone(Transform zone, float damage)
    {
        if (zone == null) return;

        FlipAttackZone(zone);

        Collider2D[] hits = GetHitsInZone(zone);

        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Player"))
            {
                CharacterStats health = hit.GetComponent<CharacterStats>();
                if (health != null)
                {
                    health.TakeDamage((int)damage);
                }
            }
        }
    }

    private void StunFromZone(Transform zone, float duration)
    {
        if (zone == null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        Animator playerAnimator = playerObj.GetComponent<Animator>();
        Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
        Collider2D zoneCollider = zone.GetComponent<Collider2D>();

        if (rb == null || playerAnimator == null || playerCollider == null || zoneCollider == null) return;

        if (!playerCollider.IsTouching(zoneCollider)) return;
        if (!playerAnimator.GetBool("isGrounded")) return;

        Vector2 center = zoneCollider.bounds.center;
        rb.velocity = Vector2.zero;
        rb.MovePosition(center);

        playerAnimator.SetFloat("speed", 0f);

        MonoBehaviour[] allScripts = playerObj.GetComponents<MonoBehaviour>();
        List<Behaviour> disabled = new List<Behaviour>();

        foreach (var script in allScripts)
        {
            if (!(script is CharacterStats) && script.enabled)
            {
                script.enabled = false;
                disabled.Add(script);
            }
        }

        playerObj.GetComponent<MonoBehaviour>().StartCoroutine(RestoreScriptsAfterDelay(disabled, duration));
    }

    private IEnumerator RestoreScriptsAfterDelay(List<Behaviour> scripts, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var script in scripts)
        {
            if (script != null)
                script.enabled = true;
        }
    }

    private void BurnFromZone(Transform zone, float damage, float duration, float interval)
    {
        if (zone == null) return;

        FlipAttackZone(zone);
        Collider2D[] hits = GetHitsInZone(zone);

        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Player"))
            {
                StartCoroutine(ApplyBurnDamage(hit.GetComponent<CharacterStats>(), damage, duration, interval));
            }
        }
    }

    private IEnumerator ApplyBurnDamage(CharacterStats target, float damage, float duration, float interval)
    {
        if (target == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target.CurrentHealth <= 0) break;

            target.TakeDamage((int)damage, false);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void FlipAttackZone(Transform zone)
    {
        if (zone == null) return;

        float scaleX = Mathf.Abs(zone.localScale.x);
        zone.localScale = new Vector3(transform.localScale.x < 0 ? -scaleX : scaleX, zone.localScale.y, zone.localScale.z);
    }

    private Collider2D[] GetHitsInZone(Transform zone)
    {
        if (zone == null) return new Collider2D[0];

        Collider2D col = zone.GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
            return Physics2D.OverlapBoxAll(box.bounds.center, box.bounds.size, 0f);
        else if (col is CircleCollider2D circle)
            return Physics2D.OverlapCircleAll(circle.bounds.center, circle.radius);

        return new Collider2D[0];
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmo(attackZone1);
        DrawGizmo(attackZone2);
        DrawGizmo(attackZone3);
        DrawGizmo(attackZone4);
        DrawGizmo(stunZone);
        DrawGizmo(burnZone);
    }

    private void DrawGizmo(Transform zone)
    {
        if (zone == null) return;

        Collider2D col = zone.GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.red;

        if (col is BoxCollider2D box)
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        else if (col is CircleCollider2D circle)
            Gizmos.DrawWireSphere(circle.bounds.center, circle.radius);
    }
}
