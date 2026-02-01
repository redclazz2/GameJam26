using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float meleeAttackCooldownDefault = 0.05f;
    [SerializeField] private float attackDuration = 0.2f; // Duración del estado "atacando"
    [SerializeField] private GameObject meleeAttack;
    [SerializeField] private GameObject longAttack;
    [SerializeField] private GameObject upAttack;

    [Header("Advanced Attack Settings")]
    [SerializeField] private float doubleTapAttackTime = 0.25f;
    [SerializeField] private float upAttackHorizontalOffSet = 2f;

    [Header("Input System")]
    [SerializeField] private bool useNewInputSystem = true;

    private float lastAttackTapTime;
    private bool waitingForSecondTap;
    private bool isAttacking = false;

    // Propiedad pública para que Player sepa si está atacando
    public bool IsAttacking => isAttacking;


    private float meleeAttackCooldown;
    private Transform playerTransform;
    private Player playerComponent;
    private PlayerInputHandler inputHandler;
    private Animator animator;
    private KeyCode keyMeleeToCheck;
    void Start()
    {
        if (meleeAttack == null)
        {
            Debug.LogError("Attack GameObject is Empty! Configure from inspector!");
            enabled = false;
            return;
        }

        playerComponent = gameObject.GetComponent<Player>();
        inputHandler = gameObject.GetComponent<PlayerInputHandler>();
        animator = gameObject.GetComponent<Animator>();

        // Verificar si se usa el nuevo Input System
        if (useNewInputSystem && inputHandler == null)
        {
            Debug.LogWarning("PlayerAttack: useNewInputSystem está activo pero no hay PlayerInputHandler. Usando input legacy.");
            useNewInputSystem = false;
        }

        playerTransform = transform;
        meleeAttackCooldown = 0f;
        keyMeleeToCheck = playerComponent.PlayerNumber == 1 ? KeyCode.E : KeyCode.M;
    }

    void Update()
    {
        if (meleeAttackCooldown > 0f)
            meleeAttackCooldown -= Time.deltaTime;

        if (meleeAttackCooldown > 0f)
            return;

        // No puede atacar mientras bloquea o en cooldown después de bloquear
        if (playerComponent.IsBlocking || !playerComponent.CanAttackAfterBlock)
            return;

        // No puede atacar si no tiene suficiente stamina
        if (!playerComponent.HasEnoughStamina(playerComponent.AttackStaminaCost))
            return;

        // Determinar si se presionó el botón de ataque según el sistema de input
        bool attackPressed;
        if (useNewInputSystem && inputHandler != null)
        {
            attackPressed = inputHandler.AttackPressed;
        }
        else
        {
            attackPressed = Input.GetKeyDown(keyMeleeToCheck);
        }

        if (!attackPressed)
            return;

        // ⬆️ UP ATTACK (priority)
        if (!playerComponent.IsGrounded())
        {
            SpawnUpAttack();
            return;
        }
        // PRIMER TAP
        if (!waitingForSecondTap)
        {
            waitingForSecondTap = true;
            lastAttackTapTime = Time.time;
            return;
        }

        // SEGUNDO TAP
        if (Time.time - lastAttackTapTime <= doubleTapAttackTime)
        {
            waitingForSecondTap = false;
            SpawnLongAttack();
        }
        if (waitingForSecondTap && Time.time - lastAttackTapTime > doubleTapAttackTime)
        {
            waitingForSecondTap = false;
            SpawnMeleeAttack();
        }
    }

    private void ResolveSingleTap()
    {
        if (!waitingForSecondTap)
            return;

        SpawnMeleeAttack();
        waitingForSecondTap = false;
    }

    private void SpawnMeleeAttack()
    {
        if (animator != null) animator.SetTrigger("MeleeAttack");
        SpawnAttack(meleeAttack, 2f);
    }
    private void SpawnLongAttack()
    {
        if (animator != null) animator.SetTrigger("LongAttack");
        SpawnAttack(longAttack, 3f);
    }
    private void SpawnUpAttack()
    {
        if (animator != null) animator.SetTrigger("UpAttack");
        
        float direction = playerComponent.FacingDirection;
        Vector3 spawnPosition = new Vector3(
            playerTransform.position.x + upAttackHorizontalOffSet * direction,
            playerTransform.position.y + 3f,
            playerTransform.position.z
        );

        GameObject attackGO = Instantiate(upAttack, spawnPosition, Quaternion.identity);
        SetupAttack(attackGO);

        meleeAttackCooldown = meleeAttackCooldownDefault;
    }
    private void SpawnAttack(GameObject attackPrefab, float horizontalOffset)
    {
        float direction = playerComponent.FacingDirection;

        Vector3 spawnPosition = new Vector3(
            playerTransform.position.x + horizontalOffset * direction,
            playerTransform.position.y,
            playerTransform.position.z
        );

        GameObject attackGO = Instantiate(attackPrefab, spawnPosition, Quaternion.identity);
        SetupAttack(attackGO);

        meleeAttackCooldown = meleeAttackCooldownDefault;
    }
    private void SetupAttack(GameObject attackGO)
    {
        Attack attack = attackGO.GetComponent<Attack>();
        attack.damage = playerComponent.GetDamage();
        attack.owner = gameObject;

        // Gastar stamina al atacar
        playerComponent.UseStamina(playerComponent.AttackStaminaCost);

        // Iniciar estado de ataque
        StartCoroutine(AttackingCoroutine());
    }

    private System.Collections.IEnumerator AttackingCoroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }
}

