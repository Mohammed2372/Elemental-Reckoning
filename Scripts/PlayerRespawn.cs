using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 lastCheckpointPosition;

    private void Start()
    {
        // Optionally, set the initial checkpoint to the player's starting position
        lastCheckpointPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        lastCheckpointPosition = checkpointPosition;
    }

    public void Respawn()
    {
        // Decrease health by 10 on death
        Player_Health playerHealth = GetComponent<Player_Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10);
        }
        transform.position = lastCheckpointPosition;
        // Optionally, reset velocity or play effects here
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("KillZone"))
        {
            Respawn();
        }
    }
}