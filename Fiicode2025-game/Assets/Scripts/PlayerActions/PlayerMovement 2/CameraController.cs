using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Referințe")]
    [Tooltip("Transformul jucătorului")]
    [SerializeField] private Transform player;
    
    [Tooltip("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Tooltip("Transformul centrului planetei")]
    [SerializeField] private Transform planetCenter;

    [Header("Setări Cameră")]
    [Tooltip("Poziția camerei relativ la jucător (în spațiul local al jucătorului)")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -4);

    // Obiectul care va fi urmărit de Cinemachine
    private Transform cameraFollowTarget;

    void Start()
    {
        if (player == null || virtualCamera == null || planetCenter == null)
        {
            Debug.LogError("Setează player-ul, vcam-ul și planetCenter-ul în Inspector!");
            return;
        }

        // Creăm un obiect auxiliar pentru Follow
        GameObject followTarget = new GameObject("CameraFollowTarget");
        cameraFollowTarget = followTarget.transform;

        // Poziționăm target-ul inițial relativ la jucător
        cameraFollowTarget.position = player.position + player.TransformDirection(offset);

        // Setăm câmpurile Follow și LookAt ale Cinemachine Virtual Camera
        virtualCamera.Follow = cameraFollowTarget;
        virtualCamera.LookAt = player;
    }

    void LateUpdate()
    {
        if (player == null || cameraFollowTarget == null || planetCenter == null)
            return;

        // Actualizează poziția target-ului camerei
        Vector3 desiredPosition = player.position + player.TransformDirection(offset);
        cameraFollowTarget.position = desiredPosition;

        // Calculează vectorul 'up' folosind poziția jucătorului față de centrul planetei
        Vector3 planetUp = (player.position - planetCenter.position).normalized;

        // Obținem direcția de vizionare a jucătorului și eliminăm componenta pe planetUp
        Vector3 playerForward = player.forward;
        Vector3 correctedForward = Vector3.ProjectOnPlane(playerForward, planetUp).normalized;

        // Dacă jucătorul privește aproape exact în direcția planetUp (sau opusul), evităm diviziunea zero
        if (correctedForward.sqrMagnitude < 0.001f)
            correctedForward = playerForward;

        // Setăm rotația target-ului camerei folosind direcția corectată și vectorul up calculat
        cameraFollowTarget.rotation = Quaternion.LookRotation(correctedForward, planetUp);
    }
}
