using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VidaPersonaje : MonoBehaviour
{
    [Header("Configuración")]
    public float vidaMax = 100f;
    public float vidaActual;

    [Header("Interfaz UI")]
    public Slider barraVida;
    public TextMeshProUGUI textoVida;
    public float velocidadSuave = 5f; // Qué tan rápido se desliza la barra

    private Rigidbody2D rb;

    void Start()
    {
        vidaActual = vidaMax;
        rb = GetComponent<Rigidbody2D>();

        if (barraVida != null)
        {
            barraVida.maxValue = vidaMax;
            barraVida.value = vidaMax;
        }
        ActualizarTexto();
    }

    void Update()
    {
        // Esto hace que la barra persiga al valor real de vida suavemente
        if (barraVida != null && barraVida.value != vidaActual)
        {
            barraVida.value = Mathf.Lerp(barraVida.value, vidaActual, velocidadSuave * Time.deltaTime);
        }
    }

    public void RecibirDaño(float cantidad, Vector3 posicionAtacante)
    {
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMax); // No bajar de 0
        
        ActualizarTexto();

        // Empuje (Knockback)
        float direccion = (transform.position.x > posicionAtacante.x) ? 1f : -1f;
        if(rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direccion * 5f, 3f), ForceMode2D.Impulse);
        }

        if (vidaActual <= 0) Morir();
    }

    void ActualizarTexto()
    {
        if (textoVida != null)
            textoVida.text = vidaActual.ToString("F0") + " / " + vidaMax.ToString();
    }

    void Morir()
    {
        Debug.Log("Enemigo derrotado");
        Destroy(gameObject);
    }
}