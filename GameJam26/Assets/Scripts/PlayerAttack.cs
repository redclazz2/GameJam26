using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Transform playerTransform;
    private Player playerComponent;

    [Header("Attack Settings")]
    [SerializeField] private float meleeAttackCooldownDefault = 1f;
    [SerializeField] private GameObject meleeAttack;

    [Header("State")]
    [SerializeField] private FacingDirection facingDirection = FacingDirection.Right;
    private float meleeAttackCooldown;

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
        // cooldown ticks every frame
        if (meleeAttackCooldown > 0f)
        {
            meleeAttackCooldown -= Time.deltaTime;
        }

        // attack only on key press
        if (meleeAttackCooldown <= 0f && Input.GetKeyDown(KeyCode.X))
        {


            float offset = playerComponent.FacingDirection == 1 ? 1f : -1f;
                        Debug.Log(playerComponent.FacingDirection);
            Vector3 spawnPosition = new Vector3(
                playerTransform.position.x + offset,
                playerTransform.position.y,
                playerTransform.position.z
            );

            Instantiate(meleeAttack, spawnPosition, Quaternion.identity);

            meleeAttackCooldown = meleeAttackCooldownDefault;
        }
    }
}
