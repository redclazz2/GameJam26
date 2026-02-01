using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject owner;
    public float damage;
    public GameObject hitSoundPrefab;
    public GameObject blockSoundPrefab;

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
            player.TakeDamage(damage, attackerPosition, out bool wasBlocked);
            
            // Vibración del mando del atacante al hacer daño
            PlayerInputHandler attackerInputHandler = owner?.GetComponent<PlayerInputHandler>();
            if (attackerInputHandler != null)
            {
                if (wasBlocked)
                    attackerInputHandler.RumbleOnBlocked();
                else
                    attackerInputHandler.RumbleOnDamageDealt();
            }

            // Si el ataque fue bloqueado, empujar al atacante hacia atrás
            if (wasBlocked && owner != null)
            {
                Player attackerPlayer = owner.GetComponent<Player>();
                if (attackerPlayer != null)
                {
                    // Dirección opuesta: el atacante es empujado alejándose del defensor
                    float pushDirection = owner.transform.position.x > player.transform.position.x ? 1f : -1f;
                    attackerPlayer.ApplyExternalKnockback(new Vector2(pushDirection, 0), player.GetBlockPushbackForce());
                }
            }
            
            // Activar HitStop para dar impacto al golpe
            HitStop.Instance?.TriggerHitStop();
            if (wasBlocked)
            {
                Instantiate(blockSoundPrefab, transform.position, Quaternion.identity);   
            }
            else
            {
                Instantiate(hitSoundPrefab, transform.position, Quaternion.identity);    
            }
            
            ScreenShakeManager.Instance?.TriggerShake();

            Destroy(gameObject);
        }
    }
}
