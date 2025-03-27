using UnityEngine;

public class NewPlayerMovement : MonoBehaviour
{
    [Header("Referințe")]
    [SerializeField] private Transform planet;               // Containerul planetei (conține cele 6 copii)
    [SerializeField] private Transform gfx;                  // Partea vizuală a jucătorului
    [SerializeField] private Transform cameraTransform;      // Camera folosită pentru raycast (sau Camera.main)

    [Header("Setări Mișcare")]
    [SerializeField] private float gravityStrength = 10f;      // Intensitatea gravitației
    [SerializeField] private float moveSpeed = 5f;             // Viteza maximă de deplasare (măsurată pe arcul sferic)
    [SerializeField] private float rotationSpeed = 10f;        // Viteza cu care gfx se rotește spre direcția de plecare
    [SerializeField] private float stoppingDistance = 0.1f;    // Distanța la care se consideră că a ajuns la țintă

    [Header("Setări Penetrare")]
    [SerializeField] private float planetRadius = 10f;         // Raza planetei
    [SerializeField] private float penetrationMargin = 0.5f;   // Marja de siguranță pentru a preveni penetrarea

    private Rigidbody rb;

    // Variabile pentru mișcarea interpolată
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPoint;
    private Vector3 startDir;         // Direcția radială la momentul de start (de la centru către poziția jucătorului)
    private Vector3 targetDir;        // Direcția radială către țintă (calculată din targetPoint)
    private float moveTimer = 0f;
    private float moveDuration = 0f;
    private float currentRadius = 0f; // Distanța jucătorului față de centrul planetei (la momentul start)
    private Vector3 departureDirection; // Direcția de plecare (tangentă la suprafața la momentul click-ului)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // Verificăm dacă obiectul lovit face parte din ierarhia planetei
                    if (hit.transform.IsChildOf(planet))
                    {
                        targetPoint = hit.point;
                        BeginMovement();
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            MoveAlongSurface();
        }

        ApplyGravityAndRotation();
        PreventPenetration();
    }

    // Inițiază o mișcare către punctul selectat pe planetă
    void BeginMovement()
    {
        startPosition = rb.position;
        currentRadius = (startPosition - planet.position).magnitude;
        startDir = (startPosition - planet.position).normalized;
        targetDir = (targetPoint - planet.position).normalized;

        // Calculăm unghiul (în radiani) și arcul sferic ce trebuie parcurs
        float angle = Vector3.Angle(startDir, targetDir) * Mathf.Deg2Rad;
        float arcLength = angle * currentRadius;
        moveDuration = arcLength / moveSpeed;
        moveTimer = 0f;
        isMoving = true;

        // Calculăm direcția tangentă de plecare: proiectăm vectorul de la start către target pe planul tangent la start
        Vector3 rawDirection = targetPoint - startPosition;
        departureDirection = Vector3.ProjectOnPlane(rawDirection, startDir).normalized;
    }

    // Mișcarea efectivă pe suprafața planetei cu ease-in-out
    void MoveAlongSurface()
    {
        moveTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(moveTimer / moveDuration);
        // Funcție de easing (ease-in-out) pentru o tranziție lină
        float tSmooth = t * t * (3f - 2f * t);

        // Interpolare sferică: se trece treptat de la direcția de start la cea țintă
        Vector3 newDir = Vector3.Slerp(startDir, targetDir, tSmooth);
        Vector3 newPosition = planet.position + newDir * currentRadius;
        rb.MovePosition(newPosition);

        // Rotim gfx spre direcția de plecare (constantă, calculată la momentul inițierii mișcării)
        if (gfx != null)
        {
            // "Up" la noua poziție este direcția radială (newDir)
            Quaternion targetGfxRotation = Quaternion.LookRotation(departureDirection, newDir);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, targetGfxRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        // Dacă am ajuns la destinație (sau aproape), oprim mișcarea
        if (t >= 1f || Vector3.Distance(newPosition, targetPoint) < stoppingDistance)
        {
            isMoving = false;
        }
    }

    // Aplică gravitația și aliniază jucătorul astfel încât să fie atașat la planetă
    void ApplyGravityAndRotation()
    {
        Vector3 gravityDirection = (planet.position - rb.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    // Previne ca jucătorul să treacă prin planetă
    void PreventPenetration()
    {
        Vector3 fromCenter = transform.position - planet.position;
        float currentDistance = fromCenter.magnitude;
        float desiredDistance = planetRadius + penetrationMargin;
        if (currentDistance < desiredDistance)
        {
            Vector3 correction = fromCenter.normalized * (desiredDistance - currentDistance);
            rb.MovePosition(rb.position + correction);
        }
    }
}
