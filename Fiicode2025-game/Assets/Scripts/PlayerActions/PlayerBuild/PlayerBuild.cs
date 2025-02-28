using UnityEngine;
using System.Collections;

public class PlayerBuild : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject buildingPrefab;
    public GameObject blueprintPrefab;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float maxPlacementDistance = 10f;
    [SerializeField] private LayerMask terrainLayer;

    private GameObject currentBlueprint;
    private Blueprint blueprint;
    private bool isBuildingMode = false;

    // Expose the building mode state so other scripts can check it.
    public static bool IsBuildingModeActive { get; private set; }
    // Flag to indicate a building was just placed.
    public static bool JustPlacedBuilding { get; private set; } = false;

    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnOpenBuildingMenu += ToggleBuildingMode;
        }
    }

    void Update()
    {
        if (!isBuildingMode)
            return;

        UpdateBlueprintPosition();

        // Confirm building placement on left-click.
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

    void ToggleBuildingMode()
    {
        if (isBuildingMode)
            ExitBuildingMode();
        else
            EnterBuildingMode();
    }

    void EnterBuildingMode()
    {
        isBuildingMode = true;
        IsBuildingModeActive = true; // Set static flag
        if (currentBlueprint == null)
        {
            Vector3 initialPos = player.position + player.forward * Mathf.Min(maxPlacementDistance, 5f);
            currentBlueprint = Instantiate(blueprintPrefab, initialPos, Quaternion.identity);

            blueprint = currentBlueprint.GetComponent<Blueprint>();
            if (blueprint == null)
            {
                Debug.LogError("Prefab-ul blueprint lipsește componenta Blueprint!");
            }
        }
    }

    void ExitBuildingMode()
    {
        isBuildingMode = false;
        IsBuildingModeActive = false; // Clear static flag
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
            blueprint = null;
        }
    }

    void UpdateBlueprintPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 desiredPosition = hit.point;
            Vector3 offset = desiredPosition - player.position;
            if (offset.magnitude > maxPlacementDistance)
                offset = offset.normalized * maxPlacementDistance;
            Vector3 clampedPosition = player.position + offset;

            currentBlueprint.transform.position = clampedPosition;
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            currentBlueprint.transform.rotation = surfaceRotation;
        }
    }

    void PlaceBuilding()
    {
        Instantiate(buildingPrefab, currentBlueprint.transform.position, currentBlueprint.transform.rotation);
        // Set the flag so that PlayerAttack can ignore the input for a short duration.
        JustPlacedBuilding = true;
        StartCoroutine(ResetJustPlacedBuilding());
        ExitBuildingMode();
    }

    IEnumerator ResetJustPlacedBuilding()
    {
        // Wait a bit longer than just one frame (e.g., 0.2 seconds) before resetting.
        yield return new WaitForSeconds(0.2f);
        JustPlacedBuilding = false;
    }
}
