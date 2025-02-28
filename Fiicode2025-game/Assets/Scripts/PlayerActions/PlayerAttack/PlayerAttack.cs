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
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 0.5f;
    
    private float nextFireTime = 0f;
    private InputAction attackAction;

    void Start()
    {
        // Obtain the PlayerInput component from the PlayerController instance.
        if (PlayerController.instance != null)
        {
            var playerInput = PlayerController.instance.GetComponent<PlayerInput>();
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

    void Update()
    {
        // Do not allow attacking when in building mode.
        if (PlayerBuild.IsBuildingModeActive)
            return;

        // When the attack button is held, fire automatically if cooldown allows.
        if (attackAction != null && attackAction.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Attack();
        }
    }

    void Attack()
    {
        // Loop through each cannon origin.
        foreach (Transform cannonOrigin in cannonOrigins)
        {
            if (cannonOrigin != null)
            {
                // Instantiate the projectile at the cannon's position, with the player's gfx rotation.
                GameObject projectile = Instantiate(projectilePrefab, cannonOrigin.position, playerGfx.rotation);

                // Optionally, if your projectile script handles damage, you could set it here.
                // Example:
                // Projectile projScript = projectile.GetComponent<Projectile>();
                // if (projScript != null) { projScript.damage = damage; }

                // Launch the projectile using its Rigidbody.
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = playerGfx.forward * projectileSpeed;
                }
            }
        }
    }
}
