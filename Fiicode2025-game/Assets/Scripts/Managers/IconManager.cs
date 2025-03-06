using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ResourceIcon
{
    public string name;
    public Sprite icon;
}

public class IconManager : MonoBehaviour
{
    public static IconManager instance;

    [SerializeField] private List<ResourceIcon> resourceIcons;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public Sprite GetResourceIcon(string resourceName)
    {
        foreach (ResourceIcon icon in resourceIcons)
        {
            if (icon.name == resourceName) return icon.icon;
        }
        return null;
    }   
}
