using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private GameObject vcam1, vcam2;
    public static CameraTransition instance;

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }

    void Start()
    {
        vcam1.SetActive(false);
        vcam2.SetActive(true);
    }

}
