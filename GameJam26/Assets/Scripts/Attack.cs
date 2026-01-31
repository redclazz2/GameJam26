using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject owner;
    public float damage;

    void Start()
    {
        Destroy(gameObject, 0.2f);
    }

    void OnDestroy()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the owner
        if (other.gameObject == owner)
            return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // Pasar la posición del atacante para el knockback
            Vector2 attackerPosition = owner != null ? owner.transform.position : transform.position;
            player.TakeDamage(damage, attackerPosition);
            Destroy(gameObject);
        }
    }
}
