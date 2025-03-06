using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Building : Interactable
{
    [Header("Building Info")]
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private string buildingName = "Default Building Name";

    private CinemachineVirtualCamera interactionCamera;

    protected virtual void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnCancelInteract += EndInteraction;
        }
    }

    public override void Interact()
    {
        // Activează camera de interacțiune (dacă vrei să focalizezi pe clădire).
        interactionCamera = PlayerController.instance.interactionCam;
        if (interactionCamera != null)
        {
            interactionCamera.LookAt = transform;
            interactionCamera.Follow = transform;
            interactionCamera.gameObject.SetActive(true);
        }

        // Apelează CanvasManager pentru a deschide meniul corespunzător acestei clădiri
        CanvasManager.instance.OpenBuildingUI(buildingType, buildingName);
        CanvasManager.instance.SetInteractionGO(this.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public virtual void EndInteraction()
    {
        if (interactionCamera != null)
        {
            interactionCamera.gameObject.SetActive(false);
            interactionCamera.LookAt = null;
            interactionCamera.Follow = null;
        }

        // Închide meniul clădirii curente
        CanvasManager.instance.CloseBuildingUI();
        CanvasManager.instance.SetInteractionGO(null);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetMiningIcons(Sprite icon)
    {
        CanvasManager.instance.SetMiningIcons(icon);
    }

    public void SetCraftingIcons(Sprite iconInput, Sprite iconOutput)
    {
        CanvasManager.instance.SetCraftingIcons(iconInput, iconOutput);
    }
}
