using UnityEngine;
using System.Collections;

[System.Serializable]
public class BuildingOption
{
    public string optionName;           // Numele opțiunii (pentru referință)
    public GameObject blueprintPrefab;  // Blueprint-ul asociat opțiunii
    public GameObject buildingPrefab;   // Prefabul clădirii care va fi instanțiată
}

public class PlayerBuild : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    
    [Header("UI")]
    public GameObject buildingMenu; // Meniul de construire, care va fi activat/dezactivat

    [Header("Settings")]
    [SerializeField] private float maxPlacementDistance = 3f;
    [SerializeField] private LayerMask terrainLayer;

    [Header("Building Options")]
    public BuildingOption[] buildingOptions; // Lista de opțiuni disponibile în meniu

    private GameObject currentBlueprint;
    private Blueprint blueprint;
    // Prefabul clădirii ce va fi instanțiată la plasare, selectat din opțiunile din meniu
    private GameObject selectedBuildingPrefab;

    private bool isBuildingMode = false;
    // Variabila statică publică care indică dacă modul de construire este activ
    public static bool IsBuildingModeActive { get; private set; } = false;
    // Flag pentru a indica că a fost plasată o clădire
    public static bool JustPlacedBuilding { get; private set; } = false;

    void Start()
    {
        // Dacă un alt sistem (ex: PlayerController) deschide meniul, se poate abona la eveniment
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnOpenBuildingMenu += ToggleBuildingMode;
        }
    }

    void Update()
    {
        if (!isBuildingMode)
            return;

        // Dacă există un blueprint activ, se actualizează poziția lui
        if (currentBlueprint != null)
        {
            UpdateBlueprintPosition();
        }

        // Se poate confirma plasarea cu click stânga (opțional, dacă blueprint-ul este plasabil)
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
    /// Comută modul de construire și afișează sau ascunde meniul de construire.
    /// </summary>
    void ToggleBuildingMode()
    {
        if (isBuildingMode)
            ExitBuildingMode();
        else
            EnterBuildingMode();
    }

    /// <summary>
    /// Intră în modul de construire și deschide meniul.
    /// </summary>
    void EnterBuildingMode()
    {
        isBuildingMode = true;
        IsBuildingModeActive = true;
        if (buildingMenu != null)
        {
            buildingMenu.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Iese complet din modul de construire și curăță blueprint-ul.
    /// </summary>
    void ExitBuildingMode()
    {
        isBuildingMode = false;
        IsBuildingModeActive = false;
        if (buildingMenu != null)
        {
            buildingMenu.SetActive(false);
        }
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
            blueprint = null;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Funcție publică apelată de butoanele din meniul de construire.
    /// Primește un index pentru a selecta opțiunea de construire din array-ul buildingOptions.
    /// Astfel se vor seta automat blueprint-ul și prefab-ul corespunzător.
    /// După selectare, meniul se închide.
    /// </summary>
    /// <param name="index">Indexul opțiunii din array-ul buildingOptions.</param>
    public void SpawnBuildingOption(int index)
    {
        if (index < 0 || index >= buildingOptions.Length)
        {
            Debug.LogError("Index de opțiune invalid!");
            return;
        }

        BuildingOption option = buildingOptions[index];

        // Dacă există deja un blueprint, îl distrugem
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
            blueprint = null;
        }

        if (option.blueprintPrefab == null || option.buildingPrefab == null)
        {
            Debug.LogError("Blueprint sau building prefab nu sunt setate pentru opțiunea: " + option.optionName);
            return;
        }

        // Salvăm referința clădirii selectate
        selectedBuildingPrefab = option.buildingPrefab;

        // Instanțiem blueprint-ul în fața jucătorului
        Vector3 initialPos = player.position + player.forward * Mathf.Min(maxPlacementDistance, 5f);
        currentBlueprint = Instantiate(option.blueprintPrefab, initialPos, Quaternion.identity);
        blueprint = currentBlueprint.GetComponent<Blueprint>();
        if (blueprint == null)
        {
            Debug.LogError("Prefab-ul blueprint lipsește componenta Blueprint!");
        }

        // Închidem meniul de construire după selectare
        if (buildingMenu != null)
        {
            buildingMenu.SetActive(false);
        }
    }

    /// <summary>
    /// Actualizează poziția blueprint-ului în funcție de poziția mouse-ului și teren.
    /// </summary>
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

    /// <summary>
    /// Plasează clădirea la poziția blueprint-ului și redeschide meniul de construire.
    /// Modul de construire rămâne activ pentru noi selecții.
    /// </summary>
    void PlaceBuilding()
    {
        if (selectedBuildingPrefab == null)
        {
            Debug.LogError("Nu a fost selectat niciun prefab de clădire!");
            return;
        }

        Instantiate(selectedBuildingPrefab, currentBlueprint.transform.position, currentBlueprint.transform.rotation);
        JustPlacedBuilding = true;
        StartCoroutine(ResetJustPlacedBuilding());

        // După plasare, ștergem blueprint-ul și redeschidem meniul pentru o nouă selecție.
        FinishBuildingPlacement();
    }

    IEnumerator ResetJustPlacedBuilding()
    {
        yield return new WaitForSeconds(0.2f);
        JustPlacedBuilding = false;
    }

    /// <summary>
    /// Elimină blueprint-ul curent și redeschide meniul de construire, fără a ieși din modul de construire.
    /// </summary>
    void FinishBuildingPlacement()
    {
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
            blueprint = null;
        }

        if (buildingMenu != null)
        {
            buildingMenu.SetActive(true);
        }
    }
}
