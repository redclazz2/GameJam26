using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float meleeAttackCooldownDefault = 0.2f;
    [SerializeField] private GameObject meleeAttack;

    private float meleeAttackCooldown;
    private Transform playerTransform;
    private Player playerComponent;
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
    }

    void Update()
    {
        if (meleeAttackCooldown > 0f)
            meleeAttackCooldown -= Time.deltaTime;

        if (meleeAttackCooldown <= 0f && Input.GetKeyDown(KeyCode.X))
        {
            float offset = playerComponent.FacingDirection == 1 ? 1f : -1f;

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
