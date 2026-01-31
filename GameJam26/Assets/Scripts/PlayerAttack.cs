using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float meleeAttackCooldownDefault = 0.05f;
    [SerializeField] private GameObject meleeAttack;

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

        if (meleeAttackCooldown <= 0f && Input.GetKeyDown(keyMeleeToCheck))
        {
            float offset = playerComponent.FacingDirection == 1 ? 3f : -3f;

            Vector3 spawnPosition = new Vector3(
                playerTransform.position.x + offset,
                playerTransform.position.y,
                playerTransform.position.z
            );

            GameObject attackGO = Instantiate(meleeAttack, spawnPosition, Quaternion.identity);

            Attack attack = attackGO.GetComponent<Attack>();
            attack.damage = playerComponent.GetDamage();
            attack.owner = gameObject;

            meleeAttackCooldown = meleeAttackCooldownDefault;
        }
    }

}
