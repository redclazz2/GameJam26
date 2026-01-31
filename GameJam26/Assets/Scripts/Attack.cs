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
        if (other.gameObject == owner)
            return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Vector2 attackerPosition = owner != null ? owner.transform.position : transform.position;
            player.TakeDamage(damage, attackerPosition);

            HitStop.Instance?.TriggerHitStop();

            ScreenShakeManager.Instance?.TriggerShake();

            Destroy(gameObject);
        }
    }
}
