using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private static Vector3 lastCheckpointPosition;

    private void Start()
    {
        // Optionally, set the initial checkpoint to the player's starting position
        lastCheckpointPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        lastCheckpointPosition = checkpointPosition;
        Debug.Log("last checkpoint updated");
        Debug.Log(lastCheckpointPosition);
    }

    public void Respawn()
    {
        // Decrease health by 10 on death
        CharacterStats playerHealth = GetComponent<CharacterStats>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10);
        }
        transform.position = lastCheckpointPosition;
        Debug.Log(lastCheckpointPosition);
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