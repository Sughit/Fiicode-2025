using UnityEngine;
using Cinemachine;

public class Building : Interactable
{
    [SerializeField] private GameObject infoMenu;
    private CinemachineVirtualCamera interactionCamera;

    public override void Interact()
    {
        interactionCamera = PlayerController.instance.interactionCam;
        if (interactionCamera != null)
        {
            // Set the camera to focus on this building.
            interactionCamera.LookAt = transform;
            interactionCamera.Follow = transform;
            
            // Activate the interaction camera.
            interactionCamera.gameObject.SetActive(true);
        }
        
        // Activate the info panel (assumed to be positioned on the right side of the screen).
        if (infoMenu != null)
        {
            infoMenu.SetActive(true);
        }
    }
    
    // Call this method to end the interaction and revert to the main player camera.
    public void EndInteraction(Transform playerCameraTarget)
    {
        if (interactionCamera != null)
        {
            // Deactivate the interaction camera.
            interactionCamera.gameObject.SetActive(false);
            // Optionally clear the LookAt and Follow targets.
            interactionCamera.LookAt = null;
            interactionCamera.Follow = null;
        }
        
        if (infoMenu != null)
        {
            infoMenu.SetActive(false);
        }
        
        // Optionally, you can reassign the main player camera here if needed.
        // For example:
        // CinemachineVirtualCamera mainVCam = ...;
        // mainVCam.LookAt = playerCameraTarget;
        // mainVCam.Follow = playerCameraTarget;
    }
}
