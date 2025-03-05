using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text; // Pentru StringBuilder

[System.Serializable]
public class ResourceRequirement
{
    public string resourceName; 
    public int requiredAmount;
}

[System.Serializable]
public class DiscoveryRequirement
{
    public string discoveryName;
}

public class ResearchObject 
    : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Research Info")]
    [SerializeField] private string researchName;
    [SerializeField] private bool isCompleted = false;
    [SerializeField] private bool unlockBuilding = true;

    [Header("Requirements")]
    [SerializeField] private ResourceRequirement[] resourceRequirements; 
    [SerializeField] private DiscoveryRequirement[] discoveryRequirements;

    [Header("Unlock on Completion")]
    [SerializeField] private string[] unlockDiscoveries;
    [SerializeField] private GameObject[] unlockBuildings;
    [SerializeField] private GameObject[] nextResearch;

    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipText;

    private void Awake()
    {
        if (tooltipPanel != null) 
            tooltipPanel.SetActive(false);
    }

    void Start()
    {
        if(CheckIfAlreadyUnlocked())
        {
            foreach(GameObject building in unlockBuildings)
            {
                building.SetActive(true);
            }
        }
    }

    // ---------------------------------------------------------------------------------------
    // Logica de research
    // ---------------------------------------------------------------------------------------
    public void AttemptResearch()
    {
        if (CheckIfAlreadyUnlocked())
        {
            Debug.LogWarning($"Research '{researchName}' este deja completat!");
            return;
        }

        // 1. Verificăm descoperirile necesare
        if (!CheckDiscoveryRequirements())
        {
            Debug.LogWarning($"Nu ai toate descoperirile necesare pentru '{researchName}'!");
            return;
        }

        // 2. Verificăm resursele necesare
        if (!CheckResourceRequirements())
        {
            Debug.LogWarning($"Nu ai suficiente resurse pentru '{researchName}'!");
            return;
        }

        // 3. Consumăm resursele
        ConsumeResources();

        // 4. Finalizăm research-ul
        CompleteResearch();
    }

    private bool CheckIfAlreadyUnlocked()
    {
        foreach(string disc in unlockDiscoveries)
        {
            if(!PlayerScanInventory.instance.IsUnlocked(disc))
            {
                isCompleted = false;
                return false;
            }
        }

        isCompleted = true;
        return true;
    }

    private bool CheckDiscoveryRequirements()
    {
        if (discoveryRequirements == null || discoveryRequirements.Length == 0) 
            return true;

        foreach (var req in discoveryRequirements)
        {
            bool unlocked = PlayerScanInventory.instance.IsUnlocked(req.discoveryName);
            if (!unlocked)
                return false;
        }
        return true;
    }

    private bool CheckResourceRequirements()
    {
        if (resourceRequirements == null || resourceRequirements.Length == 0) 
            return true;

        foreach (var req in resourceRequirements)
        {
            int currentAmount = GetResourceAmount(req.resourceName);
            if (currentAmount < req.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    private void ConsumeResources()
    {
        if (resourceRequirements == null) return;

        foreach (var req in resourceRequirements)
        {
            PlayerInventory.instance.RemoveItem(req.resourceName, req.requiredAmount);
        }
    }

    private void CompleteResearch()
    {
        isCompleted = true;
        Debug.Log($"Research '{researchName}' a fost finalizat!");

        if (unlockDiscoveries != null && unlockDiscoveries.Length > 0)
        {
            foreach (string discName in unlockDiscoveries)
            {
                PlayerScanInventory.instance.Unlock(discName);
            }

            foreach (GameObject research in nextResearch)
            {
                research.SetActive(true);
            }
        }

        if(unlockBuilding)
        {
            foreach(GameObject building in unlockBuildings)
            {
                building.SetActive(true);
            }
        }
    }

    private int GetResourceAmount(string resourceName)
    {
        var so = PlayerInventory.instance.inventory;
        if (so == null) return 0;

        var type = so.GetType();
        var field = type.GetField(resourceName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(so);
        }

        Debug.LogWarning($"Nu există resursa '{resourceName}' ca int în Inventory!");
        return 0;
    }

    // ---------------------------------------------------------------------------------------
    // Partea de UI Tooltip (hover)
    // ---------------------------------------------------------------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipText.text = BuildTooltipText();
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    // ---------------------------------------------------------------------------------------
    // Opțional: click direct prin IPointerClickHandler
    // ---------------------------------------------------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        // Apel la AttemptResearch pe click stânga, dacă vrei
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            AttemptResearch();
        }
    }

    // ---------------------------------------------------------------------------------------
    // Construim textul de tooltip
    // ---------------------------------------------------------------------------------------
    private string BuildTooltipText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>{researchName}</b>");

        if (CheckIfAlreadyUnlocked())
        {
            sb.AppendLine("Status: <color=green>COMPLETED</color>");
            return sb.ToString();
        }
        else
        {
            sb.AppendLine("Status: In progress...");
            sb.AppendLine();
        }

        // Verificăm dacă toate descoperirile sunt deblocate
        bool allDiscoveriesUnlocked = CheckDiscoveryRequirements();

        if (!allDiscoveriesUnlocked)
        {
            // Afișăm DOAR descoperirile (pentru că n-am deblocat toate)
            sb.AppendLine("Discovery Requirements:");
            if (discoveryRequirements != null && discoveryRequirements.Length > 0)
            {
                foreach (var req in discoveryRequirements)
                {
                    bool unlocked = PlayerScanInventory.instance.IsUnlocked(req.discoveryName);
                    string colorTag = unlocked ? "green" : "red";
                    sb.AppendLine($"• {req.discoveryName} -> <color={colorTag}>{(unlocked ? "Unlocked" : "Locked")}</color>");
                }
            }
            else
            {
                sb.AppendLine("Nici o descoperire necesară.");
            }
        }
        else
        {
            // Toate descoperirile necesare au fost deblocate => afișăm DOAR resursele
            sb.AppendLine("Resource Requirements:");
            if (resourceRequirements != null && resourceRequirements.Length > 0)
            {
                foreach (var resReq in resourceRequirements)
                {
                    int currentAmount = GetResourceAmount(resReq.resourceName);
                    bool enough = (currentAmount >= resReq.requiredAmount);
                    string colorTag = enough ? "green" : "red";
                    sb.AppendLine(
                        $"• {resReq.resourceName}: <color={colorTag}>{currentAmount}/{resReq.requiredAmount}</color>"
                    );
                }
            }
            else
            {
                sb.AppendLine("Nu sunt resurse necesare.");
            }
        }

        return sb.ToString();
    }
}
