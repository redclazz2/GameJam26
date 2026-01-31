using UnityEngine;
using UnityEngine.Events;

public enum FacingDirection
{
    Right,
    Left
}

public class Player : MonoBehaviour
{
    [Header("Identificación")]
    [SerializeField] private int playerNumber = 1; // 1 = Jugador 1 (WASD), 2 = Jugador 2 (Flechas)
    public int PlayerNumber => playerNumber;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    // Eventos para el sistema de combate
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged; // Pasa la salud actual
    public UnityEvent<float> OnDamageTaken; // Pasa el daño recibido

    // Propiedades públicas para el sistema de salud
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float backwardSpeedMultiplier = 0.7f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float doubleTapTime = 0.25f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Estados del personaje
    public enum PlayerState { Idle, Walking, Jumping, Crouching, Dashing }
    public PlayerState currentState = PlayerState.Idle;

    // Variables internas
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isCrouching;
    private bool isDashing;
    private bool canDash = true;

    // Variables para doble tap (dash)
    private float lastTapTimeLeft;
    private float lastTapTimeRight;
    private bool waitingForDoubleTapLeft;
    private bool waitingForDoubleTapRight;

    // Dirección del jugador (para saber si va hacia adelante o atrás)
    public int FacingDirection => isFacingRight ? 1 : -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicializar eventos si son null
        OnDeath ??= new UnityEvent();
        OnHealthChanged ??= new UnityEvent<float>();
        OnDamageTaken ??= new UnityEvent<float>();

        // Inicializar salud
        currentHealth = maxHealth;

        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        // Crear groundCheck si no existe
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        // No procesar input durante dash
        if (isDashing) return;

        CheckGrounded();
        HandleInput();
        HandleDoubleTapDash();
        UpdateState();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        ApplyMovement();
        ApplyBetterJump();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleInput()
    {
        // Input horizontal según el jugador
        if (playerNumber == 1)
        {
            // Jugador 1: WASD
            if (Input.GetKey(KeyCode.A))
                horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D))
                horizontalInput = 1f;
            else
                horizontalInput = 0f;
        }
        else
        {
            // Jugador 2: Flechas
            if (Input.GetKey(KeyCode.LeftArrow))
                horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.RightArrow))
                horizontalInput = 1f;
            else
                horizontalInput = 0f;
        }

        // Agacharse (solo en el suelo)
        if (isGrounded)
        {
            bool crouchKey = playerNumber == 1 ? Input.GetKey(KeyCode.S) : Input.GetKey(KeyCode.DownArrow);
            if (crouchKey)
            {
                isCrouching = true;
                horizontalInput = 0; // No moverse mientras está agachado
            }
            else
            {
                isCrouching = false;
            }
        }

        // Salto (solo si está en el suelo y no agachado)
        bool jumpKey = playerNumber == 1 
            ? (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            : Input.GetKeyDown(KeyCode.UpArrow);
        
        if (jumpKey && isGrounded && !isCrouching)
        {
            Jump();
        }

        // Flip del sprite según dirección
        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void HandleDoubleTapDash()
    {
        if (!isGrounded || isCrouching || !canDash) return;

        // Teclas según jugador
        KeyCode rightKey = playerNumber == 1 ? KeyCode.D : KeyCode.RightArrow;
        KeyCode leftKey = playerNumber == 1 ? KeyCode.A : KeyCode.LeftArrow;

        // Detectar doble tap derecha
        if (Input.GetKeyDown(rightKey))
        {
            if (waitingForDoubleTapRight && Time.time - lastTapTimeRight < doubleTapTime)
            {
                StartCoroutine(PerformDash(1));
                waitingForDoubleTapRight = false;
            }
            else
            {
                lastTapTimeRight = Time.time;
                waitingForDoubleTapRight = true;
            }
        }

        // Detectar doble tap izquierda
        if (Input.GetKeyDown(leftKey))
        {
            if (waitingForDoubleTapLeft && Time.time - lastTapTimeLeft < doubleTapTime)
            {
                StartCoroutine(PerformDash(-1));
                waitingForDoubleTapLeft = false;
            }
            else
            {
                lastTapTimeLeft = Time.time;
                waitingForDoubleTapLeft = true;
            }
        }

        // Reset del estado de espera
        if (waitingForDoubleTapRight && Time.time - lastTapTimeRight >= doubleTapTime)
        {
            waitingForDoubleTapRight = false;
        }
        if (waitingForDoubleTapLeft && Time.time - lastTapTimeLeft >= doubleTapTime)
        {
            waitingForDoubleTapLeft = false;
        }
    }

    private void ApplyMovement()
    {
        if (isCrouching)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float speed = walkSpeed;

        // Movimiento hacia atrás es más lento (como en Street Fighter)
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            speed *= backwardSpeedMultiplier;
        }

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void ApplyBetterJump()
    {
        // Caída más rápida para mejor sensación de juego
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Salto corto si sueltas el botón de salto
        else if (rb.linearVelocity.y > 0)
        {
            bool holdingJump = playerNumber == 1 
                ? (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
                : Input.GetKey(KeyCode.UpArrow);
            
            if (!holdingJump)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
    }

    private System.Collections.IEnumerator PerformDash(int direction)
    {
        isDashing = true;
        canDash = false;
        currentState = PlayerState.Dashing;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        
        // Opción 1: Flip con escala
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // Opción 2: Si prefieres usar SpriteRenderer
        // if (spriteRenderer != null)
        //     spriteRenderer.flipX = !isFacingRight;
    }

    private void UpdateState()
    {
        if (isDashing)
        {
            currentState = PlayerState.Dashing;
        }
        else if (!isGrounded)
        {
            currentState = PlayerState.Jumping;
        }
        else if (isCrouching)
        {
            currentState = PlayerState.Crouching;
        }
        else if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            currentState = PlayerState.Walking;
        }
        else
        {
            currentState = PlayerState.Idle;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Actualizar parámetros del Animator
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsDashing", isDashing);
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // Debug: Visualizar groundCheck en el editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // Métodos públicos para otros scripts (ej: sistema de combate)
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
    public bool IsDashing() => isDashing;
    public PlayerState GetCurrentState() => currentState;
    
    // Método para deshabilitar movimiento y ataques (útil durante countdown, fin de ronda, etc.)
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        // También deshabilitar/habilitar el ataque
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = enabled;
        }
        
        if (!enabled && rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public float GetDamage()
    {
        return damage;
    }
    /// <summary>
    /// Aplica daño al jugador
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura al jugador
    /// </summary>
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Resetea la salud al máximo
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Llamado cuando el jugador muere
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto!");
        OnDeath?.Invoke();
        
        // Deshabilitar movimiento al morir
        SetMovementEnabled(false);
    }
}
