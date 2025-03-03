using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Referințe")]
    [Tooltip("Transformul jucătorului")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform gfx;
    
    [Tooltip("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Tooltip("Transformul centrului planetei")]
    [SerializeField] private Transform planetCenter;

    [Header("Setări Cameră")]
    [Tooltip("Poziția camerei relativ la jucător (în spațiul local al jucătorului)")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -4);

    // Obiectul care va fi urmărit de Cinemachine
    private Transform cameraFollowTarget;

    // Unghiul de rotație pentru camera, modificat de mouse.
    private float yawAngle = 0f; // Rotație pe axa orizontală (Yaw)
    private float pitchAngle = 0f; // Rotație pe axa verticală (Pitch)
    [Tooltip("Viteză de rotație cu mouse-ul pe orizontală")]
    [SerializeField] private float rotationSpeed = 3f;
    [Tooltip("Viteză de rotație cu mouse-ul pe verticală")]
    [SerializeField] private float verticalRotationSpeed = 2f;

    [Header("Limite Rotație Verticală")]
    [Tooltip("Limita minimă pentru rotația pe axa Y (în grade)")]
    [SerializeField] private float minPitch = -40f;
    [Tooltip("Limita maximă pentru rotația pe axa Y (în grade)")]
    [SerializeField] private float maxPitch = 80f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;   
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

        // 1) Actualizăm poziția target-ului
        Vector3 desiredPosition = player.position + player.TransformDirection(offset);
        cameraFollowTarget.position = Vector3.Lerp(
            cameraFollowTarget.position, 
            player.position, 
            10f * Time.deltaTime
        );

        // 2) Calculăm planetUp
        Vector3 planetUp = (player.position - planetCenter.position).normalized;

        // 3) Forward-ul jucătorului, proiectat pe planul perpendicular pe planetUp
        Vector3 playerForward = player.forward;
        Vector3 correctedForward = Vector3.ProjectOnPlane(playerForward, planetUp).normalized;
        if (correctedForward.sqrMagnitude < 0.001f)
            correctedForward = playerForward;

        // 4) Citim mouseX și actualizăm yaw-ul (rotirea pe orizontală)
        float mouseX = Input.GetAxis("Mouse X");
        yawAngle += mouseX * rotationSpeed;

        // 5) Citim mouseY și actualizăm pitch-ul (rotirea pe verticală) cu limite
        float mouseY = Input.GetAxis("Mouse Y");
        pitchAngle += mouseY * verticalRotationSpeed;  // scădem pentru a obține o mișcare corectă (mărește unghiul când miști în sus)
        pitchAngle = Mathf.Clamp(pitchAngle, minPitch, maxPitch); // Aplicăm limitele

        // 6) Calculăm rotația finală pe orizontală și verticală
        Quaternion yawRotation = Quaternion.AngleAxis(yawAngle, planetUp); // Rotire pe axa orizontală
        Quaternion pitchRotation = Quaternion.AngleAxis(pitchAngle, transform.right); // Rotire pe axa verticală

        // 7) Calculăm forward-ul final rotit
        Vector3 finalForward = yawRotation * pitchRotation * correctedForward;

        // 8) Construim desiredRotation și îl aplicăm cu un Slerp pentru a face tranzițiile mai line
        Quaternion desiredRotation = Quaternion.LookRotation(finalForward, planetUp);
        cameraFollowTarget.rotation = Quaternion.Slerp(
            cameraFollowTarget.rotation,
            desiredRotation,
            10f * Time.deltaTime
        );
    }
}
