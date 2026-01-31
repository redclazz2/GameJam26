using System;
using UnityEngine;

public enum FacingDirection
{
    Right,
    Left
}

public class PlayerAttack : MonoBehaviour
{
    private Transform playerTransform;

    [SerializeField]
    public const float meleeAttackCooldownDefault = 1;

    [SerializeField]
    public FacingDirection facingDirection = FacingDirection.Right;

    [SerializeField]
    public float meleeAttackCooldown = 0f;

    [SerializeField]
    public GameObject meleeAttack;

    void Start()
    {
        if (meleeAttack == null)
        {
            throw new ArgumentNullException("Attack GameObject is Empty! Configure from inspector!");
        }

        playerTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (meleeAttackCooldown > 0 && Input.GetKey(KeyCode.X))
        {
            var attackSpawnDirectionOffSet =
                facingDirection == FacingDirection.Right ? 10 : -10;
            var attackSpawnPosition = new Vector3(
                playerTransform.position.x + attackSpawnDirectionOffSet,
                playerTransform.position.y,
                playerTransform.position.z
            );
            Instantiate(meleeAttack, attackSpawnPosition, playerTransform.rotation);

            meleeAttackCooldown = meleeAttackCooldownDefault;
        }
        else
        {
            meleeAttackCooldown -= 0.1f;
        }
    }
}
