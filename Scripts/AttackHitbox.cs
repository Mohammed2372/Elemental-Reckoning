using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Collider2D[] attackColliders;
    [SerializeField] private int[] damageValues = { 10, 15, 20 };
    
    private void Start()
    {
        // Disable all colliders initially
        foreach (var collider in attackColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableHitbox(int attackIndex)
    {
        if (attackIndex >= 0 && attackIndex < attackColliders.Length)
        {
            // Disable all other hitboxes
            DisableAllHitboxes();
            // Enable only the specified hitbox
            attackColliders[attackIndex].enabled = true;
        }
    }

    public void DisableAllHitboxes()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = false;
        }
    }

    public int GetDamage(int attackIndex)
    {
        if (attackIndex >= 0 && attackIndex < damageValues.Length)
        {
            return damageValues[attackIndex];
        }
        return 10; // Default damage
    }
}