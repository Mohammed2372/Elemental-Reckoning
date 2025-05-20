using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn respawn = other.GetComponentInParent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.SetCheckpoint(transform.position);
                Debug.Log("Checkpoint set to: " + transform.position);
            }
            else
            {
                Debug.Log("error in script player respawn");
            }
        }
    }
}