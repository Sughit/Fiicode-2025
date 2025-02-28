using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("Layer-ul cu obstacole care împiedică plasarea clădirii.")]
    public LayerMask placementObstacles;
    
    [Tooltip("Culoarea blueprint-ului când plasarea este validă.")]
    public Color validColor = Color.green;
    
    [Tooltip("Culoarea blueprint-ului când plasarea nu este validă.")]
    public Color invalidColor = Color.red;

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
    /// ignorând coliziunile cu propriile componente.
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

        // Verificăm fiecare coliziune: dacă găsim vreun collider care nu aparține blueprint-ului, plasarea este invalidă.
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
