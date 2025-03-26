using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atașează acest script fiecărui nod din scenă.
/// Va stoca în Inspector listele de părinți și copii (ca GameObject-uri/MapNodeComponent).
/// </summary>
public class MapNodeComponent : MonoBehaviour
{
    [Header("Părinți și copii în hartă (doar pentru vizualizare)")]
    public List<MapNodeComponent> parents = new List<MapNodeComponent>();
    public List<MapNodeComponent> children = new List<MapNodeComponent>();

    // Poți avea și alte informații aici (tip, scor, etc.)
}
