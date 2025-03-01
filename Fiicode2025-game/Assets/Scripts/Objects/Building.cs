using UnityEngine;
using Cinemachine;

public class Building : Interactable
{
    [Header("Building Info")]
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private string buildingName = "Default Building Name";

    private CinemachineVirtualCamera interactionCamera;

    void Start()
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
    }

    public void EndInteraction()
    {
        if (interactionCamera != null)
        {
            interactionCamera.gameObject.SetActive(false);
            interactionCamera.LookAt = null;
            interactionCamera.Follow = null;
        }

        // Închide meniul clădirii curente
        CanvasManager.instance.CloseBuildingUI();
    }
}
