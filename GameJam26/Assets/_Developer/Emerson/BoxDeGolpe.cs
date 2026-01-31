using UnityEngine;

public class BoxDeGolpe : MonoBehaviour
{
    public float daño = 10f;
    public float empuje = 5f; 

    void OnTriggerEnter2D(Collider2D otro)
    {
        // 1. PRIMERA PRUEBA: ¿Hay contacto físico?
        // Si no ves este mensaje en la Consola al atacar, el problema son los Colliders o el Rigidbody.
        Debug.Log("¡Colisión detectada con: " + otro.name + "!");

        // 2. SEGUNDA PRUEBA: ¿El objeto tiene el Tag correcto?
        // El Cubo Rojo DEBE tener el Tag "Player" en el Inspector.
        if (otro.CompareTag("Player"))
        {
            // Evitamos que el jugador se golpee a sí mismo
            if (otro.gameObject != transform.root.gameObject)
            {
                if (otro.TryGetComponent<VidaPersonaje>(out var vidaEnemigo))
                {
                    vidaEnemigo.RecibirDaño(daño, transform.root.position);
                    Debug.Log("¡Impacto conectado con éxito a " + otro.name + "!");
                }
                else
                {
                    Debug.LogWarning("Toqué a " + otro.name + " pero NO tiene el script VidaPersonaje.");
                }
            }
        }
        else
        {
            Debug.Log("Toqué a " + otro.name + " pero su Tag es '" + otro.tag + "' en lugar de 'Player'.");
        }
    }
}