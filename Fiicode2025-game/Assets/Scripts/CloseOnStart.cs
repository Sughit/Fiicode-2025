using UnityEngine;

public class CloseOnStart : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
