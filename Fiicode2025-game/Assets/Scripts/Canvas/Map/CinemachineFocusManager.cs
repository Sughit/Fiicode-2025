using UnityEngine;
using Cinemachine;

public class CinemachineFocusManager : MonoBehaviour
{
    // Singleton static - ca să poți apela CinemachineFocusManager.Instance.FocusOn(...)
    public static CinemachineFocusManager Instance;

    [Header("Camera virtuală Cinemachine")]
    public CinemachineVirtualCamera vCam;

    // Salvăm ultimul nod pe care l-am focalizat
    private static Transform lastNodeTransform;

    
    public void FocusOn(Transform target)
    {
        
    }
}
