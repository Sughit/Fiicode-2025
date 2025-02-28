using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject buildingPrefab;
    public GameObject blueprintPrefab;
    public Transform player;

    [Header("Settings")]
    public float maxPlacementDistance = 10f;
    public LayerMask terrainLayer;

    private GameObject currentBlueprint;
    private Blueprint blueprint;
    private bool isBuildingMode = false;

    void Awake()
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

        if(Input.GetMouseButtonDown(0) && currentBlueprint != null)
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
        ExitBuildingMode();
    }
}
