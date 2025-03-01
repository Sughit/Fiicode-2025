using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Cannons")]
    [SerializeField] private Transform[] cannonOrigins;

    [Header("Player GFX")]
    [SerializeField] private Transform playerGfx;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    //[SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 0.5f;

    private float nextFireTime;
    private InputAction attackAction;

    void Start()
    {
        if (PlayerController.instance != null)
        {
            var playerInput = PlayerController.instance.GetComponent<PlayerInput>();
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

    void Update()
    {
        if (PlayerBuild.IsBuildingModeActive ||
            PlayerBuild.JustPlacedBuilding ||
            PlayerController.instance.interactionCam.gameObject.activeSelf)
        {
            return;
        }

        // Fire automatically if button is held down and cooldown allows
        if (attackAction != null && attackAction.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Attack();
        }
    }

    void Attack()
    {
        foreach (Transform cannonOrigin in cannonOrigins)
        {
            if (!cannonOrigin) continue;

            // Instantiate the projectile at the cannon's position
            GameObject projectile = Instantiate(projectilePrefab, cannonOrigin.position, playerGfx.rotation);

            // Option 1: Directly set velocity if the projectile has a Rigidbody
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = playerGfx.forward * projectileSpeed;
            }

            // Option 2: or call a method on a Projectile script to handle velocity & damage
        }
    }
}
