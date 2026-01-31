using UnityEngine;

public enum FacingDirection
{
    Right,
    Left
}

public class Player : MonoBehaviour
{
    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Stats")]
    [SerializeField] private float health = 100f;

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
        // Input horizontal
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Agacharse (solo en el suelo)
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
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
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) 
            && isGrounded && !isCrouching)
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

        // Detectar doble tap derecha
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
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
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
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
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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
    
    // Método para deshabilitar movimiento (útil durante ataques)
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}
