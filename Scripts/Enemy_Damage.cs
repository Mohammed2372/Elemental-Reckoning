using UnityEngine;

public class Enemy_Damage : MonoBehaviour
{
    public int damage;
    public Player_Health playerHealth;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            playerHealth.TakeDamage(damage);
        }
    }

}
