using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar; // Reference to the HealthBar script
    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        if (player != null)
        {
            int maxHealth = player.Health; // Use Player's current health as maxHealth
            int health = player.Health;
            if (healthBar != null)
                healthBar.SetHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (player != null)
        {
            int health = player.Health - damage;
            if (healthBar != null)
                healthBar.SetHealth(health, player.Health);
        }
    }

    public void Heal(int amount)
    {
        if (player != null)
        {
            int health = player.Health + amount;
            if (healthBar != null)
                healthBar.SetHealth(health, player.Health);
        }
    }

    public void UpdateHealthBar(int current, int max)
    {
        // Debug.Log($"Updating health bar: {current}/{max}");
        if (healthBar != null)
            healthBar.SetHealth(current, max);
    }
}
