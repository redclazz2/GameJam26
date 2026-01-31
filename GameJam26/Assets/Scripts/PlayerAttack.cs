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

    private float lastAttackTapTime;
    private bool waitingForSecondTap;
    private bool isAttacking = false;

    // Propiedad pública para que Player sepa si está atacando
    public bool IsAttacking => isAttacking;


    private float meleeAttackCooldown;
    private Transform playerTransform;
    private Player playerComponent;
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

        if (!Input.GetKeyDown(keyMeleeToCheck))
            return;

        // ⬆️ UP ATTACK (priority)
        if (!playerComponent.IsGrounded())
        {
            SpawnUpAttack();
            return;
        }

        // 💥 DOUBLE TAP
        if (Time.time - lastAttackTapTime <= doubleTapAttackTime)
        {
            SpawnLongAttack();
            lastAttackTapTime = 0f;
            return;
        }

        // 💥 INSTANT BASIC ATTACK
        SpawnMeleeAttack();
        lastAttackTapTime = Time.time;
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
        SpawnAttack(meleeAttack, 2f);
    }
    private void SpawnLongAttack()
    {
        SpawnAttack(longAttack, 3f);
    }
    private void SpawnUpAttack()
    {
        Vector3 spawnPosition = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y + 2f,
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

