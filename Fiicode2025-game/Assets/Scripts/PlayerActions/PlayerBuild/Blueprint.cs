using UnityEngine;
using UnityEngine.UI; // dacă folosești UI clasic. (Pentru Text, de exemplu)
// Sau, dacă folosești TextMeshPro, înlocuiește cu: using TMPro;

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

    // --- Restricții suplimentare pe bază de prefab-uri ---
    [Header("Extra Placement Restrictions (Prefab-based)")]
    [Tooltip("Prefab-urile față de care trebuie să fim aproape (la o distanță maximă).")]
    [SerializeField] private GameObject[] requiredPrefabs;
    [SerializeField] private float requiredPrefabsMaxDistance = 3f;

    [Tooltip("Prefab-urile de care trebuie să fim departe (la o distanță minimă).")]
    [SerializeField] private GameObject[] prohibitedPrefabs;
    [SerializeField] private float prohibitedPrefabsMinDistance = 3f;

    // --- Restricții suplimentare pe bază de tag-uri ---
    [Header("Extra Placement Restrictions (Tag-based)")]
    [Tooltip("Tag-urile obiectelor de care trebuie să fim aproape (la o distanță maximă).")]
    [SerializeField] private string[] requiredTags;
    [SerializeField] private float requiredTagsMaxDistance = 3f;

    [Tooltip("Tag-urile obiectelor de care trebuie să fim departe (la o distanță minimă).")]
    [SerializeField] private string[] prohibitedTags;
    [SerializeField] private float prohibitedTagsMinDistance = 3f;

    [Header("UI Feedback (Opțional)")]
    [Tooltip("Canvas care conține textul de eroare, dacă e nevoie să afișezi motivul invalidării.")]
    [SerializeField] private Canvas canvas;

    [Tooltip("Text UI (sau TextMeshPro) unde afișăm motivul invalidării.")]
    [SerializeField] private Text errorText;
    // Dacă folosești TextMeshPro, pune `TMP_Text errorText;` și folosește `errorText.text = ...`

    // Proprietate publică pentru a afla dacă plasarea este validă.
    public bool CanPlace { get; private set; } = true;

    // Stocăm motivul invalidării, dacă există
    public string InvalidReason { get; private set; } = "";

    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        // Obținem toate componentele Renderer și Collider din blueprint (și copii).
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        // Asigurăm Canvasul să fie World Space și să aibă camera principală.
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            // Ascundem canvasul la start (poți comenta linia dacă vrei să fie mereu vizibil).
            canvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        CheckPlacement();
        UpdateColor();

        // Dacă avem un text de eroare în inspector, îl actualizăm:
        if (errorText != null && canvas != null)
        {
            if (CanPlace)
            {
                errorText.text = ""; 
                canvas.gameObject.SetActive(false);
            }
            else
            {
                errorText.text = InvalidReason;
                canvas.gameObject.SetActive(true);
            }
        }

        // Asigurăm că Canvas-ul rămâne mereu paralel cu ecranul (billboard).
        if (canvas != null && Camera.main != null)
        {
            canvas.transform.forward = Camera.main.transform.forward;
        }
    }

    /// <summary>
    /// Verifică dacă zona definită de toate collider-ele din blueprint este liberă de obstacole,
    /// ignorând coliziunile cu propriile componente, și dacă suprafața de plasare are o culoare permisă.
    /// De asemenea, verifică noile restricții (pe bază de prefab-uri și tag-uri).
    /// </summary>
    void CheckPlacement()
    {
        // Implicit, considerăm că e valid și nu avem niciun motiv de invalidare
        bool valid = true;
        InvalidReason = "";

        if (colliders.Length == 0)
        {
            // Dacă nu avem collidere, considerăm valid (sau poți considera invalid)
            CanPlace = true;
            return;
        }

        // 1) Verificăm coliziunile folosind OverlapBox
        Bounds combinedBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            combinedBounds.Encapsulate(colliders[i].bounds);
        }

        Collider[] hits = Physics.OverlapBox(combinedBounds.center, combinedBounds.extents, transform.rotation, placementObstacles);
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
                InvalidReason = "Se suprapune cu un obstacol!";
                break;
            }
        }

        // 2) Dacă încă e valid și avem culori permise, verificăm culoarea suprafeței
        if (valid && allowedColors.Length > 0)
        {
            Ray ray = new Ray(combinedBounds.center + Vector3.up * 1f, Vector3.down);
            RaycastHit[] rayHits = Physics.RaycastAll(ray, 5f);
            RaycastHit selectedHit = new RaycastHit();
            bool foundValidHit = false;

            // Căutăm primul hit care nu aparține blueprint-ului
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
                        float height = selectedHit.point.magnitude - planetRadius;
                        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
                        t = Mathf.Clamp01(t);
                        surfaceColor = gradientTexture.GetPixelBilinear(t, 0.5f);
                    }
                    else
                    {
                        if (planetMat.HasProperty("_BaseColor"))
                            surfaceColor = planetMat.GetColor("_BaseColor");
                        else
                            surfaceColor = planetMat.color;
                    }
                }

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
                    InvalidReason = "Culoarea suprafeței nu este permisă!";
                }
            }
            else
            {
                valid = false;
                InvalidReason = "Nu s-a găsit o suprafață validă sub blueprint!";
            }
        }

        // 3) Verificăm requiredPrefabs (trebuie să fie aproape de minim unul)
        if (valid && requiredPrefabs != null && requiredPrefabs.Length > 0)
        {
            bool foundAnyClose = false;
            foreach (GameObject req in requiredPrefabs)
            {
                if (req == null) continue; 
                float dist = Vector3.Distance(combinedBounds.center, req.transform.position);
                if (dist <= requiredPrefabsMaxDistance)
                {
                    foundAnyClose = true;
                    break;
                }
            }
            if (!foundAnyClose)
            {
                valid = false;
                InvalidReason = $"Nu există un obiect necesar în raza de {requiredPrefabsMaxDistance}m!";
            }
        }

        // 4) Verificăm prohibitedPrefabs (trebuie să fim la distanță mai mare)
        if (valid && prohibitedPrefabs != null && prohibitedPrefabs.Length > 0)
        {
            foreach (GameObject prohib in prohibitedPrefabs)
            {
                if (prohib == null) continue;
                float dist = Vector3.Distance(combinedBounds.center, prohib.transform.position);
                if (dist < prohibitedPrefabsMinDistance)
                {
                    valid = false;
                    InvalidReason = $"Prea aproape de un obiect interzis (distanță minimă: {prohibitedPrefabsMinDistance}m)!";
                    break;
                }
            }
        }

        // 5) Verificăm requiredTags (trebuie să fim aproape de cel puțin un obiect care are unul din tag-urile necesare)
        if (valid && requiredTags != null && requiredTags.Length > 0)
        {
            bool foundAnyCloseTag = false;
            foreach (string reqTag in requiredTags)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(reqTag);
                foreach (GameObject obj in objs)
                {
                    float dist = Vector3.Distance(combinedBounds.center, obj.transform.position);
                    if (dist <= requiredTagsMaxDistance)
                    {
                        foundAnyCloseTag = true;
                        break;
                    }
                }
                if (foundAnyCloseTag)
                    break;
            }
            if (!foundAnyCloseTag)
            {
                valid = false;
                InvalidReason = $"Nu există un obiect cu tag necesar în raza de {requiredTagsMaxDistance}m!";
            }
        }

        // 6) Verificăm prohibitedTags (trebuie să fim la distanță mai mare de toate obiectele cu aceste tag-uri)
        if (valid && prohibitedTags != null && prohibitedTags.Length > 0)
        {
            foreach (string prohibTag in prohibitedTags)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(prohibTag);
                foreach (GameObject obj in objs)
                {
                    float dist = Vector3.Distance(combinedBounds.center, obj.transform.position);
                    if (dist < prohibitedTagsMinDistance)
                    {
                        valid = false;
                        InvalidReason = $"Prea aproape de un obiect cu tag '{prohibTag}' (distanță minimă: {prohibitedTagsMinDistance}m)!";
                        break;
                    }
                }
                if (!valid)
                    break;
            }
        }

        // Final: setăm flag-ul CanPlace
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
