using UnityEngine;

public class MapParent : MonoBehaviour
{
    public MapNode currentNode;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
