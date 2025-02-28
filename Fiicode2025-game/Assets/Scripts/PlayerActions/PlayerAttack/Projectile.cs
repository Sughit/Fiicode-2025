using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Planet Settings")]
    // Reference to the planet the projectile should move around.
    [SerializeField] private Transform planet;
    // How strong the planet's gravity is on the projectile.
    [SerializeField] private float gravityStrength = 10f;
    // How fast the projectile rotates to align with the planet's surface.
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Disable Unity's default gravity so we can use our custom gravity.
        rb.useGravity = false;

        planet = GameObject.FindWithTag("Planet").transform;

        Destroy(this.gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // Calculate the direction toward the planet.
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        
        // Apply gravitational force toward the planet.
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Determine the rotation needed so the projectile's up points away from the planet.
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }
}
