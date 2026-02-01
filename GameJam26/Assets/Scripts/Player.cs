using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Input System")]
    [SerializeField] private bool useNewInputSystem = true; // Toggle para usar el nuevo sistema
    private PlayerInputHandler inputHandler;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("UI Feedback")]
    [SerializeField] private GameObject blockTextPrefab; // Prefab del texto "Bloqueado"
    [SerializeField] private Transform blockTextSpawnPoint; // Punto donde aparece el texto (encima de la cabeza)
    [SerializeField] private float blockTextDuration = 1f;
    [SerializeField] private float blockTextShakeIntensity = 0.05f; // Intensidad del temblor
    [SerializeField] private float blockTextShakeSpeed = 50f; // Velocidad del temblor
    [SerializeField] private float blockTextRiseSpeed = 1f; // Velocidad de subida

    [Header("Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f; // Stamina por segundo
    [SerializeField] private float staminaRegenDelay = 0.5f; // Delay antes de empezar a regenerar
    [SerializeField] private float attackStaminaCost = 20f; // Costo de atacar
    [SerializeField] private float blockStaminaCost = 15f; // Costo de bloquear un golpe
    [SerializeField] private float blockingStaminaDrainRate = 8f; // Stamina drenada por segundo mientras bloquea
    private float currentStamina;
    private float staminaRegenTimer;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackUpForce = 3f;
    [SerializeField] private float hitstunDuration = 0.2f;
    [SerializeField] private float damageFlashDuration = 0.15f;
    private bool isInHitstun = false;
    private bool isInDamageFlash = false;

    [Header("Bloqueo")]
    [SerializeField] private float blockDamageReduction = 0.8f; // Reduce 80% del daño
    [SerializeField] private float blockKnockbackReduction = 0.5f; // Reduce 50% del knockback
    [SerializeField] private float blockStunDuration = 0.15f; // Tiempo sin poder actuar después de bloquear
    [SerializeField] private float blockToAttackCooldown = 0.25f; // Tiempo sin poder atacar después de bloquear
    [SerializeField] private float blockPushbackToAttacker = 5f; // Fuerza de empuje al atacante cuando bloqueas
    [SerializeField] private float pushbackStunDuration = 0.1f; // Duración del stun cuando tu ataque es bloqueado
    private bool isBlocking = false;
    private bool isInBlockStun = false;
    private bool isInPushbackStun = false; // Cuando tu ataque fue bloqueado
    private bool canAttackAfterBlock = true;

    // Eventos para el sistema de combate
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged; // Pasa la salud actual
    public UnityEvent<float> OnDamageTaken; // Pasa el daño recibido
    public UnityEvent<float> OnStaminaChanged; // Pasa la stamina actual

    // Propiedades públicas para el sistema de salud
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsBlocking => isBlocking;
    public bool CanAttackAfterBlock => canAttackAfterBlock;

    // Propiedades públicas para stamina
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float AttackStaminaCost => attackStaminaCost;
    public bool HasEnoughStamina(float cost) => currentStamina >= cost;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float backwardSpeedMultiplier = 0.7f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fastFallForce = 15f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float airDashSpeed = 22f; // Dash aéreo más fuerte
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float doubleTapTime = 0.25f;

    [Header("Rebote en Cabeza")]
    [SerializeField] private float headBounceMultiplier = 0.5f; // 50% de la fuerza de salto

    [Header("Flip Estilo Paper Mario")]
    [SerializeField] private float flipDuration = 0.15f; // Duración de la animación de giro
    private bool isFlipping = false;

    // Estados del personaje
    public enum PlayerState { Idle, Walking, Jumping, Crouching, Dashing, Blocking }
    public PlayerState currentState = PlayerState.Idle;

    // Variables internas
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isCrouching;
    private bool isDashing;
    private bool canDash = true;
    private bool hasUsedAirDash = false; // Solo un air dash por salto

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
        inputHandler = GetComponent<PlayerInputHandler>();

        // Verificar si se usa el nuevo Input System
        if (useNewInputSystem && inputHandler == null)
        {
            Debug.LogWarning("Player: useNewInputSystem está activo pero no hay PlayerInputHandler. Usando input legacy.");
            useNewInputSystem = false;
        }

        // Inicializar eventos si son null
        OnDeath ??= new UnityEvent();
        OnHealthChanged ??= new UnityEvent<float>();
        OnDamageTaken ??= new UnityEvent<float>();
        OnStaminaChanged ??= new UnityEvent<float>();

        // Inicializar salud y stamina
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    void Update()
    {
        // No procesar input durante dash o blockstun
        if (isDashing || isInBlockStun) return;

        HandleInput();
        HandleBlockInput();
        HandleDoubleTapDash();
        UpdateState();
        UpdateAnimations();
        RegenerateStamina();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        ApplyMovement();
        ApplyBetterJump();
    }

    private void HandleInput()
    {
        if (useNewInputSystem && inputHandler != null)
        {
            HandleInputNewSystem();
        }
        else
        {
            HandleInputLegacy();
        }
    }

    /// <summary>
    /// Manejo de input usando el nuevo Input System
    /// </summary>
    private void HandleInputNewSystem()
    {
        // Input horizontal desde el InputHandler
        horizontalInput = inputHandler.HorizontalInput;

        // Fast Fall (solo en el aire con input hacia abajo)
        bool downInput = inputHandler.VerticalInput < -0.5f;
        
        if (!isGrounded)
        {
            if (downInput)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallForce);
            }
        }

        isCrouching = false;

        // Salto (solo si está en el suelo y no bloqueando)
        if (inputHandler.JumpPressed && isGrounded && !isBlocking)
        {
            Jump();
        }

        // Flip del sprite según dirección
        if (horizontalInput > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < -0.1f && isFacingRight)
        {
            Flip();
        }
    }

    /// <summary>
    /// Manejo de input usando el sistema legacy (Input.GetKey)
    /// </summary>
    private void HandleInputLegacy()
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

        // Fast Fall (solo en el aire con tecla abajo)
        bool downKey = playerNumber == 1 ? Input.GetKey(KeyCode.S) : Input.GetKey(KeyCode.DownArrow);
        
        if (!isGrounded)
        {
            // Fast Fall: aplicar fuerza hacia abajo cuando está en el aire
            if (downKey)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallForce);
            }
        }

        // Ya no hay agacharse, se reemplazó por bloqueo
        isCrouching = false;

        // Salto (solo si está en el suelo y no bloqueando)
        bool jumpKey = playerNumber == 1 
            ? Input.GetKeyDown(KeyCode.W)
            : Input.GetKeyDown(KeyCode.UpArrow);
        
        if (jumpKey && isGrounded && !isBlocking)
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

    private void HandleBlockInput()
    {
        // No puede bloquear si está en hitstun
        if (isInHitstun)
        {
            // Si estaba bloqueando y deja de hacerlo, iniciar cooldown
            if (isBlocking)
            {
                StartCoroutine(BlockToAttackCooldownCoroutine());
            }
            isBlocking = false;
            // DEBUG: Restaurar color cuando no bloquea (si no está en flash de daño)
            if (spriteRenderer != null && !isInDamageFlash) spriteRenderer.color = Color.white;
            return;
        }

        bool wasBlocking = isBlocking;

        // Determinar si está bloqueando según el sistema de input
        if (useNewInputSystem && inputHandler != null)
        {
            isBlocking = inputHandler.BlockHeld;
        }
        else
        {
            // Tecla de bloqueo: S (P1) o DownArrow (P2) - Sistema legacy
            KeyCode blockKey = playerNumber == 1 ? KeyCode.S : KeyCode.DownArrow;
            isBlocking = Input.GetKey(blockKey);
        }

        // Si dejó de bloquear, iniciar cooldown para atacar
        if (wasBlocking && !isBlocking)
        {
            StartCoroutine(BlockToAttackCooldownCoroutine());
        }

        // DEBUG: Cambiar color a amarillo cuando bloquea (si no está en flash de daño)
        if (spriteRenderer != null && !isInDamageFlash)
        {
            spriteRenderer.color = isBlocking ? Color.yellow : Color.white;
        }

        // No puede moverse horizontalmente mientras bloquea (pero puede caer normalmente)
        if (isBlocking)
        {
            horizontalInput = 0;
            
            // Drenar stamina mientras bloquea
            DrainStaminaWhileBlocking();
        }
    }

    /// <summary>
    /// Cooldown después de bloquear antes de poder atacar
    /// </summary>
    private System.Collections.IEnumerator BlockToAttackCooldownCoroutine()
    {
        canAttackAfterBlock = false;
        yield return new WaitForSeconds(blockToAttackCooldown);
        canAttackAfterBlock = true;
    }

    /// <summary>
    /// Drena stamina gradualmente mientras el jugador está bloqueando
    /// </summary>
    private void DrainStaminaWhileBlocking()
    {
        if (currentStamina > 0)
        {
            currentStamina -= blockingStaminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            staminaRegenTimer = staminaRegenDelay; // Resetear el delay de regeneración
            OnStaminaChanged?.Invoke(currentStamina);
        }
    }

    private void HandleDoubleTapDash()
    {
        // Puede hacer dash en el suelo o en el aire, pero no mientras bloquea
        if (isBlocking || !canDash) return;
        
        // En el aire, solo puede hacer un dash por salto
        if (!isGrounded && hasUsedAirDash) return;

        // Usar nuevo Input System si está activo
        if (useNewInputSystem && inputHandler != null)
        {
            if (inputHandler.DashRightTriggered)
            {
                StartCoroutine(PerformDash(1));
            }
            else if (inputHandler.DashLeftTriggered)
            {
                StartCoroutine(PerformDash(-1));
            }
            return;
        }

        // Sistema legacy
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
        // No aplicar movimiento durante hitstun, blockstun o pushback stun (para que funcione el knockback)
        if (isInHitstun || isInBlockStun || isInPushbackStun) return;

        // No moverse mientras ataca
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null && playerAttack.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
            bool holdingJump;
            
            if (useNewInputSystem && inputHandler != null)
            {
                holdingJump = inputHandler.JumpHeld;
            }
            else
            {
                holdingJump = playerNumber == 1 
                    ? Input.GetKey(KeyCode.W)
                    : Input.GetKey(KeyCode.UpArrow);
            }
            
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
        
        // Determinar velocidad de dash (más fuerte en el aire)
        float currentDashSpeed = isGrounded ? dashSpeed : airDashSpeed;
        
        // Marcar que usó el air dash si está en el aire
        if (!isGrounded)
        {
            hasUsedAirDash = true;
        }

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(direction * currentDashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Flip()
    {
        // No iniciar otro flip si ya está girando
        if (isFlipping) return;
        
        isFacingRight = !isFacingRight;
        StartCoroutine(PaperMarioFlipCoroutine());
    }

    /// <summary>
    /// Corrutina que anima el flip estilo Paper Mario (como una hoja de papel girando)
    /// </summary>
    private System.Collections.IEnumerator PaperMarioFlipCoroutine()
    {
        isFlipping = true;
        
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(-startScale.x, startScale.y, startScale.z);
        
        float elapsed = 0f;
        
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            
            // Usar curva sinusoidal para efecto más natural (como papel girando)
            // Va de 1 -> 0 -> -1 (o viceversa)
            float scaleX = Mathf.Lerp(startScale.x, endScale.x, t);
            
            transform.localScale = new Vector3(scaleX, startScale.y, startScale.z);
            
            yield return null;
        }
        
        // Asegurar escala final exacta
        transform.localScale = endScale;
        isFlipping = false;
    }

    private void UpdateState()
    {
        if (isDashing)
        {
            currentState = PlayerState.Dashing;
        }
        else if (isBlocking)
        {
            currentState = PlayerState.Blocking;
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
        animator.SetBool("IsBlocking", isBlocking);
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
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
        TakeDamage(damage, Vector2.zero, out _);
    }

    /// <summary>
    /// Aplica daño al jugador con knockback desde una dirección
    /// </summary>
    public void TakeDamage(float damage, Vector2 attackerPosition)
    {
        TakeDamage(damage, attackerPosition, out _);
    }

    /// <summary>
    /// Aplica daño al jugador con knockback desde una dirección.
    /// Devuelve si el jugador bloqueó el ataque.
    /// </summary>
    public void TakeDamage(float damage, Vector2 attackerPosition, out bool wasBlocked)
    {
        wasBlocked = false;
        if (IsDead) return;

        float finalDamage = damage;
        float finalKnockbackMultiplier = 1f;

        // Si está bloqueando, reducir daño y knockback
        if (isBlocking)
        {
            wasBlocked = true;
            finalDamage = damage * (1f - blockDamageReduction);
            finalKnockbackMultiplier = 1f - blockKnockbackReduction;
            
            // Gastar stamina al bloquear
            UseStamina(blockStaminaCost);
            
            // Iniciar block stun
            StartCoroutine(BlockStunCoroutine());
            
            // Mostrar texto "Bloqueado"
            StartCoroutine(ShowBlockTextCoroutine());
            
            Debug.Log($"{gameObject.name} bloqueó! Daño reducido: {damage} -> {finalDamage}");
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnDamageTaken?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(currentHealth);

        // Efecto visual de daño (naranja si bloqueó, rojo si no)
        StartCoroutine(DamageFlashCoroutine(wasBlocked));

        // Vibración del mando al recibir daño
        if (inputHandler != null)
        {
            if (wasBlocked)
                inputHandler.RumbleOnBlocked();
            else
                inputHandler.RumbleOnDamageTaken();
        }

        // Aplicar knockback si hay posición del atacante
        if (attackerPosition != Vector2.zero && rb != null)
        {
            ApplyKnockback(attackerPosition, finalKnockbackMultiplier);
        }

        if (!isBlocking)
        {
            Debug.Log($"{gameObject.name} recibió {finalDamage} de daño. Salud: {currentHealth}/{maxHealth}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Aplica fuerza de retroceso al jugador
    /// </summary>
    private void ApplyKnockback(Vector2 attackerPosition, float multiplier = 1f)
    {
        // Calcular dirección del knockback (alejándose del atacante)
        float knockbackDirection = transform.position.x > attackerPosition.x ? 1f : -1f;
        
        // Si está bloqueando, solo knockback horizontal (sin vertical)
        float verticalKnockback = isBlocking ? 0f : knockbackUpForce * multiplier;
        
        // Aplicar fuerza con multiplicador
        Vector2 knockback = new Vector2(
            knockbackDirection * knockbackForce * multiplier, 
            verticalKnockback
        );
        rb.linearVelocity = knockback;

        // Iniciar hitstun solo si no está bloqueando
        if (!isBlocking)
        {
            StartCoroutine(HitstunCoroutine());
        }
    }

    /// <summary>
    /// Corrutina de block stun (no puede actuar brevemente después de bloquear)
    /// </summary>
    private System.Collections.IEnumerator BlockStunCoroutine()
    {
        isInBlockStun = true;
        yield return new WaitForSeconds(blockStunDuration);
        isInBlockStun = false;
    }

    /// <summary>
    /// Corrutina que bloquea el movimiento brevemente después de recibir daño
    /// </summary>
    private System.Collections.IEnumerator HitstunCoroutine()
    {
        isInHitstun = true;
        yield return new WaitForSeconds(hitstunDuration);
        isInHitstun = false;
    }

    /// <summary>
    /// Corrutina que hace flash al recibir daño (naranja si bloqueó, rojo si no)
    /// </summary>
    private System.Collections.IEnumerator DamageFlashCoroutine(bool wasBlocked = false)
    {
        if (spriteRenderer != null)
        {
            isInDamageFlash = true;
            // Naranja si bloqueó, rojo si recibió daño completo
            spriteRenderer.color = wasBlocked ? new Color(1f, 0.5f, 0f) : Color.red;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = Color.white;
            isInDamageFlash = false;
        }
    }

    /// <summary>
    /// Corrutina que muestra el texto "Bloqueado" con temblor, subida y fade out
    /// </summary>
    private System.Collections.IEnumerator ShowBlockTextCoroutine()
    {
        if (blockTextPrefab == null) yield break;
        
        // Determinar posición de spawn
        Vector3 spawnPos = blockTextSpawnPoint != null 
            ? blockTextSpawnPoint.position 
            : transform.position + Vector3.up * 1.5f;
        
        // Instanciar el texto (no como hijo para que no se voltee con el personaje)
        GameObject textInstance = Instantiate(blockTextPrefab, spawnPos, Quaternion.identity);
        
        // Obtener componentes para el fade
        SpriteRenderer textSprite = textInstance.GetComponent<SpriteRenderer>();
        TMPro.TextMeshPro textTMP = textInstance.GetComponent<TMPro.TextMeshPro>();
        TMPro.TextMeshProUGUI textTMPUI = textInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        CanvasGroup canvasGroup = textInstance.GetComponent<CanvasGroup>();
        
        Vector3 startPos = textInstance.transform.position;
        float elapsed = 0f;
        
        while (elapsed < blockTextDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / blockTextDuration;
            
            // Subir gradualmente
            float riseOffset = elapsed * blockTextRiseSpeed;
            
            // Efecto de temblor
            float shakeX = Mathf.Sin(Time.time * blockTextShakeSpeed) * blockTextShakeIntensity * (1f - progress);
            float shakeY = Mathf.Cos(Time.time * blockTextShakeSpeed * 1.3f) * blockTextShakeIntensity * (1f - progress);
            
            textInstance.transform.position = startPos + new Vector3(shakeX, riseOffset + shakeY, 0);
            
            // Fade out (alpha de 1 a 0)
            float alpha = 1f - progress;
            
            if (textSprite != null)
            {
                Color c = textSprite.color;
                c.a = alpha;
                textSprite.color = c;
            }
            if (textTMP != null)
            {
                Color c = textTMP.color;
                c.a = alpha;
                textTMP.color = c;
            }
            if (textTMPUI != null)
            {
                Color c = textTMPUI.color;
                c.a = alpha;
                textTMPUI.color = c;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            
            yield return null;
        }
        
        // Destruir la instancia
        Destroy(textInstance);
    }

    /// <summary>
    /// Aplica knockback externo al jugador (usado cuando tu ataque es bloqueado)
    /// </summary>
    public void ApplyExternalKnockback(Vector2 direction, float force)
    {
        if (rb != null && !IsDead)
        {
            rb.linearVelocity = new Vector2(direction.x * force, rb.linearVelocity.y);
            StartCoroutine(PushbackStunCoroutine());
        }
    }

    /// <summary>
    /// Corrutina de pushback stun (breve pausa cuando tu ataque es bloqueado)
    /// </summary>
    private System.Collections.IEnumerator PushbackStunCoroutine()
    {
        isInPushbackStun = true;
        yield return new WaitForSeconds(pushbackStunDuration);
        isInPushbackStun = false;
    }

    /// <summary>
    /// Obtiene la fuerza de pushback que recibe el atacante al ser bloqueado
    /// </summary>
    public float GetBlockPushbackForce() => blockPushbackToAttacker;

    /// <summary>
    /// Usa stamina y notifica el cambio
    /// </summary>
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);
        staminaRegenTimer = staminaRegenDelay; // Resetear el delay de regeneración
        OnStaminaChanged?.Invoke(currentStamina);
    }

    /// <summary>
    /// Regenera stamina gradualmente
    /// </summary>
    private void RegenerateStamina()
    {
        if (currentStamina >= maxStamina) return;

        // Esperar el delay antes de regenerar
        if (staminaRegenTimer > 0)
        {
            staminaRegenTimer -= Time.deltaTime;
            return;
        }

        currentStamina += staminaRegenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina);
    }

    /// <summary>
    /// Resetea la stamina al máximo
    /// </summary>
    public void ResetStamina()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina);
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
        currentStamina = maxStamina;
        OnHealthChanged?.Invoke(currentHealth);
        OnStaminaChanged?.Invoke(currentStamina);
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

    /// <summary>
    /// Detecta colisión con otro jugador para el rebote en la cabeza
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            hasUsedAirDash = false;
            return;
        }

        // Verificar si colisionamos con otro jugador
        Player otherPlayer = collision.gameObject.GetComponent<Player>();
        if (otherPlayer == null) return;

        // Verificar si estamos cayendo y estamos por encima del otro jugador
        if (rb.linearVelocity.y < 0 && transform.position.y > otherPlayer.transform.position.y)
        {
            // Aplicar rebote (50% de la fuerza de salto)
            float bounceForce = jumpForce * headBounceMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            
            // Resetear air dash al rebotar
            hasUsedAirDash = false;
        }
    }

    /// <summary>
    /// Detecta cuando deja de tocar el suelo
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
