using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook cinemachineCamera; // Assign the FreeLook Camera in the Inspector
    [SerializeField] private Transform player;  // Assign the player transform
    [SerializeField] private Transform planet;  // Assign the planet transform
    [SerializeField] private float gravityAlignmentSpeed = 5f;

    void LateUpdate()
    {
        AlignCameraWithGravity();
    }

    void AlignCameraWithGravity()
    {
        if (player == null || planet == null || cinemachineCamera == null) return;

        // Get player's up direction (gravity-aligned)
        Vector3 playerUp = (player.position - planet.position).normalized;

        // Align Cinemachine camera up direction with planet gravity
        cinemachineCamera.m_YAxis.Value = 0.5f;  // Keeps the camera centered vertically
        cinemachineCamera.m_XAxis.m_MaxSpeed = 300f; // Adjust rotation speed

        Quaternion gravityAlignment = Quaternion.FromToRotation(cinemachineCamera.transform.up, playerUp) * cinemachineCamera.transform.rotation;
        cinemachineCamera.transform.rotation = Quaternion.Slerp(cinemachineCamera.transform.rotation, gravityAlignment, Time.deltaTime * gravityAlignmentSpeed);
    }
}
