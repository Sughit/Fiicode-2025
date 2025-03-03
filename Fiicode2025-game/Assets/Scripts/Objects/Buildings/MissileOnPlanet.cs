using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileOnPlanet : MonoBehaviour
{
    [Header("Planet Settings")]
    [Tooltip("Transform-ul centrului planetei (pivot).")]
    [SerializeField] private Transform planet;

    [Tooltip("LayerMask pentru planeta care are collider. Asigură-te că meshul planetei e pe layer-ul potrivit.")]
    [SerializeField] private LayerMask planetLayerMask;

    [Tooltip("Forța de atracție (gravitație) spre planetă.")]
    [SerializeField] private float gravityStrength = 10f;

    [Header("Movement Settings")]
    [Tooltip("Viteza de deplasare (m/s) pe suprafața planetei.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Viteza de rotație a missile-ului.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Tooltip("Transformul țintei urmărite (ex. un inamic).")]
    [SerializeField] private Transform target;

    [Tooltip("Timpul de viață (secunde) înainte să se autodistrugă.")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Optional - Obstacle Avoidance")]
    [Tooltip("Dacă vrei să eviți obstacole, setează un layerMask separat.")]
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float obstacleDetectionRadius = 1f;
    [SerializeField] private float obstacleDetectionDistance = 5f;
    [SerializeField] private float avoidStrength = 2f;
    [Tooltip("Pune pe true dacă vrei evitarea obstacolelor")]
    [SerializeField] private bool useAvoidance = false;

    private Rigidbody rb;
    private float currentLifetime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Gravitație custom
        currentLifetime = 0f;

        // Caută planeta după tag, dacă nu e setată manual
        if (!planet)
        {
            GameObject planetObj = GameObject.FindWithTag("Planet");
            if (planetObj != null) 
                planet = planetObj.transform;
        }
    }

    private void Update()
    {
        // Verificăm dacă a expirat durata de viață
        currentLifetime += Time.deltaTime;
        if (currentLifetime >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void FixedUpdate()
    {
        // 1) „Gravitație” locală și aliniere cu planeta
        ApplyCustomGravity();

        // 2) Homing spre țintă (dacă avem una), altfel avansează înainte
        if (target != null)
        {
            MoveTowardsTargetOnPlanet();
        }
        else
        {
            // Dacă nu avem țintă, decidem cum se comportă
            rb.MovePosition(rb.position + transform.forward * (moveSpeed * Time.fixedDeltaTime));
        }

        // 3) Ne asigurăm că missile-ul e PE meshul planetei (nu în el)
        SnapToPlanetSurface();
    }

    /// <summary>
    /// Trage missile-ul spre centrul planetei și aliniează 'up' cu normalul.
    /// (Similar cu PlayerMovementOnPlanet).
    /// </summary>
    private void ApplyCustomGravity()
    {
        if (!planet) return;

        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Aliniere 'up' => planet normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
    }

    /// <summary>
    /// Se deplasează pe planetă către țintă.  
    /// 1) Calculează direcția tangentă.  
    /// 2) (Opțional) face evitarea obstacolelor.  
    /// 3) Se rotește și avansează.
    /// </summary>
    private void MoveTowardsTargetOnPlanet()
    {
        Vector3 toTarget = target.position - transform.position;
        Vector3 planetUp = transform.up;

        // Proiectăm direcția spre planul tangent la planetă
        Vector3 projected = Vector3.ProjectOnPlane(toTarget, planetUp);
        if (projected.sqrMagnitude < 0.001f) return;

        // Direcția de homing (pe suprafață)
        Vector3 homingDir = projected.normalized;

        // Adăugăm, dacă e cazul, evitarea obstacolelor
        Vector3 avoidanceDir = useAvoidance ? GetAvoidanceDirection(homingDir) : Vector3.zero;
        Vector3 finalDir = (homingDir + avoidanceDir).normalized;

        // Rotim missile-ul
        Quaternion targetRot = Quaternion.LookRotation(finalDir, planetUp);
        Quaternion finalRot = Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
        rb.MoveRotation(finalRot);

        // Deplasează-l înainte
        rb.MovePosition(rb.position + finalRot * Vector3.forward * (moveSpeed * Time.fixedDeltaTime));
    }

    /// <summary>
    /// Raycast downward (opus centrului planetei) până la mesh-ul planetei,
    /// apoi mută missile-ul fix pe suprafață, ca să nu pătrundă în ea.
    /// </summary>
    private void SnapToPlanetSurface()
    {
        if (!planet) return;

        // Direcția „în jos” = opusă vectorului (planet->missile)
        Vector3 fromMissileToPlanet = (planet.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + (-fromMissileToPlanet * 2f); // un mic offset deasupra solului
        float rayLength = 10f; // Ajustează cât să fie mai mare decât orice alt offset

        // Lansăm ray-ul
        if (Physics.Raycast(rayOrigin, fromMissileToPlanet, out RaycastHit hit, rayLength, planetLayerMask))
        {
            // Așezăm missile-ul exact la punctul de contact
            transform.position = hit.point;
        }
        else
        {
            // Dacă nu detectăm deloc terenul, e posibil să fim departe => fallback
            // ex. transform.position = planet.position + fromMissileToPlanet * (planetRadius + ceva);
        }
    }

    /// <summary>
    /// Face un SphereCast în direcția de homing, pentru a detecta obstacole.
    /// </summary>
    private Vector3 GetAvoidanceDirection(Vector3 forwardDir)
    {
        RaycastHit hit;
        if (Physics.SphereCast(
            origin: transform.position,
            radius: obstacleDetectionRadius,
            direction: forwardDir,
            out hit,
            obstacleDetectionDistance,
            obstacleLayerMask))
        {
            // Normal la obstacol
            Vector3 obstacleNormal = hit.normal;
            // Vector de evitare (Cross)
            Vector3 avoidDir = Vector3.Cross(obstacleNormal, Vector3.up).normalized;
            return avoidDir * avoidStrength;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Setează ținta dinamic.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Distrugem missile-ul la impact
        Destroy(gameObject);
    }
}
