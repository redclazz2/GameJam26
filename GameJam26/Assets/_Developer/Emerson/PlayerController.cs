using UnityEngine;
using System.Collections; // Necesario para las corrutinas

public class PlayerController : MonoBehaviour
{
    // --- CONFIGURACIÓN ---
    [Header("Ajustes de Movimiento")]
    public float velocidad = 8f;
    public float fuerzaSalto = 15f;

    [Header("Referencias")]
    public Rigidbody2D rb;
    public Transform piesPosicion;
    public float radioPies = 0.3f;
    public LayerMask capaSuelo;

    [Header("Referencias de Ataque")]
    public GameObject hbDebil;  // Arrastra aquí la Hitbox_Debil
    public GameObject hbFuerte; // Arrastra aquí la Hitbox_Fuerte

    // --- MÁQUINA DE ESTADOS ---
    public enum Estado { Idle, Walk, Jump, Attack, Stun }
    public Estado estadoActual;

    // --- VARIABLES PRIVADAS ---
    private float inputHorizontal;
    private bool estoyEnSuelo;
    private bool mirandoDerecha = true;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        // Aseguramos que las hitboxes empiecen apagadas
        if(hbDebil) hbDebil.SetActive(false);
        if(hbFuerte) hbFuerte.SetActive(false);
    }

    void Update()
    {
        // Si estamos atacando o aturdidos, no leemos movimiento ni saltos
        if (estadoActual == Estado.Attack || estadoActual == Estado.Stun) return;

        inputHorizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && estoyEnSuelo)
        {
            Saltar();
        }

        // --- NUEVO: DETECCIÓN DE ATAQUES ---
        if (Input.GetKeyDown(KeyCode.Z) && estoyEnSuelo) 
        {
            StartCoroutine(EjecutarAtaque(hbDebil, 0.2f)); // Ataque rápido
        }
        if (Input.GetKeyDown(KeyCode.X) && estoyEnSuelo) 
        {
            StartCoroutine(EjecutarAtaque(hbFuerte, 0.5f)); // Ataque lento
        }

        ActualizarEstado();
        Girar();
    }

    void FixedUpdate()
    {
        // No nos movemos si estamos atacando
        if (estadoActual == Estado.Attack || estadoActual == Estado.Stun) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(inputHorizontal * velocidad, rb.linearVelocity.y);
    }

    // --- CORRUTINA DE ATAQUE ---
    IEnumerator EjecutarAtaque(GameObject hitbox, float duracion)
    {
        estadoActual = Estado.Attack;
        
        if(hitbox != null) hitbox.SetActive(true); // Enciende la "burbuja" de daño
        
        yield return new WaitForSeconds(duracion); // Espera el tiempo del golpe
        
        if(hitbox != null) hitbox.SetActive(false); // Apaga la "burbuja"
        
        estadoActual = Estado.Idle;
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
    }

    void ActualizarEstado()
    {
        estoyEnSuelo = Physics2D.OverlapCircle(piesPosicion.position, radioPies, capaSuelo);

        if (!estoyEnSuelo) estadoActual = Estado.Jump;
        else if (Mathf.Abs(inputHorizontal) > 0.1f) estadoActual = Estado.Walk;
        else if (estadoActual != Estado.Attack) estadoActual = Estado.Idle;
    }

    void Girar()
    {
        if (inputHorizontal > 0 && !mirandoDerecha) Voltear();
        else if (inputHorizontal < 0 && mirandoDerecha) Voltear();
    }

    void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    void OnDrawGizmos()
    {
        if (piesPosicion != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(piesPosicion.position, radioPies);
        }
    }
}