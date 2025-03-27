using UnityEngine;
using Cinemachine;

public class NewCameraController : MonoBehaviour
{
    [Header("Referințe")]
    [SerializeField] private Transform player;                     // Transformul jucătorului
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    [SerializeField] private Transform planetCenter;               // Centrul planetei
    [SerializeField] private Transform cameraFollowTarget;         // Target-ul pentru urmărirea camerei

    [Header("Setări Cameră")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -4); // Offset-ul camerei față de jucător

    void Start()
    {
        if (player == null || virtualCamera == null || planetCenter == null || cameraFollowTarget == null)
        {
            Debug.LogError("Setează player-ul, vcam-ul, planetCenter-ul și cameraFollowTarget-ul în Inspector!");
            return;
        }

        // Poziționează inițial target-ul camerei
        cameraFollowTarget.position = player.position + player.TransformDirection(offset);
        // Setează Follow și LookAt pentru Cinemachine
        virtualCamera.Follow = cameraFollowTarget;
        virtualCamera.LookAt = player;
    }

    void FixedUpdate()
    {
        if (player == null || cameraFollowTarget == null || planetCenter == null)
            return;

        // Actualizează poziția target-ului cu un Lerp pentru o tranziție lină
        Vector3 desiredPosition = player.position + player.TransformDirection(offset);
        cameraFollowTarget.position = Vector3.Lerp(cameraFollowTarget.position, desiredPosition, 10f * Time.deltaTime);

        // Calculează vectorul "up" al planetei, folosind poziția jucătorului și centrul planetei
        Vector3 planetUp = (player.position - planetCenter.position).normalized;

        // Rotația camerei: se calculează direcția de la target-ul camerei către jucător
        Vector3 lookDirection = (player.position - cameraFollowTarget.position).normalized;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, planetUp);

        // Se aplică Slerp pentru o tranziție lină a rotației
        cameraFollowTarget.rotation = Quaternion.Slerp(cameraFollowTarget.rotation, desiredRotation, 10f * Time.deltaTime);
    }
}
