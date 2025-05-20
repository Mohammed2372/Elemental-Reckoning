using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviour
{
    [Header("Ending Level 1")]
    public GameObject arrow;
    public GameObject rightCollider;

    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("Mana Settings")]
    public float maxMana = 100;
    public float currentMana;

    [Header("Ultimate Settings")]
    public float maxUltimate = 100;
    public float currentUltimate;

    [Header("Mana Regeneration")]
    public float manaRegenRate = 5f; // Mana regenerated per second

    private Animator animator;

    public GameManager gameManager;

    private void addScore(int Score) => gameManager.add_score(Score);

    public float CurrentHealth => currentHealth;
    public float CurrentMana => currentMana;
    public float CurrentUltimate => currentUltimate;

    public bool IsDead => currentHealth <= 0;
    public bool IsUltimateFull => currentUltimate >= maxUltimate;

    // New flag for being attacked during attack cooldown
    public bool isAttacked = false;

    // Attack cooldown control
    private bool attackOnCooldown = false;
    private float attackCooldownTime = 1.5f; // example cooldown duration
    private float attackCooldownTimer = 0f;

    // References to other components to disable/enable
    private EnemyMove enemyMove;
    private EnemyAttack enemyAttack;
    private BossMove bossMove;
    private BossAttack bossAttack;
    private Player playerComponent; // For rolling check

    // Stun control
    private bool isStunned = false;
    public float stunDuration = 0.5f; // Time to stop everything after getting hit

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentUltimate = 0f;
        animator = GetComponent<Animator>();

        enemyMove = GetComponent<EnemyMove>();
        enemyAttack = GetComponent<EnemyAttack>();
        bossMove = GetComponent<BossMove>();
        bossAttack = GetComponent<BossAttack>();
        playerComponent = GetComponent<Player>();
    }

    void Update()
    {
        // Mana regenerates every frame based on manaRegenRate
        RegenerateMana(manaRegenRate * Time.deltaTime);

        // Countdown attack cooldown timer if active
        if (attackOnCooldown)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
            {
                attackOnCooldown = false;
                attackCooldownTimer = 0f;
                // Reset isAttacked when cooldown ends
                isAttacked = false;
            }
        }
    }

    // Call this method externally when attack starts to begin cooldown
    public void StartAttackCooldown(float cooldownDuration = -1f)
    {
        attackOnCooldown = true;
        attackCooldownTimer = (cooldownDuration > 0f) ? cooldownDuration : attackCooldownTime;
        isAttacked = false;
    }

    // HEALTH ---------------------------------------
    public void TakeDamage(int amount, bool triggerAnimation = true)
    {
        if (IsDead) return;

        if (playerComponent != null && playerComponent.IsRolling()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Stop everything on hit
        StopAllActions();

        if (animator != null && triggerAnimation)
        {
            animator.SetTrigger("isAttacked");
        }

        // Resume actions after stun duration
        StartCoroutine(ResumeAfterDelay(stunDuration));

        if (currentHealth <= 0)
        {
            animator.SetBool("killed", true);
            RemoveLogic();
        }
    }

    // Freeze movement, attacks, and input
    private void StopAllActions()
    {
        if (isStunned) return;

        isStunned = true;

        if (enemyMove != null) enemyMove.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;
        if (bossMove != null) bossMove.enabled = false;
        if (bossAttack != null) bossAttack.enabled = false;
    }

    private System.Collections.IEnumerator ResumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!IsDead)
            ResumeAllActions();
    }

    private void ResumeAllActions()
    {
        isStunned = false;

        if (enemyMove != null) enemyMove.enabled = true;
        if (enemyAttack != null) enemyAttack.enabled = true;
        if (bossMove != null) bossMove.enabled = true;
        if (bossAttack != null) bossAttack.enabled = true;
    }

    // 🧠 Removes logic and physical components
    public void RemoveLogic()
    {
        // Disable logic scripts
        if (enemyMove != null) enemyMove.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;
        if (bossAttack != null) bossAttack.enabled = false;
        if (bossMove != null) bossMove.enabled = false;

        // Disable CapsuleCollider2D
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null) capsule.enabled = false;

        // Disable Rigidbody or make it non-interactive
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            rb2D.gravityScale = 0f;
            rb2D.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public float GetCurrentHealthPercentage() => (currentHealth / maxHealth) * 100f;
    public float GetNormalizedHealth() => currentHealth / maxHealth;

    // MANA -----------------------------------------
    public void UseMana(float amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0);
    }

    public void RegenerateMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
    }

    public float GetCurrentManaPercentage() => (currentMana / maxMana) * 100f;
    public float GetNormalizedMana() => currentMana / maxMana;

    // ULTIMATE -------------------------------------
    public void AddUltimate(float amount)
    {
        currentUltimate = Mathf.Min(currentUltimate + amount, maxUltimate);
    }

    public void ResetUltimate()
    {
        currentUltimate = 0;
    }

    public float GetCurrentUltimatePercentage() => (currentUltimate / maxUltimate) * 100f;
    public float GetNormalizedUltimate() => currentUltimate / maxUltimate;

    private void endLevel1()
    {
        if ((SceneManager.GetActiveScene().buildIndex) == 1)
        {
            arrow.SetActive(true);
            rightCollider.SetActive(false);
        }
    }
}
