using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("Layer-ul cu obstacole care împiedică plasarea clădirii.")]
    [SerializeField] private LayerMask placementObstacles;
    
    [Tooltip("Culoarea blueprint-ului când plasarea este validă.")]
    [SerializeField] private Color validColor = Color.green;
    
    [Tooltip("Culoarea blueprint-ului când plasarea nu este validă.")]
    [SerializeField] private Color invalidColor = Color.red;

    [Tooltip("Culorile permise pentru plasare. Dacă este gol, se poate plasa pe orice culoare.")]
    [SerializeField] private Color[] allowedColors;

    [Tooltip("Toleranța pentru compararea culorilor (doar r, g, b).")]
    [SerializeField] private float colorThreshold = 0.3f;

    // Proprietate publică pentru a afla dacă plasarea este validă.
    public bool CanPlace { get; private set; } = true;

    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        // Obținem toate componentele Renderer și Collider din blueprint (și copii).
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        CheckPlacement();
        UpdateColor();
    }

    /// <summary>
    /// Verifică dacă zona definită de toate collider-ele din blueprint este liberă de obstacole,
    /// ignorând coliziunile cu propriile componente, și dacă suprafața de plasare are o culoare permisă.
    /// Se folosește aceeași metodă de determinare a culorii ca în Spawner.cs, cu un threshold pentru toleranță.
    /// </summary>
    void CheckPlacement()
    {
        if (colliders.Length == 0)
        {
            CanPlace = true;
            return;
        }

        // Calculăm un Bounds combinat din toate collider-ele.
        Bounds combinedBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            combinedBounds.Encapsulate(colliders[i].bounds);
        }

        // Verificăm coliziunile folosind OverlapBox.
        Collider[] hits = Physics.OverlapBox(combinedBounds.center, combinedBounds.extents, transform.rotation, placementObstacles);
        bool valid = true;
        foreach (Collider hit in hits)
        {
            bool isOwnCollider = false;
            foreach (Collider own in colliders)
            {
                if (hit == own)
                {
                    isOwnCollider = true;
                    break;
                }
            }
            if (!isOwnCollider)
            {
                valid = false;
                break;
            }
        }

        // Dacă plasarea este încă validă și se specifică culori permise, verificăm culoarea suprafeței.
        if (valid && allowedColors.Length > 0)
        {
            // Lansăm ray-ul de la o poziție puțin deasupra centrului bounds-ului, în jos.
            Ray ray = new Ray(combinedBounds.center + Vector3.up * 1f, Vector3.down);
            RaycastHit[] rayHits = Physics.RaycastAll(ray, 5f);
            RaycastHit selectedHit = new RaycastHit();
            bool foundValidHit = false;

            // Căutăm primul hit care nu aparține blueprint-ului (sau copiilor lui)
            foreach (RaycastHit h in rayHits)
            {
                bool isOwnCollider = false;
                foreach (Collider own in colliders)
                {
                    if (h.collider == own)
                    {
                        isOwnCollider = true;
                        break;
                    }
                }
                if (!isOwnCollider)
                {
                    selectedHit = h;
                    foundValidHit = true;
                    break;
                }
            }

            if (foundValidHit)
            {
                Color surfaceColor = Color.black;
                Renderer hitRenderer = selectedHit.collider.GetComponent<Renderer>();
                if (hitRenderer != null && hitRenderer.sharedMaterial != null)
                {
                    Material planetMat = hitRenderer.sharedMaterial;
                    // Obținem proprietățile din material (asigură-te că shaderul folosește aceste nume)
                    float planetRadius = planetMat.GetFloat("_PlanetRadius");
                    float minHeight = planetMat.GetFloat("_MinHeight");
                    float maxHeight = planetMat.GetFloat("_MaxHeight");
                    Texture2D gradientTexture = planetMat.GetTexture("_GradientTex") as Texture2D;

                    if (gradientTexture != null)
                    {
                        // Calculăm înălțimea similar cu Spawner.cs:
                        float height = selectedHit.point.magnitude - planetRadius;
                        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
                        t = Mathf.Clamp01(t);
                        // Obținem culoarea din textura gradient
                        surfaceColor = gradientTexture.GetPixelBilinear(t, 0.5f);
                    }
                    else
                    {
                        // Fallback: folosim culoarea materialului
                        surfaceColor = planetMat.HasProperty("_BaseColor") ? planetMat.GetColor("_BaseColor") : planetMat.color;
                    }
                }

                // Comparăm culoarea obținută cu cele din allowedColors, ignorând canalul alfa.
                bool colorAllowed = false;
                foreach (Color allowed in allowedColors)
                {
                    if (Mathf.Abs(surfaceColor.r - allowed.r) < colorThreshold &&
                        Mathf.Abs(surfaceColor.g - allowed.g) < colorThreshold &&
                        Mathf.Abs(surfaceColor.b - allowed.b) < colorThreshold)
                    {
                        colorAllowed = true;
                        break;
                    }
                }
                if (!colorAllowed)
                {
                    valid = false;
                }
            }
            else
            {
                valid = false;
            }
        }

        CanPlace = valid;
    }

    /// <summary>
    /// Actualizează culoarea tuturor materialelor din blueprint în funcție de validitatea plasării.
    /// </summary>
    void UpdateColor()
    {
        Color targetColor = CanPlace ? validColor : invalidColor;
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = targetColor;
            }
        }
    }
}
