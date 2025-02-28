using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [Header("Prefabs & References")]
    [Tooltip("Prefab-ul clădirii ce va fi plasată efectiv.")]
    public GameObject buildingPrefab;
    
    [Tooltip("Prefab-ul blueprint-ului (previzualizare).")]
    public GameObject blueprintPrefab;
    
    [Tooltip("Referință la jucător (folosit pentru calculul distanței).")]
    public Transform player;

    [Header("Settings")]
    [Tooltip("Distanța maximă permisă față de jucător pentru plasare.")]
    public float maxPlacementDistance = 10f;
    
    [Tooltip("Layer-ul pentru teren (pe care se face plasarea).")]
    public LayerMask terrainLayer;

    private GameObject currentBlueprint;
    private Blueprint blueprint;
    private bool isBuildingMode = false;

    void Update()
    {
        // Toggle building mode cu tasta Space.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isBuildingMode)
                ExitBuildingMode();
            else
                EnterBuildingMode();
        }

        if (!isBuildingMode)
            return;

        // Actualizează poziția blueprint-ului în funcție de poziția mouse-ului pe teren.
        UpdateBlueprintPosition();

        // La click stânga, dacă plasarea este validă, plasăm clădirea.
        if (Input.GetMouseButtonDown(0) && currentBlueprint != null)
        {
            if (blueprint != null && blueprint.CanPlace)
            {
                PlaceBuilding();
            }
            else
            {
                Debug.Log("Plasare invalidă!");
            }
        }
    }

    /// <summary>
    /// Activează modul de construire și instanțiază blueprint-ul.
    /// </summary>
    void EnterBuildingMode()
    {
        isBuildingMode = true;

        if (currentBlueprint == null)
        {
            Vector3 initialPos = player.position + player.forward * Mathf.Min(maxPlacementDistance, 5f);
            currentBlueprint = Instantiate(blueprintPrefab, initialPos, Quaternion.identity);

            // Obținem componenta blueprint de pe blueprint.
            blueprint = currentBlueprint.GetComponent<Blueprint>();
            if (blueprint == null)
            {
                Debug.LogError("Prefab-ul blueprint lipsește componenta Blueprint!");
            }
        }
    }

    /// <summary>
    /// Dezactivează modul de construire și distruge blueprint-ul.
    /// </summary>
    void ExitBuildingMode()
    {
        isBuildingMode = false;
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
            blueprint = null;
        }
    }

    /// <summary>
    /// Actualizează poziția și rotația blueprint-ului pe baza punctului de pe teren unde se află mouse-ul.
    /// </summary>
    void UpdateBlueprintPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 desiredPosition = hit.point;
            Vector3 offset = desiredPosition - player.position;
            if (offset.magnitude > maxPlacementDistance)
            {
                offset = offset.normalized * maxPlacementDistance;
            }
            Vector3 clampedPosition = player.position + offset;

            // Actualizăm poziția și rotația blueprint-ului.
            currentBlueprint.transform.position = clampedPosition;
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            currentBlueprint.transform.rotation = surfaceRotation;
        }
    }

    /// <summary>
    /// Plasează clădirea la locația blueprint-ului și iese din modul de construire.
    /// </summary>
    void PlaceBuilding()
    {
        Instantiate(buildingPrefab, currentBlueprint.transform.position, currentBlueprint.transform.rotation);
        ExitBuildingMode();
    }
}
